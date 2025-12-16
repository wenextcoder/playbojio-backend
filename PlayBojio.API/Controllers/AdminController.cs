using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayBojio.API.Services;
using System.Security.Claims;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ==================== STATISTICS ====================
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetStatsAsync();
        return Ok(stats);
    }

    // ==================== USERS ====================
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int pageSize = 100)
    {
        var users = await _adminService.GetUsersAsync(pageSize);
        return Ok(users);
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserDetail(string userId)
    {
        var user = await _adminService.GetUserDetailAsync(userId);
        if (user == null)
            return NotFound(new { message = "User not found" });
        
        return Ok(user);
    }

    [HttpPost("users/{userId}/suspend")]
    public async Task<IActionResult> SuspendUser(string userId)
    {
        var success = await _adminService.SuspendUserAsync(userId);
        if (!success)
            return BadRequest(new { message = "Failed to suspend user" });
        
        return Ok(new { message = "User suspended successfully" });
    }

    [HttpPost("users/{userId}/unsuspend")]
    public async Task<IActionResult> UnsuspendUser(string userId)
    {
        var success = await _adminService.UnsuspendUserAsync(userId);
        if (!success)
            return BadRequest(new { message = "Failed to unsuspend user" });
        
        return Ok(new { message = "User unsuspended successfully" });
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var success = await _adminService.DeleteUserAsync(userId);
        if (!success)
            return BadRequest(new { message = "Failed to delete user" });
        
        return Ok(new { message = "User deleted successfully" });
    }

    // ==================== EVENTS ====================
    [HttpGet("events")]
    public async Task<IActionResult> GetEvents([FromQuery] int pageSize = 100)
    {
        var events = await _adminService.GetEventsAsync(pageSize);
        return Ok(events);
    }

    [HttpDelete("events/{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var success = await _adminService.DeleteEventAsync(id);
        if (!success)
            return BadRequest(new { message = "Failed to delete event" });
        
        return Ok(new { message = "Event deleted successfully" });
    }

    // ==================== SESSIONS ====================
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions([FromQuery] int pageSize = 100)
    {
        var sessions = await _adminService.GetSessionsAsync(pageSize);
        return Ok(sessions);
    }

    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var success = await _adminService.DeleteSessionAsync(id);
        if (!success)
            return BadRequest(new { message = "Failed to delete session" });
        
        return Ok(new { message = "Session deleted successfully" });
    }

    // ==================== GROUPS ====================
    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups([FromQuery] int pageSize = 100)
    {
        var groups = await _adminService.GetGroupsAsync(pageSize);
        return Ok(groups);
    }

    [HttpDelete("groups/{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var success = await _adminService.DeleteGroupAsync(id);
        if (!success)
            return BadRequest(new { message = "Failed to delete group" });
        
        return Ok(new { message = "Group deleted successfully" });
    }
}
