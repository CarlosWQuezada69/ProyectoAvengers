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
        var ns = (XNamespace)"http://www.sitemaps.org/schemas/sitemap/0.9";

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
            new(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/"),
                new XElement(ns + "changefreq", "daily"),
                new XElement(ns + "priority", "1.0")),
        };

        foreach (var category in categories)
        {
            urls.Add(new(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/categorias/{category.Slug}"),
                new XElement(ns + "changefreq", "weekly"),
                new XElement(ns + "priority", "0.8")));
        }

        foreach (var product in products)
        {
            urls.Add(new(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/productos/{product.Slug}"),
                new XElement(ns + "lastmod", product.UpdatedAt?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd")),
                new XElement(ns + "changefreq", "weekly"),
                new XElement(ns + "priority", "0.6")));
        }

        var xml = new XDocument(
            new XElement(ns + "urlset", urls));

        return Content(xml.ToString(), "application/xml", Encoding.UTF8);
    }
}