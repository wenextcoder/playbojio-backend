namespace PlayBojio.API.Models;

public class EventAttendee
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class SessionAttendee
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public bool DidAttend { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class SessionWaitlist
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class Blacklist
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public string BlacklistedUserId { get; set; } = string.Empty;
    public User BlacklistedUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EventGroup
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
}

public class SessionGroup
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
}

public class SessionInvite
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}

