using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Domain.DTOs;

public class UpdateProductRequest
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product brand is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Product brand must be between 1 and 100 characters.")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product model is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Product model must be between 1 and 100 characters.")]
    public string Model { get; set; } = string.Empty;
}

