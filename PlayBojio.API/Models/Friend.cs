namespace PlayBojio.API.Models;

/// <summary>
/// Represents a friendship between two users.
/// UserId and FriendId are stored in a normalized way (lower ID first) to avoid duplicates.
/// </summary>
public class Friend
{
    public int Id { get; set; }
    
    /// <summary>
    /// The user with the lower ID (for consistency)
    /// </summary>
    public required string UserId { get; set; }
    
    /// <summary>
    /// The user with the higher ID (for consistency)
    /// </summary>
    public required string FriendId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public User FriendUser { get; set; } = null!;
}
