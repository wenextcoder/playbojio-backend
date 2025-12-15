namespace PlayBojio.API.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    string UserId,
    string Email,
    string DisplayName
);

public record UpdateProfileRequest(
    string DisplayName,
    string? AvatarUrl,
    string? PreferredAreas,
    string? GamePreferences,
    bool WillingToHost,
    bool IsProfilePublic
);

public record UserProfileResponse(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? PreferredAreas,
    string? GamePreferences,
    bool WillingToHost,
    bool IsProfilePublic,
    int AttendedSessions,
    int TotalSessions,
    DateTime CreatedAt
);

public record RegisterResult(
    bool Success,
    AuthResponse? Data,
    List<string>? Errors
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record ForgotPasswordRequest(
    string Email
);

public record GoogleLoginRequest(
    string IdToken
);

