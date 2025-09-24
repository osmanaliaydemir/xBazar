using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Application.DTOs.Category;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers;

[Route("api/[controller]")]
public class CategoriesController : BaseController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _categoryService.GetBySlugAsync(slug);
        return Ok(result);
    }

    [HttpGet("root")]
    public async Task<IActionResult> GetRootCategories()
    {
        var result = await _categoryService.GetRootCategoriesAsync();
        return Ok(result);
    }

    [HttpGet("{parentId}/subcategories")]
    public async Task<IActionResult> GetSubCategories(Guid parentId)
    {
        var result = await _categoryService.GetSubCategoriesAsync(parentId);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Role_Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto createCategoryDto)
    {
        var result = await _categoryService.CreateAsync(createCategoryDto);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Role_Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto updateCategoryDto)
    {
        var result = await _categoryService.UpdateAsync(id, updateCategoryDto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Role_Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return Ok(result);
    }

    [HttpPut("{id}/toggle-active")]
    [Authorize(Policy = "Role_Admin")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var result = await _categoryService.ToggleActiveAsync(id);
        return Ok(result);
    }
}
