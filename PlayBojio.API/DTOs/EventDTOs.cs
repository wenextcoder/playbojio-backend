using PlayBojio.API.Models;

namespace PlayBojio.API.DTOs;

public record CreateEventRequest(
    string Name,
    string? ImageUrl,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    string? MapLink,
    int? MaxParticipants,
    decimal? Price,
    string EventType,
    EventVisibility Visibility,
    int[]? GroupIds
);

public record UpdateEventRequest(
    string Name,
    string? ImageUrl,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    string? MapLink,
    int? MaxParticipants,
    decimal? Price,
    string EventType,
    EventVisibility Visibility,
    int[]? GroupIds
);

public record EventResponse(
    int Id,
    string Name,
    string Slug,
    string? ImageUrl,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    string? MapLink,
    int? MaxParticipants,
    decimal? Price,
    string EventType,
    EventVisibility Visibility,
    bool IsCancelled,
    string OrganizerId,
    string OrganizerName,
    int AttendeeCount,
    bool IsUserAttending,
    bool IsUserOrganizer,
    DateTime CreatedAt
);

public record EventListResponse(
    int Id,
    string Name,
    string Slug,
    string? ImageUrl,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    int AttendeeCount,
    int? MaxParticipants,
    string OrganizerName,
    decimal? Price
);

