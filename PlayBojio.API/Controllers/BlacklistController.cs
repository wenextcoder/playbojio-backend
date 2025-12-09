using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.Models;
using System.Security.Claims;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BlacklistController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BlacklistController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetBlacklist()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var blacklistedUsers = await _context.Blacklists
            .Where(b => b.UserId == userId)
            .Include(b => b.BlacklistedUser)
            .Select(b => new
            {
                b.Id,
                UserId = b.BlacklistedUser.Id,
                b.BlacklistedUser.DisplayName,
                b.BlacklistedUser.AvatarUrl,
                b.CreatedAt
            })
            .ToListAsync();

        return Ok(blacklistedUsers);
    }

    [HttpPost("{blacklistedUserId}")]
    public async Task<IActionResult> AddToBlacklist(string blacklistedUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        if (userId == blacklistedUserId)
            return BadRequest(new { message = "Cannot blacklist yourself" });

        var exists = await _context.Blacklists
            .AnyAsync(b => b.UserId == userId && b.BlacklistedUserId == blacklistedUserId);

        if (exists)
            return BadRequest(new { message = "User already blacklisted" });

        _context.Blacklists.Add(new Blacklist
        {
            UserId = userId,
            BlacklistedUserId = blacklistedUserId
        });

        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{blacklistedUserId}")]
    public async Task<IActionResult> RemoveFromBlacklist(string blacklistedUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var blacklist = await _context.Blacklists
            .FirstOrDefaultAsync(b => b.UserId == userId && b.BlacklistedUserId == blacklistedUserId);

        if (blacklist == null)
            return NotFound();

        _context.Blacklists.Remove(blacklist);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

