using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API.Dtos;
using API.Services;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
public class OrdersController(SatinRoadDatabase db, IRandomProvider random) : ControllerBase
{
    [Authorize]
    [HttpPost(nameof(Buy))]
    public OrderResponse Buy([FromBody] BuyRequest request)
    {
        // The buyer is whoever is logged in, from token, never from the request.
        var buyerId = CurrentUserId();

        // 1) Unhappy path
        if (request.Quantity < 1)
            throw new ValidationException("Quantity must be at least 1.");

        var product = db.Products().FirstOrDefault(p => p.Id == request.ProductId)
                      ?? throw new KeyNotFoundException("Product not found.");

        if (product.VendorId == buyerId)
            throw new InvalidOperationException("You cannot buy your own product.");

        if (product.StockCount < request.Quantity)
            throw new InvalidOperationException("Not enough in stock.");

        // 2) Loyalty discount: count this buyer's earlier orders with this vendor
        var vendorId = product.VendorId;
        var previousOrders = db.Orders().Count(o => o.BuyerId == buyerId && o.VendorId == vendorId);
        var discountApplies = DiscountCalculator.QualifiesForDiscount(previousOrders);

        // 3) Build the order. The price is copied now, because the vendor may change it later.
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            BuyerId = buyerId,
            VendorId = vendorId,
            ProductId = product.Id,
            Quantity = request.Quantity,
            UnitPriceDkk = product.PriceDkk,
            DiscountApplied = discountApplies,
            TotalPriceDkk = DiscountCalculator.TotalPrice(product.PriceDkk, request.Quantity, discountApplies),
            CreatedAtUtc = DateTime.UtcNow
        };

        // 4) FBI check: every purchase has a 1% chance that the buyer is FBI
        var vendorWasRaided = FbiRaid.IsRaid(random.NextDouble());

        // 5) Save everything together: the stock, the order and, on a raid, the shutdown
        using var transaction = db.BeginTransaction();
        product.StockCount -= request.Quantity;
        db.Update(product);
        db.Insert(order);

        if (vendorWasRaided)
        {
            // The vendor is shut down permanently...
            db.Users()
                .Where(u => u.Id == vendorId)
                .Set(u => u.IsShutDown, true)
                .Update();

            // ...and all their products are removed
            db.Products()
                .Where(p => p.VendorId == vendorId)
                .Delete();
        }

        transaction.Commit();

        // 6) Map and return
        return new OrderResponse(order, vendorWasRaided);
    }

    [Authorize]
    [HttpGet(nameof(GetMine))]
    public List<OrderResponse> GetMine()
    {
        var buyerId = CurrentUserId();

        var orders = db.Orders()
            .Where(o => o.BuyerId == buyerId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToList();

        return orders.Select(o => new OrderResponse(o)).ToList();
    }

    private string CurrentUserId()
    {
        return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? throw new UnauthorizedAccessException("You must be logged in.");
    }
}