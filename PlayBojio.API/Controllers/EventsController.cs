using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
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
    public async Task<IActionResult> GetMyEvents()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _eventService.GetUserEventsAsync(userId);
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
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _eventService.SearchEventsAsync(userId, fromDate, toDate, location, searchText, page, pageSize);

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

}

