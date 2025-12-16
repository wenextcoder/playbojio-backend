using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayBojio.API.DTOs;
using PlayBojio.API.Services;
using System.Security.Claims;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    /// <summary>
    /// Get list of friends
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var friends = await _friendService.GetFriendsAsync(userId);
        return Ok(friends);
    }

    /// <summary>
    /// Get sent friend requests
    /// </summary>
    [HttpGet("requests/sent")]
    public async Task<IActionResult> GetSentRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var requests = await _friendService.GetSentFriendRequestsAsync(userId);
        return Ok(requests);
    }

    /// <summary>
    /// Get received friend requests
    /// </summary>
    [HttpGet("requests/received")]
    public async Task<IActionResult> GetReceivedRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var requests = await _friendService.GetReceivedFriendRequestsAsync(userId);
        return Ok(requests);
    }

    /// <summary>
    /// Send friend request to a user
    /// </summary>
    [HttpPost("request/{receiverId}")]
    public async Task<IActionResult> SendFriendRequest(string receiverId)
    {
        var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (senderId == null)
            return Unauthorized();

        var result = await _friendService.SendFriendRequestAsync(senderId, receiverId);
        if (!result)
            return BadRequest(new { message = "Failed to send friend request. Already friends or request exists." });

        return Ok(new { message = "Friend request sent successfully" });
    }

    /// <summary>
    /// Accept a friend request
    /// </summary>
    [HttpPost("accept/{requestId}")]
    public async Task<IActionResult> AcceptFriendRequest(int requestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _friendService.AcceptFriendRequestAsync(requestId, userId);
        if (!result)
            return BadRequest(new { message = "Failed to accept friend request" });

        return Ok(new { message = "Friend request accepted" });
    }

    /// <summary>
    /// Reject a friend request
    /// </summary>
    [HttpPost("reject/{requestId}")]
    public async Task<IActionResult> RejectFriendRequest(int requestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _friendService.RejectFriendRequestAsync(requestId, userId);
        if (!result)
            return BadRequest(new { message = "Failed to reject friend request" });

        return Ok(new { message = "Friend request rejected" });
    }

    /// <summary>
    /// Cancel a sent friend request
    /// </summary>
    [HttpDelete("request/{requestId}")]
    public async Task<IActionResult> CancelFriendRequest(int requestId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _friendService.CancelFriendRequestAsync(requestId, userId);
        if (!result)
            return BadRequest(new { message = "Failed to cancel friend request" });

        return Ok(new { message = "Friend request cancelled" });
    }

    /// <summary>
    /// Remove a friend
    /// </summary>
    [HttpDelete("{friendId}")]
    public async Task<IActionResult> RemoveFriend(string friendId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var result = await _friendService.RemoveFriendAsync(userId, friendId);
        if (!result)
            return BadRequest(new { message = "Failed to remove friend" });

        return Ok(new { message = "Friend removed successfully" });
    }
}
