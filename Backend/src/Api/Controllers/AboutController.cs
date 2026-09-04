using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Application.Mapping;
using ProyectoAvengers.Infrastructure.Persistence;
using ProyectoAvengers.Shared.DTOs.Admin;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
[Route("api/v1/about")]
[EnableRateLimiting("Catalog")]
public class AboutController : ControllerBase
{
    private readonly AppDbContext _context;

    public AboutController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ResponseCache(Duration = 120)]
    public async Task<ActionResult<AboutInfoDto>> GetAbout()
    {
        var about = await _context.AboutInfos
            .AsNoTracking()
            .Include(a => a.Galleries.OrderBy(g => g.DisplayOrder))
            .FirstOrDefaultAsync();

        if (about == null)
            return Ok(new AboutInfoDto());

        return Ok(about.ToDto());
    }
}