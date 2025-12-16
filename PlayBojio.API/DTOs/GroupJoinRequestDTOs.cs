namespace PlayBojio.API.DTOs;

// Response DTOs
public record GroupJoinRequestResponse(
    int Id,
    int GroupId,
    string GroupName,
    string UserId,
    string UserName,
    string? UserAvatarUrl,
    string Status,
    DateTime CreatedAt,
    DateTime? RespondedAt,
    string? RespondedByUserName
);

public record MyGroupJoinRequestResponse(
    int Id,
    int GroupId,
    string GroupName,
    string? GroupProfileImageUrl,
    string Status,
    DateTime CreatedAt,
    DateTime? RespondedAt
);
