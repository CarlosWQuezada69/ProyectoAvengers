using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProyectoAvengers.Application.Interfaces;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/analytics")]
[EnableRateLimiting("Catalog")]
public class AnalyticsController : ControllerBase
{
    private readonly IViewTracker _viewTracker;

    public AnalyticsController(IViewTracker viewTracker)
    {
        _viewTracker = viewTracker;
    }

    [HttpPost("page-view")]
    public IActionResult TrackPageView([FromBody] TrackPageViewRequest request)
    {
        var page = request.Page?.Trim().ToLowerInvariant() ?? string.Empty;

        if (page.Length == 0 || page.Length > 40)
            return BadRequest();

        _viewTracker.TrackPageView(page);
        return Ok();
    }
}