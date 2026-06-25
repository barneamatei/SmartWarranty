using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Service;
using ProductCatalog.Domain.DTOs;

namespace ProductCatalog.Controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly CategoryCatalogService _categoryCatalogService;

    public CategoryController(CategoryCatalogService categoryCatalogService)
    {
        _categoryCatalogService = categoryCatalogService ?? throw new ArgumentNullException(nameof(categoryCatalogService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var categoryDto = await _categoryCatalogService.CreateCategoryAsync(
            request.Name,
            request.Description,
            request.UserId);

        return CreatedAtAction(nameof(GetCategoryById), new { id = categoryDto.CategoryId }, categoryDto);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var categoryDto = await _categoryCatalogService.UpdateCategoryAsync(
            id,
            request.Name,
            request.Description);

        return Ok(categoryDto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories([FromQuery] Guid? userId)
    {
        var categoryDtos = await _categoryCatalogService.GetAllCategoriesAsync(userId);
        return Ok(categoryDtos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
    {
        var categoryDto = await _categoryCatalogService.GetCategoryByIdAsync(id);

        if (categoryDto == null)
        {
            return NotFound(new { error = $"Category with ID {id} not found." });
        }

        return Ok(categoryDto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var deleted = await _categoryCatalogService.DeleteCategoryAsync(id);

        if (!deleted)
        {
            return NotFound(new { error = $"Category with ID {id} not found." });
        }

        return NoContent();
    }
}


