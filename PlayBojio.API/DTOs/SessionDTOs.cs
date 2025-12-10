using PlayBojio.API.Models;

namespace PlayBojio.API.DTOs;

public record CreateSessionRequest(
    string Title,
    string? ImageUrl,
    SessionType SessionType,
    int? EventId,
    string Location,
    LocationType LocationType,
    DateTime StartTime,
    DateTime? EndTime,
    decimal? CostPerPerson,
    string? CostNotes,
    string PrimaryGame,
    string? AdditionalGames,
    int MinPlayers,
    int MaxPlayers,
    int ReservedSlots,
    bool IsHostParticipating,
    string? GameTags,
    bool IsNewbieFriendly,
    string Language,
    string? AdditionalNotes,
    SessionVisibility Visibility,
    int[]? GroupIds,
    string[]? InvitedUserIds
);

public record UpdateSessionRequest(
    string Title,
    string? ImageUrl,
    string Location,
    LocationType LocationType,
    DateTime StartTime,
    DateTime? EndTime,
    decimal? CostPerPerson,
    string? CostNotes,
    string PrimaryGame,
    string? AdditionalGames,
    int MinPlayers,
    int MaxPlayers,
    int ReservedSlots,
    bool IsHostParticipating,
    string? GameTags,
    bool IsNewbieFriendly,
    string Language,
    string? AdditionalNotes,
    SessionVisibility Visibility,
    int[]? GroupIds,
    string[]? InvitedUserIds
);

public record SessionResponse(
    int Id,
    string Title,
    string Slug,
    string? ImageUrl,
    SessionType SessionType,
    int? EventId,
    string? EventName,
    string Location,
    LocationType LocationType,
    DateTime StartTime,
    DateTime? EndTime,
    decimal? CostPerPerson,
    string? CostNotes,
    string PrimaryGame,
    string? AdditionalGames,
    int MinPlayers,
    int MaxPlayers,
    int ReservedSlots,
    bool IsHostParticipating,
    int AvailableSlots,
    string? GameTags,
    bool IsNewbieFriendly,
    string Language,
    string? AdditionalNotes,
    SessionVisibility Visibility,
    bool IsCancelled,
    string HostId,
    string HostName,
    int CurrentPlayers,
    int WaitlistCount,
    bool IsUserAttending,
    bool IsUserOnWaitlist,
    bool IsUserHost,
    DateTime CreatedAt
);

public record SessionListResponse(
    int Id,
    string Title,
    string Slug,
    string? ImageUrl,
    string PrimaryGame,
    DateTime StartTime,
    string Location,
    int CurrentPlayers,
    int MaxPlayers,
    int AvailableSlots,
    string HostName,
    bool IsNewbieFriendly,
    decimal? CostPerPerson,
    SessionType SessionType,
    int? EventId,
    string? EventName
);

public record SessionAttendeeResponse(
    string UserId,
    string DisplayName,
    string? AvatarUrl,
    bool DidAttend,
    DateTime JoinedAt
);

