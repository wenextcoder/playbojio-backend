using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.Utils;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("regenerate-slugs")]
    public async Task<IActionResult> RegenerateSlugs()
    {
        var results = new
        {
            sessionsUpdated = 0,
            eventsUpdated = 0,
            sessions = new List<object>(),
            events = new List<object>()
        };

        // Update session slugs
        var sessions = await _context.Sessions.ToListAsync();
        foreach (var session in sessions)
        {
            if (string.IsNullOrEmpty(session.Slug))
            {
                var baseSlug = SlugHelper.GenerateSlug(session.Title);
                var slug = baseSlug;
                int counter = 1;
                while (await _context.Sessions.AnyAsync(s => s.Slug == slug && s.Id != session.Id))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }
                session.Slug = slug;
                results.sessions.Add(new { id = session.Id, title = session.Title, slug = slug });
                results = results with { sessionsUpdated = results.sessionsUpdated + 1 };
            }
        }

        // Update event slugs
        var events = await _context.Events.ToListAsync();
        foreach (var evt in events)
        {
            if (string.IsNullOrEmpty(evt.Slug))
            {
                var baseSlug = SlugHelper.GenerateSlug(evt.Name);
                var slug = baseSlug;
                int counter = 1;
                while (await _context.Events.AnyAsync(e => e.Slug == slug && e.Id != evt.Id))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }
                evt.Slug = slug;
                results.events.Add(new { id = evt.Id, name = evt.Name, slug = slug });
                results = results with { eventsUpdated = results.eventsUpdated + 1 };
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Regenerated slugs: {results.sessionsUpdated} sessions, {results.eventsUpdated} events");

        return Ok(results);
    }
}
