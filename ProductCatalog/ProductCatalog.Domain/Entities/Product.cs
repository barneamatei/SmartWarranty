namespace ProductCatalog.Domain.Entities;

public class Product
{
    public Guid ProductId { get; private set; }

    public string Name { get; private set; }

    public string Brand { get; private set; }

    public string Model { get; private set; }

    public Guid CategoryId { get; private set; }

    public ProductStatus Status { get; private set; }

    protected Product()
    {
        Name = string.Empty;
        Brand = string.Empty;
        Model = string.Empty;
    }

    public Product(Guid productId, string name, string brand, string model, Guid categoryId)
    {
        ProductId = productId;
        Name = name;
        Brand = brand;
        Model = model;
        CategoryId = categoryId;
        Status = ProductStatus.Active;
    }

    public void UpdateDetails(string name, string brand, string model)
    {
        Name = name;
        Brand = brand;
        Model = model;
    }

    public void ChangeCategory(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
    }
}
