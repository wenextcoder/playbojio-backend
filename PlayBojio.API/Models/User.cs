using Microsoft.AspNetCore.Identity;

namespace PlayBojio.API.Models;

public class User : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? PreferredAreas { get; set; }
    public string? GamePreferences { get; set; }
    public bool WillingToHost { get; set; }
    public bool IsProfilePublic { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int AttendedSessions { get; set; }
    public int TotalSessions { get; set; }

    public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
    public ICollection<Session> CreatedSessions { get; set; } = new List<Session>();
    public ICollection<EventAttendee> EventAttendances { get; set; } = new List<EventAttendee>();
    public ICollection<SessionAttendee> SessionAttendances { get; set; } = new List<SessionAttendee>();
    public ICollection<Group> OwnedGroups { get; set; } = new List<Group>();
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public ICollection<Blacklist> BlacklistedUsers { get; set; } = new List<Blacklist>();
    public ICollection<Blacklist> BlacklistedByUsers { get; set; } = new List<Blacklist>();
}

