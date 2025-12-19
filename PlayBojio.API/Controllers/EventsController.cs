using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.DTOs;
using PlayBojio.API.Services;
using System.Security.Claims;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ApplicationDbContext _context;

    public EventsController(IEventService eventService, ApplicationDbContext context)
    {
        _eventService = eventService;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _eventService.CreateEventAsync(userId, request);

        if (result == null)
            return BadRequest(new { message = "Failed to create event" });

        return CreatedAtAction(nameof(GetEvent), new { id = result.Id }, result);
    }

    [HttpGet("my-events")]
    public async Task<IActionResult> GetMyEvents([FromQuery] bool? upcomingOnly)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _eventService.GetUserEventsAsync(userId, upcomingOnly ?? true);
        return Ok(result);
    }

    [HttpGet("attending")]
    public async Task<IActionResult> GetAttendingEvents([FromQuery] bool? upcomingOnly)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _eventService.GetUserAttendingEventsAsync(userId, upcomingOnly ?? true);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEvent(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _eventService.GetEventAsync(id, userId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventBySlug(string slug)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _eventService.GetEventBySlugAsync(slug, userId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _eventService.UpdateEventAsync(id, userId, request);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelEvent(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _eventService.CancelEventAsync(id, userId);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchEvents(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? location,
        [FromQuery] string? searchText,
        [FromQuery] bool? upcomingOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _eventService.SearchEventsAsync(userId, fromDate, toDate, location, searchText, upcomingOnly ?? true, page, pageSize);

        return Ok(result);
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinEvent(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _eventService.JoinEventAsync(id, userId);

        if (!result)
            return BadRequest(new { message = "Failed to join event" });

        return Ok();
    }

    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeaveEvent(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _eventService.LeaveEventAsync(id, userId);

        if (!result)
            return BadRequest(new { message = "Failed to leave event" });

        return Ok();
    }

    // Event Blacklist Management
    [HttpGet("{id}/blacklist")]
    public async Task<IActionResult> GetEventBlacklist(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        // Check if user is event organizer
        var eventItem = await _context.Events.FindAsync(id);
        if (eventItem == null)
            return NotFound();

        if (eventItem.OrganizerId != userId)
            return Forbid();

        var blacklistedUsers = await _context.EventBlacklists
            .Where(eb => eb.EventId == id)
            .Include(eb => eb.BlacklistedUser)
            .Include(eb => eb.BlacklistedByUser)
            .Select(eb => new
            {
                eb.Id,
                UserId = eb.BlacklistedUser.Id,
                eb.BlacklistedUser.DisplayName,
                eb.BlacklistedUser.AvatarUrl,
                eb.BlacklistedUser.Email,
                BlacklistedBy = eb.BlacklistedByUser.DisplayName,
                eb.CreatedAt,
                eb.Reason
            })
            .ToListAsync();

        return Ok(blacklistedUsers);
    }

    [HttpPost("{id}/blacklist/{blacklistedUserId}")]
    public async Task<IActionResult> AddToEventBlacklist(int id, string blacklistedUserId, [FromBody] string? reason = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var eventItem = await _context.Events.FindAsync(id);
        if (eventItem == null)
            return NotFound();

        // Check if user is event organizer
        if (eventItem.OrganizerId != userId)
            return Forbid();

        // Cannot blacklist the organizer
        if (blacklistedUserId == eventItem.OrganizerId)
            return BadRequest(new { message = "Cannot blacklist the event organizer" });

        // Check if already blacklisted
        var exists = await _context.EventBlacklists
            .AnyAsync(eb => eb.EventId == id && eb.BlacklistedUserId == blacklistedUserId);

        if (exists)
            return BadRequest(new { message = "User already blacklisted from this event" });

        // Add to blacklist
        _context.EventBlacklists.Add(new Models.EventBlacklist
        {
            EventId = id,
            BlacklistedUserId = blacklistedUserId,
            BlacklistedByUserId = userId,
            Reason = reason
        });

        // Remove from event if they are an attendee
        var attendance = await _context.EventAttendees
            .FirstOrDefaultAsync(ea => ea.EventId == id && ea.UserId == blacklistedUserId);
        
        if (attendance != null)
        {
            _context.EventAttendees.Remove(attendance);
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "User blacklisted from event successfully" });
    }

    [HttpDelete("{id}/blacklist/{blacklistedUserId}")]
    public async Task<IActionResult> RemoveFromEventBlacklist(int id, string blacklistedUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var eventItem = await _context.Events.FindAsync(id);
        if (eventItem == null)
            return NotFound();

        // Check if user is event organizer
        if (eventItem.OrganizerId != userId)
            return Forbid();

        var blacklist = await _context.EventBlacklists
            .FirstOrDefaultAsync(eb => eb.EventId == id && eb.BlacklistedUserId == blacklistedUserId);

        if (blacklist == null)
            return NotFound();

        _context.EventBlacklists.Remove(blacklist);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Get attendees list for event organizer
    [HttpGet("{id}/attendees")]
    public async Task<IActionResult> GetEventAttendees(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var eventItem = await _context.Events.FindAsync(id);
        if (eventItem == null)
            return NotFound();

        // Check if user is event organizer
        if (eventItem.OrganizerId != userId)
            return Forbid();

        var attendees = await _context.EventAttendees
            .Where(ea => ea.EventId == id)
            .Include(ea => ea.User)
            .Select(ea => new
            {
                ea.User.Id,
                ea.User.DisplayName,
                ea.User.AvatarUrl,
                ea.User.Email,
                ea.JoinedAt
            })
            .ToListAsync();

        return Ok(attendees);
    }

}

