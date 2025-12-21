using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlayBojio.API.DTOs;
using PlayBojio.API.Models;
using PlayBojio.API.Services;
using System.Security.Claims;
using Google.Apis.Auth;

namespace PlayBojio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, UserManager<User> userManager, IConfiguration configuration)
    {
        _authService = authService;
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(new { message = "Registration failed", errors = result.Errors });

        return Ok(result.Data);
    }

    [HttpPost("check-email")]
    public async Task<IActionResult> CheckEmail([FromBody] CheckEmailRequest request)
    {
        // Extract username from email
        var username = request.Email.Split('@')[0].ToLower();
        
        // Find all accounts with the same username but different domains
        var allUsers = await _userManager.Users.ToListAsync();
        var similarAccounts = allUsers
            .Where(u => u.Email != null && 
                   u.Email.Split('@')[0].ToLower() == username &&
                   u.Email.ToLower() != request.Email.ToLower())
            .Select(u => new {
                email = u.Email,
                displayName = u.DisplayName,
                createdAt = u.CreatedAt
            })
            .ToList();

        return Ok(new {
            exists = await _userManager.FindByEmailAsync(request.Email) != null,
            similarAccounts = similarAccounts
        });
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

        // Check if user has a password (to determine if they logged in with Google)
        var hasPassword = await _userManager.HasPasswordAsync(user);

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
            user.CreatedAt,
            hasPassword
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

        // Check if user has a password
        var hasPassword = await _userManager.HasPasswordAsync(user);

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
            user.CreatedAt,
            hasPassword
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

        // Check if user has a password
        var hasPassword = await _userManager.HasPasswordAsync(user);

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
            user.CreatedAt,
            hasPassword
        );

        return Ok(response);
    }

    [Authorize]
    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return BadRequest(new { message = "Search query must be at least 2 characters" });

        // Convert query to lowercase for case-insensitive search
        var lowerQuery = query.ToLower();
        
        var users = _userManager.Users
            .Where(u => u.DisplayName.ToLower().Contains(lowerQuery) || 
                       u.Email.ToLower().Contains(lowerQuery))
            .Take(20)
            .Select(u => new
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl
            })
            .ToList();

        return Ok(users);
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

        // Check if user has a password (traditional login) or not (Google login)
        var hasPassword = await _userManager.HasPasswordAsync(user);
        
        IdentityResult result;
        
        if (!hasPassword)
        {
            // User logged in with Google - add password without requiring current password
            result = await _userManager.AddPasswordAsync(user, request.NewPassword);
        }
        else
        {
            // Traditional user - change password with current password verification
            if (string.IsNullOrEmpty(request.CurrentPassword))
                return BadRequest(new { message = "Current password is required" });
                
            result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        }

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

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var googleClientId = _configuration["Google:ClientId"];
            
            // Verify the Google ID token
            var validPayload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleClientId }
            });

            if (validPayload == null)
                return Unauthorized(new { message = "Invalid Google token" });

            // Check if user exists
            var user = await _userManager.FindByEmailAsync(validPayload.Email);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    UserName = validPayload.Email,
                    Email = validPayload.Email,
                    DisplayName = validPayload.Name ?? validPayload.Email.Split('@')[0],
                    AvatarUrl = validPayload.Picture,
                    EmailConfirmed = true, // Google emails are already verified
                    IsProfilePublic = true,
                    WillingToHost = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user);
                
                if (!result.Succeeded)
                    return BadRequest(new { message = "Failed to create user", errors = result.Errors.Select(e => e.Description) });

                // Add Google login info
                await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", validPayload.Subject, "Google"));
            }
            else
            {
                // Check if Google login already linked
                var logins = await _userManager.GetLoginsAsync(user);
                if (!logins.Any(l => l.LoginProvider == "Google"))
                {
                    await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", validPayload.Subject, "Google"));
                }
            }

            // Generate JWT token
            var loginResult = await _authService.GenerateTokenForUser(user);
            
            if (loginResult == null)
                return Unauthorized(new { message = "Failed to generate token" });

            return Ok(loginResult);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Google authentication failed", error = ex.Message });
        }
    }
}

