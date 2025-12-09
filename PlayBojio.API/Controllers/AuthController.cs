using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PlayBojio.API.DTOs;
using PlayBojio.API.Models;
using PlayBojio.API.Services;
using System.Security.Claims;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<User> _userManager;

    public AuthController(IAuthService authService, UserManager<User> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(new { message = "Registration failed", errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(result);
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var response = new UserProfileResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.AvatarUrl,
            user.PreferredAreas,
            user.GamePreferences,
            user.WillingToHost,
            user.IsProfilePublic,
            user.AttendedSessions,
            user.TotalSessions,
            user.CreatedAt
        );

        return Ok(response);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        user.DisplayName = request.DisplayName;
        user.AvatarUrl = request.AvatarUrl;
        user.PreferredAreas = request.PreferredAreas;
        user.GamePreferences = request.GamePreferences;
        user.WillingToHost = request.WillingToHost;
        user.IsProfilePublic = request.IsProfilePublic;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        var response = new UserProfileResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.AvatarUrl,
            user.PreferredAreas,
            user.GamePreferences,
            user.WillingToHost,
            user.IsProfilePublic,
            user.AttendedSessions,
            user.TotalSessions,
            user.CreatedAt
        );

        return Ok(response);
    }

    [Authorize]
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserProfile(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var response = new UserProfileResponse(
            user.Id,
            user.IsProfilePublic ? user.Email! : "",
            user.DisplayName,
            user.AvatarUrl,
            user.PreferredAreas,
            user.GamePreferences,
            user.WillingToHost,
            user.IsProfilePublic,
            user.AttendedSessions,
            user.TotalSessions,
            user.CreatedAt
        );

        return Ok(response);
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        
        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to change password", errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Password changed successfully" });
    }

    [Authorize]
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);
        
        if (!result.Succeeded)
            return BadRequest(new { message = "Failed to delete account" });

        return Ok(new { message = "Account deleted successfully" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        // Always return success to prevent email enumeration
        if (user == null)
        {
            return Ok(new { message = "If an account exists with this email, a password reset link has been sent." });
        }

        // Generate password reset token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // In a production app, you would send an email with the reset link here
        // For now, we'll just return success
        // Example: await _emailService.SendPasswordResetEmailAsync(user.Email, token);
        
        return Ok(new { message = "If an account exists with this email, a password reset link has been sent." });
    }
}

