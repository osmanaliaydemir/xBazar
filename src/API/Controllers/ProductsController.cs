using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Services;
using Application.DTOs.Product;
using Application.DTOs.Common;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : BaseController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var data = await _productService.GetAllAsync(page, pageSize);
        return Ok(ApiResponse.Success(data));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var data = await _productService.GetByIdAsync(id);
        return Ok(ApiResponse.Success(data));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var data = await _productService.SearchAsync(q ?? string.Empty, page, pageSize);
        return Ok(ApiResponse.Success(data));
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 10)
    {
        var data = await _productService.GetFeaturedAsync(count);
        return Ok(ApiResponse.Success(data));
    }

    // Related products endpoint is not supported by current service

    [HttpGet("store/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var data = await _productService.GetByStoreAsync(storeId, page, pageSize);
        return Ok(ApiResponse.Success(data));
    }

    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var data = await _productService.GetByCategoryAsync(categoryId, page, pageSize);
        return Ok(ApiResponse.Success(data));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProductDto createProductDto)
    {
        var data = await _productService.CreateAsync(createProductDto);
        return Ok(ApiResponse.Success(data));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto updateProductDto)
    {
        var data = await _productService.UpdateAsync(id, updateProductDto);
        return Ok(ApiResponse.Success(data));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _productService.DeleteAsync(id);
        return Ok(ApiResponse.Success(ok));
    }

    [HttpPut("{id}/stock")]
    [Authorize]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockDto updateStockDto)
    {
        var ok = await _productService.UpdateStockAsync(id, updateStockDto.Quantity);
        return Ok(ApiResponse.Success(ok));
    }

    [HttpPut("{id}/price")]
    [Authorize]
    public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceDto updatePriceDto)
    {
        var ok = await _productService.UpdatePriceAsync(id, updatePriceDto.Price);
        return Ok(ApiResponse.Success(ok));
    }

    // Toggle endpoints removed due to lack of service support

    [HttpPost("search-advanced")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchAdvanced([FromBody] ProductSearchQueryDto query)
    {
        var data = await _productService.SearchAdvancedAsync(query);
        return Ok(ApiResponse.Success(data));
    }
}

public class UpdateStockDto
{
    public int Quantity { get; set; }
}

public class UpdatePriceDto
{
    public decimal Price { get; set; }
}
