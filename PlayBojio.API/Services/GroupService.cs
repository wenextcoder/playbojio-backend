using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.DTOs;
using PlayBojio.API.Models;

namespace PlayBojio.API.Services;

public interface IGroupService
{
    Task<GroupResponse?> CreateGroupAsync(string userId, CreateGroupRequest request);
    Task<GroupResponse?> UpdateGroupAsync(int groupId, string userId, UpdateGroupRequest request);
    Task<bool> DeleteGroupAsync(int groupId, string userId);
    Task<GroupResponse?> GetGroupAsync(int groupId, string? userId);
    Task<List<GroupResponse>> GetUserGroupsAsync(string userId);
    Task<List<GroupResponse>> GetAllPublicGroupsAsync(string? userId);
    Task<bool> JoinGroupAsync(int groupId, string userId);
    Task<bool> LeaveGroupAsync(int groupId, string userId);
    Task<bool> RemoveMemberAsync(int groupId, string adminUserId, string memberUserId);
    Task<bool> PromoteToAdminAsync(int groupId, string ownerUserId, string memberUserId);
    Task<List<GroupMemberResponse>> GetGroupMembersAsync(int groupId);
}

public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _context;

    public GroupService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GroupResponse?> CreateGroupAsync(string userId, CreateGroupRequest request)
    {
        var group = new Group
        {
            Name = request.Name,
            Description = request.Description,
            ProfileImageUrl = request.ProfileImageUrl,
            CoverImageUrl = request.CoverImageUrl,
            Visibility = request.Visibility,
            OwnerId = userId
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        _context.GroupMembers.Add(new GroupMember
        {
            GroupId = group.Id,
            UserId = userId,
            IsAdmin = true
        });
        await _context.SaveChangesAsync();

        return await GetGroupAsync(group.Id, userId);
    }

    public async Task<GroupResponse?> UpdateGroupAsync(int groupId, string userId, UpdateGroupRequest request)
    {
        var group = await _context.Groups.FindAsync(groupId);

        if (group == null || group.OwnerId != userId)
            return null;

        group.Name = request.Name;
        group.Description = request.Description;
        group.ProfileImageUrl = request.ProfileImageUrl;
        group.CoverImageUrl = request.CoverImageUrl;
        group.Visibility = request.Visibility;

        await _context.SaveChangesAsync();

        return await GetGroupAsync(groupId, userId);
    }

    public async Task<bool> DeleteGroupAsync(int groupId, string userId)
    {
        var group = await _context.Groups.FindAsync(groupId);

        if (group == null || group.OwnerId != userId)
            return false;

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<GroupResponse?> GetGroupAsync(int groupId, string? userId)
    {
        var group = await _context.Groups
            .Include(g => g.Owner)
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return null;

        var memberCount = group.Members.Count;
        var isUserMember = userId != null && group.Members.Any(m => m.UserId == userId);
        var isUserAdmin = userId != null && group.Members.Any(m => m.UserId == userId && m.IsAdmin);

        return new GroupResponse(
            group.Id,
            group.Name,
            group.Description,
            group.ProfileImageUrl,
            group.CoverImageUrl,
            group.Visibility,
            group.OwnerId,
            group.Owner.DisplayName,
            memberCount,
            isUserMember,
            isUserAdmin,
            group.CreatedAt
        );
    }

    public async Task<List<GroupResponse>> GetUserGroupsAsync(string userId)
    {
        var groupIds = await _context.GroupMembers
            .Where(gm => gm.UserId == userId)
            .Select(gm => gm.GroupId)
            .ToListAsync();

        var groups = await _context.Groups
            .Include(g => g.Owner)
            .Include(g => g.Members)
            .Where(g => groupIds.Contains(g.Id))
            .ToListAsync();

        return groups.Select(g =>
        {
            var isUserAdmin = g.Members.Any(m => m.UserId == userId && m.IsAdmin);
            return new GroupResponse(
                g.Id,
                g.Name,
                g.Description,
                g.ProfileImageUrl,
                g.CoverImageUrl,
                g.Visibility,
                g.OwnerId,
                g.Owner.DisplayName,
                g.Members.Count,
                true,
                isUserAdmin,
                g.CreatedAt
            );
        }).ToList();
    }

    public async Task<List<GroupResponse>> GetAllPublicGroupsAsync(string? userId)
    {
        var groups = await _context.Groups
            .Include(g => g.Owner)
            .Include(g => g.Members)
            .Where(g => g.Visibility == GroupVisibility.Public)
            .ToListAsync();

        return groups.Select(g =>
        {
            var isUserMember = userId != null && g.Members.Any(m => m.UserId == userId);
            var isUserAdmin = userId != null && g.Members.Any(m => m.UserId == userId && m.IsAdmin);
            return new GroupResponse(
                g.Id,
                g.Name,
                g.Description,
                g.ProfileImageUrl,
                g.CoverImageUrl,
                g.Visibility,
                g.OwnerId,
                g.Owner.DisplayName,
                g.Members.Count,
                isUserMember,
                isUserAdmin,
                g.CreatedAt
            );
        }).ToList();
    }

    public async Task<bool> JoinGroupAsync(int groupId, string userId)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return false;

        if (group.Visibility == GroupVisibility.Private)
            return false;

        if (group.Members.Any(m => m.UserId == userId))
            return false;

        _context.GroupMembers.Add(new GroupMember
        {
            GroupId = groupId,
            UserId = userId,
            IsAdmin = false
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LeaveGroupAsync(int groupId, string userId)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return false;

        if (group.OwnerId == userId)
            return false;

        var member = group.Members.FirstOrDefault(m => m.UserId == userId);

        if (member == null)
            return false;

        _context.GroupMembers.Remove(member);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveMemberAsync(int groupId, string adminUserId, string memberUserId)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            return false;

        var isAdmin = group.OwnerId == adminUserId || 
            group.Members.Any(m => m.UserId == adminUserId && m.IsAdmin);

        if (!isAdmin)
            return false;

        if (group.OwnerId == memberUserId)
            return false;

        var member = group.Members.FirstOrDefault(m => m.UserId == memberUserId);

        if (member == null)
            return false;

        _context.GroupMembers.Remove(member);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PromoteToAdminAsync(int groupId, string ownerUserId, string memberUserId)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null || group.OwnerId != ownerUserId)
            return false;

        var member = group.Members.FirstOrDefault(m => m.UserId == memberUserId);

        if (member == null)
            return false;

        member.IsAdmin = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<GroupMemberResponse>> GetGroupMembersAsync(int groupId)
    {
        var members = await _context.GroupMembers
            .Where(gm => gm.GroupId == groupId)
            .Include(gm => gm.User)
            .OrderByDescending(gm => gm.IsAdmin)
            .ThenBy(gm => gm.JoinedAt)
            .ToListAsync();

        return members.Select(m => new GroupMemberResponse(
            m.UserId,
            m.User.DisplayName,
            m.User.AvatarUrl,
            m.IsAdmin,
            m.JoinedAt
        )).ToList();
    }
}

