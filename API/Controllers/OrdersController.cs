using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API.Dtos;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
public class OrdersController(SatinRoadDatabase db) : ControllerBase
{
    [Authorize]
    [HttpPost(nameof(Buy))]
    public OrderResponse Buy([FromBody] BuyRequest request)
    {
        // The buyer is whoever is logged in, from token not request.
        var buyerId = CurrentUserId();
        
        if (request.Quantity < 1)
            throw new ValidationException("Quantity must be at least 1.");

        var product = db.Products().FirstOrDefault(p => p.Id == request.ProductId)
                      ?? throw new KeyNotFoundException("Product not found.");

        if (product.VendorId == buyerId)
            throw new InvalidOperationException("You cannot buy your own product.");

        if (product.StockCount < request.Quantity)
            throw new InvalidOperationException("Not enough in stock.");

        // 2) Build the order
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            BuyerId = buyerId,
            VendorId = product.VendorId,
            ProductId = product.Id,
            Quantity = request.Quantity,
            UnitPriceDkk = product.PriceDkk,
            DiscountApplied = false,
            TotalPriceDkk = product.PriceDkk * request.Quantity,
            CreatedAtUtc = DateTime.UtcNow
        };

        // 3) Save both changes together: lower the stock and store the order
        using var transaction = db.BeginTransaction();
        product.StockCount -= request.Quantity;
        db.Update(product);
        db.Insert(order);
        transaction.Commit();

        // 4) Map and return
        return new OrderResponse(order);
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