using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Api.Authorization;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[EnableRateLimiting("Admin")]
public class AdminAboutController : AdminBaseController
{
    private readonly IAboutService _aboutService;

    public AdminAboutController(IAboutService aboutService)
    {
        _aboutService = aboutService;
    }

    [HttpGet("about")]
    [RequirePermission("about.view")]
    public async Task<ActionResult<AboutInfoDto>> GetAbout()
        => Ok(await _aboutService.GetAboutAsync());

    [HttpPut("about")]
    [RequirePermission("about.update")]
    public async Task<ActionResult<AboutInfoDto>> UpdateAbout([FromBody] UpdateAboutInfoRequest request)
        => Ok(await _aboutService.UpdateAboutAsync(request));

    [HttpPost("about/gallery")]
    [RequirePermission("about.update")]
    public async Task<ActionResult<AboutGalleryDto>> UploadImage(
        [FromQuery] string section, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ProblemDetails { Title = "Archivo vacío", Status = 400 });

        var upload = new FileUpload
        {
            Content = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };

        try
        {
            var dto = await _aboutService.UploadImageAsync(section, upload);
            return Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Archivo no válido",
                Status = 400,
                Detail = ex.Message
            });
        }
    }

    [HttpDelete("about/gallery/{id:guid}")]
    [RequirePermission("about.update")]
    public async Task<ActionResult> DeleteImage(Guid id)
        => await _aboutService.DeleteImageAsync(id) ? NoContent() : NotFound();

    [HttpPut("about/gallery/order")]
    [RequirePermission("about.update")]
    public async Task<ActionResult> UpdateOrder([FromBody] List<UpdateGalleryOrderItem> order)
    {
        await _aboutService.UpdateOrderAsync(order);
        return NoContent();
    }
}
