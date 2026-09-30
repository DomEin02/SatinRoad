using System.ComponentModel.DataAnnotations;
using API.Dtos;
using Infa;
using LinqToDB;
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
    
    [HttpPost(nameof(Create))]
    public ProductResponse Create([FromBody] ProductCreateRequest request)
    {
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
            VendorId = request.VendorId,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Insert(product);
        return new ProductResponse(product);
    }
    
    [HttpPatch(nameof(Update))]
    public ProductResponse Update([FromBody] ProductUpdateRequest request)
    {
        var product = db.Products().FirstOrDefault(p => p.Id == request.Id)
                     ?? throw new KeyNotFoundException("Product not found.");

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
    
    [HttpDelete(nameof(Delete))]
    public void Delete([FromQuery] string id)
    {
        var product = db.Products().FirstOrDefault(p => p.Id == id)
                     ?? throw new KeyNotFoundException("Product not found.");

        db.Delete(product);
    }
}