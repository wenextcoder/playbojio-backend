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
    private readonly IGroupJoinRequestService _groupJoinRequestService;
    private readonly IGroupInvitationService _groupInvitationService;
    private readonly ApplicationDbContext _context;

    public GroupsController(
        IGroupService groupService,
        IGroupJoinRequestService groupJoinRequestService,
        IGroupInvitationService groupInvitationService,
        ApplicationDbContext context)
    {
        _groupService = groupService;
        _groupJoinRequestService = groupJoinRequestService;
        _groupInvitationService = groupInvitationService;
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

    // Group Blacklist Management
    [HttpGet("{id}/blacklist")]
    public async Task<IActionResult> GetGroupBlacklist(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        // Check if user is group owner or admin
        var group = await _context.Groups.FindAsync(id);
        if (group == null)
            return NotFound();

        var isAdmin = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == id && gm.UserId == userId && gm.IsAdmin);

        if (group.OwnerId != userId && !isAdmin)
            return Forbid();

        var blacklistedUsers = await _context.GroupBlacklists
            .Where(gb => gb.GroupId == id)
            .Include(gb => gb.BlacklistedUser)
            .Include(gb => gb.BlacklistedByUser)
            .Select(gb => new
            {
                gb.Id,
                UserId = gb.BlacklistedUser.Id,
                gb.BlacklistedUser.DisplayName,
                gb.BlacklistedUser.AvatarUrl,
                gb.BlacklistedUser.Email,
                BlacklistedBy = gb.BlacklistedByUser.DisplayName,
                gb.CreatedAt,
                gb.Reason
            })
            .ToListAsync();

        return Ok(blacklistedUsers);
    }

    [HttpPost("{id}/blacklist/{blacklistedUserId}")]
    public async Task<IActionResult> AddToGroupBlacklist(int id, string blacklistedUserId, [FromBody] string? reason = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var group = await _context.Groups.FindAsync(id);
        if (group == null)
            return NotFound();

        // Check if user is group owner or admin
        var isAdmin = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == id && gm.UserId == userId && gm.IsAdmin);

        if (group.OwnerId != userId && !isAdmin)
            return Forbid();

        // Cannot blacklist the owner
        if (blacklistedUserId == group.OwnerId)
            return BadRequest(new { message = "Cannot blacklist the group owner" });

        // Check if already blacklisted
        var exists = await _context.GroupBlacklists
            .AnyAsync(gb => gb.GroupId == id && gb.BlacklistedUserId == blacklistedUserId);

        if (exists)
            return BadRequest(new { message = "User already blacklisted from this group" });

        // Add to blacklist
        _context.GroupBlacklists.Add(new Models.GroupBlacklist
        {
            GroupId = id,
            BlacklistedUserId = blacklistedUserId,
            BlacklistedByUserId = userId,
            Reason = reason
        });

        // Remove from group if they are a member
        var membership = await _context.GroupMembers
            .FirstOrDefaultAsync(gm => gm.GroupId == id && gm.UserId == blacklistedUserId);
        
        if (membership != null)
        {
            _context.GroupMembers.Remove(membership);
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "User blacklisted from group successfully" });
    }

    [HttpDelete("{id}/blacklist/{blacklistedUserId}")]
    public async Task<IActionResult> RemoveFromGroupBlacklist(int id, string blacklistedUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var group = await _context.Groups.FindAsync(id);
        if (group == null)
            return NotFound();

        // Check if user is group owner or admin
        var isAdmin = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == id && gm.UserId == userId && gm.IsAdmin);

        if (group.OwnerId != userId && !isAdmin)
            return Forbid();

        var blacklist = await _context.GroupBlacklists
            .FirstOrDefaultAsync(gb => gb.GroupId == id && gb.BlacklistedUserId == blacklistedUserId);

        if (blacklist == null)
            return NotFound();

        _context.GroupBlacklists.Remove(blacklist);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ==================== Group Join Requests ====================

    /// <summary>
    /// Request to join a private group
    /// </summary>
    [HttpPost("{id}/join-request")]
    public async Task<IActionResult> CreateJoinRequest(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupJoinRequestService.CreateJoinRequestAsync(id, userId);
        if (!result)
            return BadRequest(new { message = "Failed to create join request. You may already be a member or blacklisted." });

        return Ok(new { message = "Join request sent successfully" });
    }

    /// <summary>
    /// Get pending join requests for a group (admin only)
    /// </summary>
    [HttpGet("{id}/join-requests")]
    public async Task<IActionResult> GetGroupJoinRequests(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var requests = await _groupJoinRequestService.GetGroupJoinRequestsAsync(id, userId);
        return Ok(requests);
    }

    /// <summary>
    /// Get my join requests
    /// </summary>
    [HttpGet("my-join-requests")]
    public async Task<IActionResult> GetMyJoinRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var requests = await _groupJoinRequestService.GetMyJoinRequestsAsync(userId);
        return Ok(requests);
    }

    /// <summary>
    /// Approve a join request (admin only)
    /// </summary>
    [HttpPost("{id}/join-requests/{requestId}/approve")]
    public async Task<IActionResult> ApproveJoinRequest(int id, int requestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupJoinRequestService.ApproveJoinRequestAsync(requestId, userId);
        if (!result)
            return BadRequest(new { message = "Failed to approve join request" });

        return Ok(new { message = "Join request approved" });
    }

    /// <summary>
    /// Reject a join request (admin only)
    /// </summary>
    [HttpPost("{id}/join-requests/{requestId}/reject")]
    public async Task<IActionResult> RejectJoinRequest(int id, int requestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupJoinRequestService.RejectJoinRequestAsync(requestId, userId);
        if (!result)
            return BadRequest(new { message = "Failed to reject join request" });

        return Ok(new { message = "Join request rejected" });
    }

    // ==================== Group Invitations ====================

    /// <summary>
    /// Invite a user to the group (admin only)
    /// </summary>
    [HttpPost("{id}/invite/{invitedUserId}")]
    public async Task<IActionResult> InviteUserToGroup(int id, string invitedUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupInvitationService.InviteUserToGroupAsync(id, invitedUserId, userId);
        if (!result)
            return BadRequest(new { message = "Failed to invite user. User may already be a member or blacklisted." });

        return Ok(new { message = "Invitation sent successfully" });
    }

    /// <summary>
    /// Get invitations for a group (admin only)
    /// </summary>
    [HttpGet("{id}/invitations")]
    public async Task<IActionResult> GetGroupInvitations(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var invitations = await _groupInvitationService.GetGroupInvitationsAsync(id, userId);
        return Ok(invitations);
    }

    /// <summary>
    /// Get my group invitations
    /// </summary>
    [HttpGet("my-invitations")]
    public async Task<IActionResult> GetMyInvitations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var invitations = await _groupInvitationService.GetMyInvitationsAsync(userId);
        return Ok(invitations);
    }

    /// <summary>
    /// Accept a group invitation
    /// </summary>
    [HttpPost("invitations/{invitationId}/accept")]
    public async Task<IActionResult> AcceptInvitation(int invitationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupInvitationService.AcceptInvitationAsync(invitationId, userId);
        if (!result)
            return BadRequest(new { message = "Failed to accept invitation" });

        return Ok(new { message = "Invitation accepted" });
    }

    /// <summary>
    /// Decline a group invitation
    /// </summary>
    [HttpPost("invitations/{invitationId}/decline")]
    public async Task<IActionResult> DeclineInvitation(int invitationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupInvitationService.DeclineInvitationAsync(invitationId, userId);
        if (!result)
            return BadRequest(new { message = "Failed to decline invitation" });

        return Ok(new { message = "Invitation declined" });
    }

    /// <summary>
    /// Cancel a group invitation (admin only)
    /// </summary>
    [HttpDelete("{id}/invitations/{invitationId}")]
    public async Task<IActionResult> CancelInvitation(int id, int invitationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _groupInvitationService.CancelInvitationAsync(invitationId, userId);
        if (!result)
            return BadRequest(new { message = "Failed to cancel invitation" });

        return NoContent();
    }
}



