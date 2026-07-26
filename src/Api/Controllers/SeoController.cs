using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoAvengers.Infrastructure.Persistence;

namespace ProyectoAvengers.Api.Controllers;

[ApiController]
public class SeoController : ControllerBase
{
    private readonly AppDbContext _context;

    public SeoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 86400)]
    public async Task<ActionResult> RobotsTxt()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine("Disallow: /api/");
        sb.AppendLine();
        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 86400)]
    public async Task<ActionResult> SitemapXml()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.DeletedAt == null)
            .Select(p => new { p.Slug, p.UpdatedAt })
            .ToListAsync();

        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new { c.Slug })
            .ToListAsync();

        var urls = new List<XElement>
        {
            new XElement("url",
                new XElement("loc", $"{baseUrl}/"),
                new XElement("changefreq", "daily"),
                new XElement("priority", "1.0")),
        };

        foreach (var category in categories)
        {
            urls.Add(new XElement("url",
                new XElement("loc", $"{baseUrl}/categorias/{category.Slug}"),
                new XElement("changefreq", "weekly"),
                new XElement("priority", "0.8")));
        }

        foreach (var product in products)
        {
            urls.Add(new XElement("url",
                new XElement("loc", $"{baseUrl}/productos/{product.Slug}"),
                new XElement("lastmod", product.UpdatedAt?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd")),
                new XElement("changefreq", "weekly"),
                new XElement("priority", "0.6")));
        }

        var xml = new XDocument(
            new XElement("urlset",
                new XAttribute("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9"),
                urls));

        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }
}