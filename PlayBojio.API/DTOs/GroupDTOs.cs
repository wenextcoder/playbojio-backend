using PlayBojio.API.Models;

namespace PlayBojio.API.DTOs;

public record CreateGroupRequest(
    string Name,
    string Description,
    string? ProfileImageUrl,
    string? CoverImageUrl,
    GroupVisibility Visibility
);

public record UpdateGroupRequest(
    string Name,
    string Description,
    string? ProfileImageUrl,
    string? CoverImageUrl,
    GroupVisibility Visibility
);

public record GroupResponse(
    int Id,
    string Name,
    string Description,
    string? ProfileImageUrl,
    string? CoverImageUrl,
    GroupVisibility Visibility,
    string OwnerId,
    string OwnerName,
    int MemberCount,
    bool IsUserMember,
    bool IsUserAdmin,
    DateTime CreatedAt
);

public record GroupMemberResponse(
    string UserId,
    string DisplayName,
    string? AvatarUrl,
    bool IsAdmin,
    DateTime JoinedAt
);

