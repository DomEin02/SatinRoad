using System.ComponentModel.DataAnnotations;
using API.Dtos;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
public class CategoriesController(SatinRoadDatabase db) : ControllerBase
{
    [HttpGet(nameof(GetAll))]
    public List<CategoryResponse> GetAll()
    {
        // Hent alle kategorier, sorter dem alfabetisk, og pak hver én ind i et svar-objekt.
        return db.Categories()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(c))
            .ToList();
    }
    
    [HttpPost(nameof(Create))]
    public CategoryResponse Create([FromBody] CategoryCreateRequest request)
    {
        // Et tomt eller kun-mellemrum navn er ikke gyldigt.
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException();

        // Byg en ny kategori op med et unikt ID.
        var category = new Category
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name
        };

        db.Insert(category);

        return new CategoryResponse(category);
    }
    
    [HttpPatch(nameof(Update))]
    public CategoryResponse Update([FromBody] CategoryUpdateRequest request)
    {
        // Find kategorien; findes den ikke, giver det ingen mening at fortsætte.
        var category = db.Categories().FirstOrDefault(c => c.Id == request.Id)
            ?? throw new KeyNotFoundException();

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException();

        // Ret navnet i hukommelsen, og gem det til databasen.
        category.Name = request.Name;
        db.Update(category);

        return new CategoryResponse(category);
    }
    
    [HttpDelete(nameof(Delete))]
    public void Delete([FromQuery] string id)
    {
        var category = db.Categories().FirstOrDefault(c => c.Id == id)
            ?? throw new KeyNotFoundException();

        // Sikkerhedstjek: findes der stadig et produkt, der bruger denne kategori?
        var hasProducts = db.Products().Any(p => p.CategoryId == id);
        if (hasProducts)
            throw new InvalidOperationException();

        db.Delete(category);
    }
}