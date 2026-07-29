using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/settings")]
[EnableRateLimiting("Catalog")]
public class SettingsController : ControllerBase
{
    private static readonly HashSet<string> PublicKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "logo_url", "business_name", "copyright_text",
        "contact_email", "contact_phone", "social_links",
        "seo_title", "seo_description", "seo_keywords"
    };

    private readonly AppDbContext _context;

    public SettingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("public")]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<Dictionary<string, string?>>> GetPublicSettings()
    {
        var settings = await _context.SiteSettings
            .AsNoTracking()
            .Where(s => PublicKeys.Contains(s.Key))
            .ToListAsync();

        return Ok(settings.ToDictionary(s => s.Key, s => s.Value));
    }
}
