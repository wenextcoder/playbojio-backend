using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.DTOs;
using PlayBojio.API.Models;

namespace PlayBojio.API.Services;

public interface IFriendService
{
    Task<List<FriendResponse>> GetFriendsAsync(string userId);
    Task<List<FriendRequestResponse>> GetSentFriendRequestsAsync(string userId);
    Task<List<FriendRequestResponse>> GetReceivedFriendRequestsAsync(string userId);
    Task<bool> SendFriendRequestAsync(string senderId, string receiverId);
    Task<bool> AcceptFriendRequestAsync(int requestId, string userId);
    Task<bool> RejectFriendRequestAsync(int requestId, string userId);
    Task<bool> RemoveFriendAsync(string userId, string friendId);
    Task<bool> CancelFriendRequestAsync(int requestId, string userId);
}

public class FriendService : IFriendService
{
    private readonly ApplicationDbContext _context;

    public FriendService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FriendResponse>> GetFriendsAsync(string userId)
    {
        var friends = await _context.Friends
            .Where(f => f.UserId == userId || f.FriendId == userId)
            .Include(f => f.User)
            .Include(f => f.FriendUser)
            .ToListAsync();

        return friends.Select(f =>
        {
            var friend = f.UserId == userId ? f.FriendUser : f.User;
            return new FriendResponse(
                friend.Id,
                friend.DisplayName,
                friend.AvatarUrl,
                f.CreatedAt
            );
        }).ToList();
    }

    public async Task<List<FriendRequestResponse>> GetSentFriendRequestsAsync(string userId)
    {
        var requests = await _context.FriendRequests
            .Where(fr => fr.SenderId == userId)
            .Include(fr => fr.Sender)
            .Include(fr => fr.Receiver)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync();

        return requests.Select(fr => new FriendRequestResponse(
            fr.Id,
            fr.SenderId,
            fr.Sender.DisplayName,
            fr.Sender.AvatarUrl,
            fr.ReceiverId,
            fr.Receiver.DisplayName,
            fr.Receiver.AvatarUrl,
            fr.Status.ToString(),
            fr.CreatedAt
        )).ToList();
    }

    public async Task<List<FriendRequestResponse>> GetReceivedFriendRequestsAsync(string userId)
    {
        var requests = await _context.FriendRequests
            .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
            .Include(fr => fr.Sender)
            .Include(fr => fr.Receiver)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync();

        return requests.Select(fr => new FriendRequestResponse(
            fr.Id,
            fr.SenderId,
            fr.Sender.DisplayName,
            fr.Sender.AvatarUrl,
            fr.ReceiverId,
            fr.Receiver.DisplayName,
            fr.Receiver.AvatarUrl,
            fr.Status.ToString(),
            fr.CreatedAt
        )).ToList();
    }

    public async Task<bool> SendFriendRequestAsync(string senderId, string receiverId)
    {
        // Can't send request to yourself
        if (senderId == receiverId)
            return false;

        // Check if already friends
        var existingFriendship = await _context.Friends
            .AnyAsync(f => (f.UserId == senderId && f.FriendId == receiverId) ||
                          (f.UserId == receiverId && f.FriendId == senderId));

        if (existingFriendship)
            return false;

        // Check if pending request already exists
        var existingPendingRequest = await _context.FriendRequests
            .AnyAsync(fr => ((fr.SenderId == senderId && fr.ReceiverId == receiverId) ||
                           (fr.SenderId == receiverId && fr.ReceiverId == senderId)) &&
                           fr.Status == FriendRequestStatus.Pending);

        if (existingPendingRequest)
            return false;

        // Clean up old rejected/cancelled requests between these users
        var oldRequests = await _context.FriendRequests
            .Where(fr => ((fr.SenderId == senderId && fr.ReceiverId == receiverId) ||
                         (fr.SenderId == receiverId && fr.ReceiverId == senderId)) &&
                         fr.Status != FriendRequestStatus.Pending)
            .ToListAsync();

        if (oldRequests.Any())
        {
            _context.FriendRequests.RemoveRange(oldRequests);
            await _context.SaveChangesAsync();
        }

        var friendRequest = new FriendRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.FriendRequests.Add(friendRequest);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AcceptFriendRequestAsync(int requestId, string userId)
    {
        var request = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending);

        if (request == null)
            return false;

        // Update request status
        request.Status = FriendRequestStatus.Accepted;
        request.RespondedAt = DateTime.UtcNow;

        // Create friendship (ensure UserId < FriendId for consistency)
        var userId1 = string.CompareOrdinal(request.SenderId, request.ReceiverId) < 0 ? request.SenderId : request.ReceiverId;
        var userId2 = string.CompareOrdinal(request.SenderId, request.ReceiverId) < 0 ? request.ReceiverId : request.SenderId;

        var friendship = new Friend
        {
            UserId = userId1,
            FriendId = userId2,
            CreatedAt = DateTime.UtcNow
        };

        _context.Friends.Add(friendship);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectFriendRequestAsync(int requestId, string userId)
    {
        var request = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending);

        if (request == null)
            return false;

        request.Status = FriendRequestStatus.Rejected;
        request.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFriendAsync(string userId, string friendId)
    {
        var friendship = await _context.Friends
            .FirstOrDefaultAsync(f => (f.UserId == userId && f.FriendId == friendId) ||
                                     (f.UserId == friendId && f.FriendId == userId));

        if (friendship == null)
            return false;

        _context.Friends.Remove(friendship);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelFriendRequestAsync(int requestId, string userId)
    {
        var request = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.SenderId == userId && fr.Status == FriendRequestStatus.Pending);

        if (request == null)
            return false;

        _context.FriendRequests.Remove(request);
        await _context.SaveChangesAsync();
        return true;
    }
}
