using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Service;
using ProductCatalog.Domain.DTOs;

namespace ProductCatalog.Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductCatalogService _productCatalogService;

    public ProductController(ProductCatalogService productCatalogService)
    {
        _productCatalogService = productCatalogService ?? throw new ArgumentNullException(nameof(productCatalogService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var productDto = await _productCatalogService.CreateProductAsync(
            request.Name,
            request.Brand,
            request.Model,
            request.CategoryId,
            request.UserId);

        return CreatedAtAction(nameof(GetProductById), new { id = productDto.ProductId }, productDto);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var productDto = await _productCatalogService.UpdateProductAsync(
            id,
            request.Name,
            request.Brand,
            request.Model);

        return Ok(productDto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts([FromQuery] Guid? userId)
    {
        var productDtos = userId.HasValue
            ? await _productCatalogService.GetProductsByUserIdAsync(userId.Value)
            : await _productCatalogService.GetAllProductsAsync();

        return Ok(productDtos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id)
    {
        var productDto = await _productCatalogService.GetProductByIdAsync(id);

        if (productDto == null)
        {
            return NotFound(new { error = $"Product with ID {id} not found." });
        }

        return Ok(productDto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var deleted = await _productCatalogService.DeleteProductAsync(id);

        if (!deleted)
        {
            return NotFound(new { error = $"Product with ID {id} not found." });
        }

        return NoContent();
    }
}


