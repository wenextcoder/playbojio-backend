namespace PlayBojio.API.Models;

public class Session
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public SessionType SessionType { get; set; }
    public int? EventId { get; set; }
    public Event? Event { get; set; }
    public string Location { get; set; } = string.Empty;
    public LocationType LocationType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? CostPerPerson { get; set; }
    public string? CostNotes { get; set; }
    public string PrimaryGame { get; set; } = string.Empty;
    public string? AdditionalGames { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public string? GameTags { get; set; }
    public bool IsNewbieFriendly { get; set; }
    public string Language { get; set; } = "English";
    public string? AdditionalNotes { get; set; }
    public SessionVisibility Visibility { get; set; } = SessionVisibility.Public;
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public string HostId { get; set; } = string.Empty;
    public User Host { get; set; } = null!;

    public ICollection<SessionAttendee> Attendees { get; set; } = new List<SessionAttendee>();
    public ICollection<SessionWaitlist> Waitlist { get; set; } = new List<SessionWaitlist>();
    public ICollection<SessionGroup> SessionGroups { get; set; } = new List<SessionGroup>();
    public ICollection<SessionInvite> SessionInvites { get; set; } = new List<SessionInvite>();
}

public enum SessionType
{
    Standalone,
    EventSession
}

public enum LocationType
{
    Home,
    Cafe,
    Other
}

public enum SessionVisibility
{
    Public,
    GroupLimited,
    InviteOnly
}

