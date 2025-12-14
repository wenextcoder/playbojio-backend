using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.Models;
using PlayBojio.API.Utils;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<User> _userManager;

    public AdminController(
        ApplicationDbContext context, 
        ILogger<AdminController> logger,
        UserManager<User> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    // ============ DASHBOARD STATS ============
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var now = DateTime.UtcNow;
        
        var totalUsers = await _context.Users.CountAsync();
        var totalSessions = await _context.Sessions.CountAsync();
        var totalEvents = await _context.Events.CountAsync();
        var totalGroups = await _context.Groups.CountAsync();
        
        var activeSessions = await _context.Sessions.CountAsync(s => s.StartTime > now);
        var upcomingEvents = await _context.Events.CountAsync(e => e.StartDate > now);

        return Ok(new
        {
            totalUsers,
            totalSessions,
            activeSessions,
            totalEvents,
            upcomingEvents,
            totalGroups
        });
    }

    // ============ USER MANAGEMENT ============
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.DisplayName,
                u.EmailConfirmed,
                u.CreatedAt,
                u.LockoutEnd,
                u.TotalSessions,
                u.AttendedSessions
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserDetails(string userId)
    {
        var user = await _context.Users
            .Include(u => u.CreatedSessions)
            .Include(u => u.CreatedEvents)
            .Include(u => u.OwnedGroups)
            .Include(u => u.GroupMemberships)
                .ThenInclude(gm => gm.Group)
            .Include(u => u.SessionAttendances)
                .ThenInclude(sa => sa.Session)
            .Include(u => u.EventAttendances)
                .ThenInclude(ea => ea.Event)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        var createdSessions = user.CreatedSessions
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Slug,
                s.PrimaryGame,
                s.StartTime,
                s.Location,
                s.IsCancelled
            })
            .ToList();

        var createdEvents = user.CreatedEvents
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Slug,
                e.StartDate,
                e.Location,
                e.IsCancelled
            })
            .ToList();

        var ownedGroups = user.OwnedGroups
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Visibility,
                MembersCount = g.Members.Count
            })
            .ToList();

        var joinedGroups = user.GroupMemberships
            .Where(gm => gm.Group.OwnerId != userId)
            .Select(gm => new
            {
                gm.Group.Id,
                gm.Group.Name,
                gm.Group.Visibility,
                MembersCount = gm.Group.Members.Count,
                gm.JoinedAt
            })
            .OrderByDescending(g => g.JoinedAt)
            .ToList();

        var attendedSessions = user.SessionAttendances
            .Where(sa => sa.Session.HostId != userId)
            .OrderByDescending(sa => sa.JoinedAt)
            .Select(sa => new
            {
                sa.Session.Id,
                sa.Session.Title,
                sa.Session.Slug,
                sa.Session.PrimaryGame,
                sa.Session.StartTime,
                sa.Session.Location,
                sa.JoinedAt
            })
            .ToList();

        var attendedEvents = user.EventAttendances
            .Where(ea => ea.Event.OrganizerId != userId)
            .OrderByDescending(ea => ea.JoinedAt)
            .Select(ea => new
            {
                ea.Event.Id,
                ea.Event.Name,
                ea.Event.Slug,
                ea.Event.StartDate,
                ea.Event.Location,
                ea.JoinedAt
            })
            .ToList();

        return Ok(new
        {
            user.Id,
            user.Email,
            user.DisplayName,
            user.AvatarUrl,
            user.PreferredAreas,
            user.GamePreferences,
            user.WillingToHost,
            user.IsProfilePublic,
            user.EmailConfirmed,
            user.CreatedAt,
            user.LockoutEnd,
            user.TotalSessions,
            TotalAttendedSessions = user.AttendedSessions,
            CreatedSessions = createdSessions,
            CreatedEvents = createdEvents,
            OwnedGroups = ownedGroups,
            JoinedGroups = joinedGroups,
            AttendedSessions = attendedSessions,
            AttendedEvents = attendedEvents
        });
    }

    [HttpPost("users/{userId}/suspend")]
    public async Task<IActionResult> SuspendUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        // Suspend for 100 years (effectively permanent)
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        
        _logger.LogInformation($"User {user.Email} suspended by admin");
        
        return Ok(new { message = "User suspended successfully" });
    }

    [HttpPost("users/{userId}/unsuspend")]
    public async Task<IActionResult> UnsuspendUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        await _userManager.SetLockoutEndDateAsync(user, null);
        
        _logger.LogInformation($"User {user.Email} unsuspended by admin");
        
        return Ok(new { message = "User unsuspended successfully" });
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });

        await _userManager.DeleteAsync(user);
        
        _logger.LogInformation($"User {user.Email} deleted by admin");
        
        return Ok(new { message = "User deleted successfully" });
    }

    // ============ SESSION MANAGEMENT ============
    [HttpGet("sessions")]
    public async Task<IActionResult> GetAllSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var sessions = await _context.Sessions
            .Include(s => s.Host)
            .Include(s => s.Event)
            .Include(s => s.Attendees)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Slug,
                s.PrimaryGame,
                s.StartTime,
                s.Location,
                s.MaxPlayers,
                HostName = s.Host.DisplayName,
                HostEmail = s.Host.Email,
                AttendeesCount = s.Attendees.Count,
                EventName = s.Event != null ? s.Event.Name : null,
                s.CreatedAt
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session == null)
            return NotFound(new { message = "Session not found" });

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Session {session.Title} (ID: {id}) deleted by admin");
        
        return Ok(new { message = "Session deleted successfully" });
    }

    // ============ EVENT MANAGEMENT ============
    [HttpGet("events")]
    public async Task<IActionResult> GetAllEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var events = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .Include(e => e.Sessions)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Slug,
                e.Description,
                e.StartDate,
                e.EndDate,
                e.Location,
                e.Price,
                OrganizerName = e.Organizer.DisplayName,
                OrganizerEmail = e.Organizer.Email,
                AttendeesCount = e.Attendees.Count,
                SessionsCount = e.Sessions.Count,
                e.CreatedAt
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(events);
    }

    [HttpDelete("events/{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt == null)
            return NotFound(new { message = "Event not found" });

        _context.Events.Remove(evt);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Event {evt.Name} (ID: {id}) deleted by admin");
        
        return Ok(new { message = "Event deleted successfully" });
    }

    // ============ GROUP MANAGEMENT ============
    [HttpGet("groups")]
    public async Task<IActionResult> GetAllGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
    {
        var groups = await _context.Groups
            .Include(g => g.Owner)
            .Include(g => g.Members)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Description,
                g.Visibility,
                OwnerName = g.Owner.DisplayName,
                OwnerEmail = g.Owner.Email,
                MembersCount = g.Members.Count,
                g.CreatedAt
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(groups);
    }

    [HttpDelete("groups/{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null)
            return NotFound(new { message = "Group not found" });

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Group {group.Name} (ID: {id}) deleted by admin");
        
        return Ok(new { message = "Group deleted successfully" });
    }

    // ============ UTILITY ENDPOINTS ============
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
