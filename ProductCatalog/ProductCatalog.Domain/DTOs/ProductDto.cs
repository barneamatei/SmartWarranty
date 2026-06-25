namespace ProductCatalog.Domain.DTOs;

public class ProductDto
{
    public Guid ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }

    public Guid? UserId { get; set; }

    public string Status { get; set; } = string.Empty;
}

