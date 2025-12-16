namespace PlayBojio.API.Models;

public enum GroupInvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2
}

/// <summary>
/// Represents an invitation from a group admin to a user to join a group
/// </summary>
public class GroupInvitation
{
    public int Id { get; set; }
    
    public int GroupId { get; set; }
    
    /// <summary>
    /// User being invited
    /// </summary>
    public required string InvitedUserId { get; set; }
    
    /// <summary>
    /// Admin who sent the invitation
    /// </summary>
    public required string InvitedByUserId { get; set; }
    
    public GroupInvitationStatus Status { get; set; } = GroupInvitationStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? RespondedAt { get; set; }
    
    // Navigation properties
    public Group Group { get; set; } = null!;
    public User InvitedUser { get; set; } = null!;
    public User InvitedByUser { get; set; } = null!;
}
