using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayBojio.API.DTOs;
using PlayBojio.API.Services;
using System.Security.Claims;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _sessionService.CreateSessionAsync(userId, request);

        if (result == null)
            return BadRequest(new { message = "Failed to create session" });

        return CreatedAtAction(nameof(GetSession), new { id = result.Id }, result);
    }

    [HttpGet("my-sessions")]
    public async Task<IActionResult> GetMySessions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _sessionService.GetUserSessionsAsync(userId);
        return Ok(result);
    }

    [HttpGet("attending")]
    public async Task<IActionResult> GetAttendingSessions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _sessionService.GetUserAttendingSessionsAsync(userId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSession(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _sessionService.GetSessionAsync(id, userId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSessionBySlug(string slug)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _sessionService.GetSessionBySlugAsync(slug, userId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSession(int id, [FromBody] UpdateSessionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _sessionService.UpdateSessionAsync(id, userId, request);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelSession(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _sessionService.CancelSessionAsync(id, userId);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchSessions(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? location,
        [FromQuery] string? gameType,
        [FromQuery] bool? availableOnly,
        [FromQuery] bool? newbieFriendly,
        [FromQuery] string? searchText,
        [FromQuery] int? eventId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _sessionService.SearchSessionsAsync(
            userId, fromDate, toDate, location, gameType, availableOnly, newbieFriendly, searchText, eventId, page, pageSize);

        return Ok(result);
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinSession(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _sessionService.JoinSessionAsync(id, userId);

        if (!result)
            return BadRequest(new { message = "Failed to join session" });

        return Ok();
    }

    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeaveSession(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _sessionService.LeaveSessionAsync(id, userId);

        if (!result)
            return BadRequest(new { message = "Failed to leave session" });

        return Ok();
    }

    [HttpGet("{id}/attendees")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAttendees(int id)
    {
        var result = await _sessionService.GetSessionAttendeesAsync(id);
        return Ok(result);
    }

    [HttpGet("{id}/waitlist")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWaitlist(int id)
    {
        var result = await _sessionService.GetSessionWaitlistAsync(id);
        return Ok(result);
    }

    [HttpGet("event/{eventId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEventSessions(int eventId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _sessionService.GetEventSessionsAsync(eventId, userId);
        return Ok(result);
    }

}

