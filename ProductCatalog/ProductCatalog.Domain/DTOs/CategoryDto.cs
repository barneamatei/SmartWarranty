namespace ProductCatalog.Domain.DTOs;

public class CategoryDto
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
}

