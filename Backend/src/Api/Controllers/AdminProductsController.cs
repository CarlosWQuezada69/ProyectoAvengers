using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[EnableRateLimiting("Admin")]
public class AdminProductsController : AdminBaseController
{
    private readonly IProductService _productService;

    public AdminProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("products")]
    [RequirePermission("products.view")]
    public async Task<ActionResult<PaginatedResponse<ProductListDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _productService.ListAsync(search, categoryId, isActive, page, pageSize);
        return Ok(result);
    }

    [HttpGet("products/{id:guid}")]
    [RequirePermission("products.view")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost("products")]
    [RequirePermission("products.create")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("products/{id:guid}")]
    [RequirePermission("products.update")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productService.UpdateAsync(id, request);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpDelete("products/{id:guid}")]
    [RequirePermission("products.delete")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("products/{id:guid}/images")]
    [RequirePermission("products.update")]
    public async Task<ActionResult<ProductImageDto>> UploadImage(Guid id, IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var image = await _productService.UploadImageAsync(id, stream, file.FileName, file.ContentType);
        if (image == null) return NotFound();
        return Ok(image);
    }

    [HttpDelete("products/{id:guid}/images/{imageId:guid}")]
    [RequirePermission("products.update")]
    public async Task<ActionResult> DeleteImage(Guid id, Guid imageId)
    {
        var deleted = await _productService.DeleteImageAsync(id, imageId);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPut("products/{id:guid}/images/order")]
    [RequirePermission("products.update")]
    public async Task<ActionResult> UpdateImageOrder(Guid id, [FromBody] List<UpdateImageOrderItem> order)
    {
        var updated = await _productService.UpdateImageOrderAsync(id, order);
        if (!updated) return NotFound();
        return NoContent();
    }

    [HttpPost("products/{id:guid}/restrictions")]
    [RequirePermission("products.manage-restrictions")]
    public async Task<ActionResult<ProductRestrictionDto>> CreateRestriction(Guid id, [FromBody] CreateRestrictionRequest request)
    {
        var restriction = await _productService.CreateRestrictionAsync(id, request);
        if (restriction == null) return NotFound();
        return Ok(restriction);
    }

    [HttpPut("products/{id:guid}/restrictions/{restrictionId:guid}")]
    [RequirePermission("products.manage-restrictions")]
    public async Task<ActionResult<ProductRestrictionDto>> UpdateRestriction(Guid id, Guid restrictionId, [FromBody] UpdateRestrictionRequest request)
    {
        var restriction = await _productService.UpdateRestrictionAsync(id, restrictionId, request);
        if (restriction == null) return NotFound();
        return Ok(restriction);
    }

    [HttpDelete("products/{id:guid}/restrictions/{restrictionId:guid}")]
    [RequirePermission("products.manage-restrictions")]
    public async Task<ActionResult> DeleteRestriction(Guid id, Guid restrictionId)
    {
        var deleted = await _productService.DeleteRestrictionAsync(id, restrictionId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
