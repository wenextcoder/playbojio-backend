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
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly ApplicationDbContext _context;

    public GroupsController(IGroupService groupService, ApplicationDbContext context)
    {
        _groupService = groupService;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupService.CreateGroupAsync(userId, request);

        if (result == null)
            return BadRequest(new { message = "Failed to create group" });

        return CreatedAtAction(nameof(GetGroup), new { id = result.Id }, result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllGroups()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _groupService.GetAllPublicGroupsAsync(userId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGroup(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _groupService.GetGroupAsync(id, userId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("my-groups")]
    public async Task<IActionResult> GetMyGroups()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupService.GetUserGroupsAsync(userId);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupService.UpdateGroupAsync(id, userId, request);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupService.DeleteGroupAsync(id, userId);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> JoinGroup(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupService.JoinGroupAsync(id, userId);

        if (!result)
            return BadRequest(new { message = "Failed to join group" });

        return Ok();
    }

    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeaveGroup(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupService.LeaveGroupAsync(id, userId);

        if (!result)
            return BadRequest(new { message = "Failed to leave group" });

        return Ok();
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(int id)
    {
        var result = await _groupService.GetGroupMembersAsync(id);
        return Ok(result);
    }

    [HttpDelete("{id}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(int id, string memberId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupService.RemoveMemberAsync(id, userId, memberId);

        if (!result)
            return BadRequest(new { message = "Failed to remove member" });

        return Ok();
    }

    [HttpPost("{id}/members/{memberId}/promote")]
    public async Task<IActionResult> PromoteMember(int id, string memberId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupService.PromoteToAdminAsync(id, userId, memberId);

        if (!result)
            return BadRequest(new { message = "Failed to promote member" });

        return Ok();
    }

    [HttpGet("{id}/events")]
    public async Task<IActionResult> GetGroupEvents(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Check if user is a member of the group
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == id && gm.UserId == userId);

        if (!isMember)
            return Forbid();

        // Get events linked to this group
        var eventIds = await _context.EventGroups
            .Where(eg => eg.GroupId == id)
            .Select(eg => eg.EventId)
            .ToListAsync();

        var events = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .Where(e => eventIds.Contains(e.Id) && !e.IsCancelled && e.StartDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .Select(e => new EventListResponse(
                e.Id,
                e.Name,
                e.Slug,
                e.ImageUrl,
                e.StartDate,
                e.EndDate,
                e.Location,
                e.Attendees.Count,
                e.MaxParticipants,
                e.Organizer.DisplayName,
                e.Price
            ))
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id}/sessions")]
    public async Task<IActionResult> GetGroupSessions(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        // Check if user is a member of the group
        var isMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == id && gm.UserId == userId);

        if (!isMember)
            return Forbid();

        // Get sessions linked to this group
        var sessionIds = await _context.SessionGroups
            .Where(sg => sg.GroupId == id)
            .Select(sg => sg.SessionId)
            .ToListAsync();

        var sessions = await _context.Sessions
            .Include(s => s.Host)
            .Include(s => s.Attendees)
            .Include(s => s.Event)
            .Where(s => sessionIds.Contains(s.Id) && !s.IsCancelled && s.StartTime >= DateTime.UtcNow)
            .OrderBy(s => s.StartTime)
            .Select(s => new SessionListResponse(
                s.Id,
                s.Title,
                s.Slug,
                s.ImageUrl,
                s.PrimaryGame,
                s.StartTime,
                s.Location,
                s.Attendees.Count,
                s.MaxPlayers,
                s.MaxPlayers - (s.Attendees.Count + s.ReservedSlots + (s.IsHostParticipating ? 1 : 0)),
                s.Host.DisplayName,
                s.IsNewbieFriendly,
                s.CostPerPerson,
                s.SessionType,
                s.EventId,
                s.Event != null ? s.Event.Name : null
            ))
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetEventGroups(int eventId)
    {
        // Get group IDs associated with this event
        var groupIds = await _context.EventGroups
            .Where(eg => eg.EventId == eventId)
            .Select(eg => eg.GroupId)
            .ToListAsync();

        return Ok(groupIds);
    }

    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetSessionGroups(int sessionId)
    {
        // Get group IDs associated with this session
        var groupIds = await _context.SessionGroups
            .Where(sg => sg.SessionId == sessionId)
            .Select(sg => sg.GroupId)
            .ToListAsync();

        return Ok(groupIds);
    }
}

