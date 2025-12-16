namespace PlayBojio.API.DTOs;

// Response DTOs
public record GroupInvitationResponse(
    int Id,
    int GroupId,
    string GroupName,
    string? GroupProfileImageUrl,
    string InvitedUserId,
    string InvitedUserName,
    string? InvitedUserAvatarUrl,
    string InvitedByUserName,
    string Status,
    DateTime CreatedAt
);

public record MyGroupInvitationResponse(
    int Id,
    int GroupId,
    string GroupName,
    string? GroupProfileImageUrl,
    string InvitedByUserName,
    string? InvitedByUserAvatarUrl,
    string Status,
    DateTime CreatedAt
);

// Request DTOs
public record InviteUserToGroupRequest(
    string UserId
);
