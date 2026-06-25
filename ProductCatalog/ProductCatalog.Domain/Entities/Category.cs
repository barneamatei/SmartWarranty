namespace ProductCatalog.Domain.Entities;

public class Category
{
    public Guid CategoryId { get; private set; }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Guid? UserId { get; private set; }

    protected Category()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    public Category(Guid categoryId, string name, string description, Guid? userId = null)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description ?? string.Empty;
        UserId = userId;
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
