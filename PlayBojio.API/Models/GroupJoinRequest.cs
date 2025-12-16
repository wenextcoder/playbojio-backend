namespace PlayBojio.API.Models;

public enum GroupJoinRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>
/// Represents a request from a user to join a private group
/// </summary>
public class GroupJoinRequest
{
    public int Id { get; set; }
    
    public int GroupId { get; set; }
    
    /// <summary>
    /// User requesting to join
    /// </summary>
    public required string UserId { get; set; }
    
    public GroupJoinRequestStatus Status { get; set; } = GroupJoinRequestStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? RespondedAt { get; set; }
    
    /// <summary>
    /// Admin who approved/rejected the request
    /// </summary>
    public string? RespondedByUserId { get; set; }
    
    // Navigation properties
    public Group Group { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? RespondedByUser { get; set; }
}
