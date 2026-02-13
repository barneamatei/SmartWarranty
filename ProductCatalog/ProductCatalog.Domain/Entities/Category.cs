namespace ProductCatalog.Domain.Entities;

public class Category
{
    public Guid CategoryId { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    protected Category()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public Category(Guid categoryId, string name, string description)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description ?? string.Empty;
    }

    public void Rename(string name)
    {
        Name = name;
    }

    public void UpdateDescription(string description)
    {
        Description = description ?? string.Empty;
    }
}
