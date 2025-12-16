namespace PlayBojio.API.Models;

public enum FriendRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

/// <summary>
/// Represents a friend request from one user to another
/// </summary>
public class FriendRequest
{
    public int Id { get; set; }
    
    /// <summary>
    /// User who sent the friend request
    /// </summary>
    public required string SenderId { get; set; }
    
    /// <summary>
    /// User who received the friend request
    /// </summary>
    public required string ReceiverId { get; set; }
    
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? RespondedAt { get; set; }
    
    // Navigation properties
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}
