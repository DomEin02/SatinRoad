namespace API.Dtos;

public record CategoryCreateRequest(string Name);

public record CategoryUpdateRequest(string Id, string Name);

public class CategoryResponse
{
    public string Id { get; }
    public string Name { get; }

    public CategoryResponse(Infa.Category category)
    {
        Id = category.Id;
        Name = category.Name;
    }
}