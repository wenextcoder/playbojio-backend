using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.DTOs;
using PlayBojio.API.Models;

namespace PlayBojio.API.Services;

public interface IGroupInvitationService
{
    Task<bool> InviteUserToGroupAsync(int groupId, string invitedUserId, string invitedByUserId);
    Task<List<GroupInvitationResponse>> GetGroupInvitationsAsync(int groupId, string adminUserId);
    Task<List<MyGroupInvitationResponse>> GetMyInvitationsAsync(string userId);
    Task<bool> AcceptInvitationAsync(int invitationId, string userId);
    Task<bool> DeclineInvitationAsync(int invitationId, string userId);
    Task<bool> CancelInvitationAsync(int invitationId, string adminUserId);
}

public class GroupInvitationService : IGroupInvitationService
{
    private readonly ApplicationDbContext _context;

    public GroupInvitationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> InviteUserToGroupAsync(int groupId, string invitedUserId, string invitedByUserId)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return false;

        // Verify inviter is admin
        var isAdmin = group.OwnerId == invitedByUserId ||
                     group.Members.Any(m => m.UserId == invitedByUserId && m.IsAdmin);

        if (!isAdmin)
            return false;

        // Check if user is already a member
        if (group.Members.Any(m => m.UserId == invitedUserId))
            return false;

        // Check if blacklisted
        var isBlacklisted = await _context.GroupBlacklists
            .AnyAsync(gb => gb.GroupId == groupId && gb.BlacklistedUserId == invitedUserId);

        if (isBlacklisted)
            return false;

        // Check if pending invitation already exists
        var existingInvitation = await _context.GroupInvitations
            .AnyAsync(gi => gi.GroupId == groupId && gi.InvitedUserId == invitedUserId && gi.Status == GroupInvitationStatus.Pending);

        if (existingInvitation)
            return false;

        var invitation = new GroupInvitation
        {
            GroupId = groupId,
            InvitedUserId = invitedUserId,
            InvitedByUserId = invitedByUserId,
            Status = GroupInvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.GroupInvitations.Add(invitation);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<GroupInvitationResponse>> GetGroupInvitationsAsync(int groupId, string adminUserId)
    {
        // Verify user is admin
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null)
            return new List<GroupInvitationResponse>();

        var isAdmin = group.OwnerId == adminUserId ||
                     await _context.GroupMembers
                         .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == adminUserId && gm.IsAdmin);

        if (!isAdmin)
            return new List<GroupInvitationResponse>();

        var invitations = await _context.GroupInvitations
            .Where(gi => gi.GroupId == groupId)
            .Include(gi => gi.Group)
            .Include(gi => gi.InvitedUser)
            .Include(gi => gi.InvitedByUser)
            .OrderByDescending(gi => gi.CreatedAt)
            .ToListAsync();

        return invitations.Select(gi => new GroupInvitationResponse(
            gi.Id,
            gi.GroupId,
            gi.Group.Name,
            gi.Group.ProfileImageUrl,
            gi.InvitedUserId,
            gi.InvitedUser.DisplayName,
            gi.InvitedUser.AvatarUrl,
            gi.InvitedByUser.DisplayName,
            gi.Status.ToString(),
            gi.CreatedAt
        )).ToList();
    }

    public async Task<List<MyGroupInvitationResponse>> GetMyInvitationsAsync(string userId)
    {
        var invitations = await _context.GroupInvitations
            .Where(gi => gi.InvitedUserId == userId && gi.Status == GroupInvitationStatus.Pending)
            .Include(gi => gi.Group)
            .Include(gi => gi.InvitedByUser)
            .OrderByDescending(gi => gi.CreatedAt)
            .ToListAsync();

        return invitations.Select(gi => new MyGroupInvitationResponse(
            gi.Id,
            gi.GroupId,
            gi.Group.Name,
            gi.Group.ProfileImageUrl,
            gi.InvitedByUser.DisplayName,
            gi.InvitedByUser.AvatarUrl,
            gi.Status.ToString(),
            gi.CreatedAt
        )).ToList();
    }

    public async Task<bool> AcceptInvitationAsync(int invitationId, string userId)
    {
        var invitation = await _context.GroupInvitations
            .Include(gi => gi.Group)
            .FirstOrDefaultAsync(gi => gi.Id == invitationId && 
                                      gi.InvitedUserId == userId && 
                                      gi.Status == GroupInvitationStatus.Pending);

        if (invitation == null)
            return false;

        // Update invitation status
        invitation.Status = GroupInvitationStatus.Accepted;
        invitation.RespondedAt = DateTime.UtcNow;

        // Add user to group
        _context.GroupMembers.Add(new GroupMember
        {
            GroupId = invitation.GroupId,
            UserId = userId,
            IsAdmin = false,
            JoinedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeclineInvitationAsync(int invitationId, string userId)
    {
        var invitation = await _context.GroupInvitations
            .FirstOrDefaultAsync(gi => gi.Id == invitationId && 
                                      gi.InvitedUserId == userId && 
                                      gi.Status == GroupInvitationStatus.Pending);

        if (invitation == null)
            return false;

        invitation.Status = GroupInvitationStatus.Declined;
        invitation.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelInvitationAsync(int invitationId, string adminUserId)
    {
        var invitation = await _context.GroupInvitations
            .Include(gi => gi.Group)
            .ThenInclude(g => g.Members)
            .FirstOrDefaultAsync(gi => gi.Id == invitationId && gi.Status == GroupInvitationStatus.Pending);

        if (invitation == null)
            return false;

        // Verify user is admin
        var isAdmin = invitation.Group.OwnerId == adminUserId ||
                     invitation.Group.Members.Any(m => m.UserId == adminUserId && m.IsAdmin);

        if (!isAdmin)
            return false;

        _context.GroupInvitations.Remove(invitation);
        await _context.SaveChangesAsync();
        return true;
    }
}
