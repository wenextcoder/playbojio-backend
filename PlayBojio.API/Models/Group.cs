namespace PlayBojio.API.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public GroupVisibility Visibility { get; set; } = GroupVisibility.Public;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string OwnerId { get; set; } = string.Empty;
    public User Owner { get; set; } = null!;

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<EventGroup> EventGroups { get; set; } = new List<EventGroup>();
    public ICollection<SessionGroup> SessionGroups { get; set; } = new List<SessionGroup>();
}

public enum GroupVisibility
{
    Public,
    Private
}

public class GroupMember
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

