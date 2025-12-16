using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.DTOs;
using PlayBojio.API.Models;

namespace PlayBojio.API.Services;

public interface IGroupJoinRequestService
{
    Task<bool> CreateJoinRequestAsync(int groupId, string userId);
    Task<List<GroupJoinRequestResponse>> GetGroupJoinRequestsAsync(int groupId, string adminUserId);
    Task<List<MyGroupJoinRequestResponse>> GetMyJoinRequestsAsync(string userId);
    Task<bool> ApproveJoinRequestAsync(int requestId, string adminUserId);
    Task<bool> RejectJoinRequestAsync(int requestId, string adminUserId);
}

public class GroupJoinRequestService : IGroupJoinRequestService
{
    private readonly ApplicationDbContext _context;

    public GroupJoinRequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateJoinRequestAsync(int groupId, string userId)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return false;

        // Only create request for private groups
        if (group.Visibility != GroupVisibility.Private)
            return false;

        // Check if already a member
        if (group.Members.Any(m => m.UserId == userId))
            return false;

        // Check if blacklisted
        var isBlacklisted = await _context.GroupBlacklists
            .AnyAsync(gb => gb.GroupId == groupId && gb.BlacklistedUserId == userId);

        if (isBlacklisted)
            return false;

        // Check if pending request already exists
        var existingRequest = await _context.GroupJoinRequests
            .AnyAsync(gjr => gjr.GroupId == groupId && gjr.UserId == userId && gjr.Status == GroupJoinRequestStatus.Pending);

        if (existingRequest)
            return false;

        var joinRequest = new GroupJoinRequest
        {
            GroupId = groupId,
            UserId = userId,
            Status = GroupJoinRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.GroupJoinRequests.Add(joinRequest);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<GroupJoinRequestResponse>> GetGroupJoinRequestsAsync(int groupId, string adminUserId)
    {
        // Verify user is admin
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null)
            return new List<GroupJoinRequestResponse>();

        var isAdmin = group.OwnerId == adminUserId ||
                     await _context.GroupMembers
                         .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == adminUserId && gm.IsAdmin);

        if (!isAdmin)
            return new List<GroupJoinRequestResponse>();

        var requests = await _context.GroupJoinRequests
            .Where(gjr => gjr.GroupId == groupId && gjr.Status == GroupJoinRequestStatus.Pending)
            .Include(gjr => gjr.User)
            .Include(gjr => gjr.Group)
            .OrderByDescending(gjr => gjr.CreatedAt)
            .ToListAsync();

        return requests.Select(gjr => new GroupJoinRequestResponse(
            gjr.Id,
            gjr.GroupId,
            gjr.Group.Name,
            gjr.UserId,
            gjr.User.DisplayName,
            gjr.User.AvatarUrl,
            gjr.Status.ToString(),
            gjr.CreatedAt,
            gjr.RespondedAt,
            gjr.RespondedByUser?.DisplayName
        )).ToList();
    }

    public async Task<List<MyGroupJoinRequestResponse>> GetMyJoinRequestsAsync(string userId)
    {
        var requests = await _context.GroupJoinRequests
            .Where(gjr => gjr.UserId == userId)
            .Include(gjr => gjr.Group)
            .OrderByDescending(gjr => gjr.CreatedAt)
            .ToListAsync();

        return requests.Select(gjr => new MyGroupJoinRequestResponse(
            gjr.Id,
            gjr.GroupId,
            gjr.Group.Name,
            gjr.Group.ProfileImageUrl,
            gjr.Status.ToString(),
            gjr.CreatedAt,
            gjr.RespondedAt
        )).ToList();
    }

    public async Task<bool> ApproveJoinRequestAsync(int requestId, string adminUserId)
    {
        var request = await _context.GroupJoinRequests
            .Include(gjr => gjr.Group)
            .ThenInclude(g => g.Members)
            .FirstOrDefaultAsync(gjr => gjr.Id == requestId && gjr.Status == GroupJoinRequestStatus.Pending);

        if (request == null)
            return false;

        // Verify user is admin
        var isAdmin = request.Group.OwnerId == adminUserId ||
                     request.Group.Members.Any(m => m.UserId == adminUserId && m.IsAdmin);

        if (!isAdmin)
            return false;

        // Update request status
        request.Status = GroupJoinRequestStatus.Approved;
        request.RespondedAt = DateTime.UtcNow;
        request.RespondedByUserId = adminUserId;

        // Add user to group
        _context.GroupMembers.Add(new GroupMember
        {
            GroupId = request.GroupId,
            UserId = request.UserId,
            IsAdmin = false,
            JoinedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectJoinRequestAsync(int requestId, string adminUserId)
    {
        var request = await _context.GroupJoinRequests
            .Include(gjr => gjr.Group)
            .ThenInclude(g => g.Members)
            .FirstOrDefaultAsync(gjr => gjr.Id == requestId && gjr.Status == GroupJoinRequestStatus.Pending);

        if (request == null)
            return false;

        // Verify user is admin
        var isAdmin = request.Group.OwnerId == adminUserId ||
                     request.Group.Members.Any(m => m.UserId == adminUserId && m.IsAdmin);

        if (!isAdmin)
            return false;

        request.Status = GroupJoinRequestStatus.Rejected;
        request.RespondedAt = DateTime.UtcNow;
        request.RespondedByUserId = adminUserId;

        await _context.SaveChangesAsync();
        return true;
    }
}
