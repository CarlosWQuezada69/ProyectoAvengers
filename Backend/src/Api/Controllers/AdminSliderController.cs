using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[EnableRateLimiting("Admin")]
public class AdminSliderController : AdminBaseController
{
    private readonly ISliderService _sliderService;

    public AdminSliderController(ISliderService sliderService)
    {
        _sliderService = sliderService;
    }

    [HttpGet("slider")]
    [RequirePermission("slider.view")]
    public async Task<ActionResult<List<SliderItemDto>>> GetSlider()
    {
        var items = await _sliderService.ListAsync();
        return Ok(items);
    }

    [HttpPost("slider")]
    [RequirePermission("slider.create")]
    public async Task<ActionResult<SliderItemDto>> CreateSlider(IFormFile? image, [FromForm] CreateSliderItemRequest request)
    {
        Stream? imageStream = null;
        string? fileName = null;
        string? contentType = null;

        if (image != null)
        {
            imageStream = image.OpenReadStream();
            fileName = image.FileName;
            contentType = image.ContentType;
        }

        var item = await _sliderService.CreateAsync(imageStream, fileName, contentType, request);
        return Ok(item);
    }

    [HttpPut("slider/{id:guid}")]
    [RequirePermission("slider.update")]
    public async Task<ActionResult<SliderItemDto>> UpdateSlider(Guid id, [FromBody] UpdateSliderItemRequest request)
    {
        var item = await _sliderService.UpdateAsync(id, request);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpDelete("slider/{id:guid}")]
    [RequirePermission("slider.delete")]
    public async Task<ActionResult> DeleteSlider(Guid id)
    {
        var deleted = await _sliderService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPut("slider/order")]
    [RequirePermission("slider.update")]
    public async Task<ActionResult> UpdateOrder([FromBody] List<UpdateSliderOrderItem> order)
    {
        var updated = await _sliderService.UpdateOrderAsync(order);
        if (!updated) return NotFound();
        return NoContent();
    }
}
