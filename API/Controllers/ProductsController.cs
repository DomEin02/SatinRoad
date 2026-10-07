using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API.Dtos;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
public class ProductsController(SatinRoadDatabase db) : ControllerBase
{
    [HttpGet(nameof(GetAll))]
    public List<ProductResponse> GetAll()
    {
        return db.Products()
            .OrderBy(p => p.Name)
            .Select(p => new ProductResponse(p))
            .ToList();
    }

    [Authorize]
    [HttpPost(nameof(Create))]
    public ProductResponse Create([FromBody] ProductCreateRequest request)
    {
        // The vendor is whoever is logged in, from the token, never from the request.
        var vendorId = CurrentUserId();

        // A vendor that has been shut down (or no longer exists) cannot sell anything,
        // even if their old token is still valid.
        var vendor = db.Users().FirstOrDefault(u => u.Id == vendorId);
        if (vendor == null || vendor.IsShutDown)
            throw new UnauthorizedAccessException("This account cannot create products.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name is required.");
        if (request.PriceDkk < 0)
            throw new ValidationException("Price cannot be negative.");
        if (request.StockCount < 0)
            throw new ValidationException("Stock count cannot be negative.");

        // Make sure the category actually exists before we attach a product to it
        var category = db.Categories().FirstOrDefault(c => c.Id == request.CategoryId)
                       ?? throw new KeyNotFoundException("Category not found.");

        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            PriceDkk = request.PriceDkk,
            StockCount = request.StockCount,
            CategoryId = category.Id,
            VendorId = vendorId,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Insert(product);
        return new ProductResponse(product);
    }

    [Authorize]
    [HttpPatch(nameof(Update))]
    public ProductResponse Update([FromBody] ProductUpdateRequest request)
    {
        var product = GetOwnProduct(request.Id);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name is required.");
        if (request.PriceDkk < 0)
            throw new ValidationException("Price cannot be negative.");
        if (request.StockCount < 0)
            throw new ValidationException("Stock count cannot be negative.");

        var category = db.Categories().FirstOrDefault(c => c.Id == request.CategoryId)
                       ?? throw new KeyNotFoundException("Category not found.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.PriceDkk = request.PriceDkk;
        product.StockCount = request.StockCount;
        product.CategoryId = category.Id;

        db.Update(product);
        return new ProductResponse(product);
    }

    [Authorize]
    [HttpDelete(nameof(Delete))]
    public void Delete([FromQuery] string id)
    {
        var product = GetOwnProduct(id);

        db.Delete(product);
    }

    [HttpGet(nameof(GetByVendor))]
    public List<ProductResponse> GetByVendor([FromQuery] string vendorId)
    {
        var products = db.Products()
            .Where(p => p.VendorId == vendorId)
            .OrderBy(p => p.Name)
            .ToList();

        return products.Select(p => new ProductResponse(p)).ToList();
    }

    [Authorize]
    [HttpPatch(nameof(AdjustStock))]
    public ProductResponse AdjustStock([FromBody] StockAdjustmentRequest request)
    {
        var product = GetOwnProduct(request.ProductId);

        var newStock = product.StockCount + request.ChangeBy;
        if (newStock < 0)
            throw new ValidationException("Stock cannot go below zero.");

        product.StockCount = newStock;
        db.Update(product);
        return new ProductResponse(product);
    }

    // Finds a product and makes sure it belongs to the logged-in user
    private Product GetOwnProduct(string productId)
    {
        var userId = CurrentUserId();

        var product = db.Products().FirstOrDefault(p => p.Id == productId)
                      ?? throw new KeyNotFoundException("Product not found.");

        if (product.VendorId != userId)
            throw new UnauthorizedAccessException("You can only change your own products.");

        return product;
    }

    private string CurrentUserId()
    {
        return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? throw new UnauthorizedAccessException("You must be logged in.");
    }
}