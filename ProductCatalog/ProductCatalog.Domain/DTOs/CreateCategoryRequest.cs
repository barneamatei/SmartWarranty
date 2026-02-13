using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Domain.DTOs;

public class CreateCategoryRequest
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Category description must not exceed 500 characters.")]
    public string Description { get; set; } = string.Empty;
}

