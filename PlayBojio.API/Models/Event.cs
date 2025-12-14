namespace PlayBojio.API.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? MapLink { get; set; }
    public int? MaxParticipants { get; set; }
    public decimal? Price { get; set; }
    public string EventType { get; set; } = "Open Meetup";
    public EventVisibility Visibility { get; set; } = EventVisibility.Public;
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string OrganizerId { get; set; } = string.Empty;
    public User Organizer { get; set; } = null!;

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<EventAttendee> Attendees { get; set; } = new List<EventAttendee>();
    public ICollection<EventGroup> EventGroups { get; set; } = new List<EventGroup>();
    public ICollection<EventBlacklist> BlacklistedUsers { get; set; } = new List<EventBlacklist>();
}

public enum EventVisibility
{
    Public,
    GroupOnly,
    InviteOnly
}

