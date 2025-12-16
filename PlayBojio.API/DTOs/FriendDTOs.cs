namespace PlayBojio.API.DTOs;

// Response DTOs
public record FriendResponse(
    string UserId,
    string DisplayName,
    string? AvatarUrl,
    DateTime FriendsSince
);

public record FriendRequestResponse(
    int Id,
    string SenderId,
    string SenderName,
    string? SenderAvatarUrl,
    string ReceiverId,
    string ReceiverName,
    string? ReceiverAvatarUrl,
    string Status,
    DateTime CreatedAt
);

// Request DTOs
public record SendFriendRequestRequest(
    string ReceiverId
);
