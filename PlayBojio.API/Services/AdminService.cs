using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.Models;

namespace PlayBojio.API.Services;

public interface IAdminService
{
    Task<object> GetStatsAsync();
    Task<List<object>> GetUsersAsync(int pageSize);
    Task<object?> GetUserDetailAsync(string userId);
    Task<bool> SuspendUserAsync(string userId);
    Task<bool> UnsuspendUserAsync(string userId);
    Task<bool> DeleteUserAsync(string userId);
    Task<List<object>> GetEventsAsync(int pageSize);
    Task<bool> DeleteEventAsync(int id);
    Task<List<object>> GetSessionsAsync(int pageSize);
    Task<bool> DeleteSessionAsync(int id);
    Task<List<object>> GetGroupsAsync(int pageSize);
    Task<bool> DeleteGroupAsync(int id);
}

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public AdminService(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<object> GetStatsAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalSessions = await _context.Sessions.CountAsync();
        var activeSessions = await _context.Sessions
            .Where(s => !s.IsCancelled && s.StartTime > DateTime.UtcNow)
            .CountAsync();
        var totalEvents = await _context.Events.CountAsync();
        var upcomingEvents = await _context.Events
            .Where(e => !e.IsCancelled && e.StartDate > DateTime.UtcNow)
            .CountAsync();
        var totalGroups = await _context.Groups.CountAsync();

        return new
        {
            totalUsers,
            totalSessions,
            activeSessions,
            totalEvents,
            upcomingEvents,
            totalGroups
        };
    }

    public async Task<List<object>> GetUsersAsync(int pageSize)
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.Email,
                u.EmailConfirmed,
                u.CreatedAt,
                u.LockoutEnd
            })
            .ToListAsync();

        return users.Cast<object>().ToList();
    }

    public async Task<object?> GetUserDetailAsync(string userId)
    {
        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.DisplayName,
                u.Email,
                u.EmailConfirmed,
                u.AvatarUrl,
                u.PreferredAreas,
                u.GamePreferences,
                u.WillingToHost,
                u.IsProfilePublic,
                u.CreatedAt,
                u.LockoutEnd,
                CreatedSessions = u.CreatedSessions
                    .Select(s => new
                    {
                        s.Id,
                        s.Title,
                        s.PrimaryGame,
                        s.StartTime,
                        s.Location,
                        s.IsCancelled
                    })
                    .ToList(),
                CreatedEvents = u.CreatedEvents
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.StartDate,
                        e.Location,
                        e.IsCancelled
                    })
                    .ToList(),
                OwnedGroups = u.OwnedGroups
                    .Select(g => new
                    {
                        g.Id,
                        g.Name,
                        g.Visibility,
                        MembersCount = g.Members.Count
                    })
                    .ToList(),
                JoinedGroups = u.GroupMemberships
                    .Select(gm => new
                    {
                        gm.Group.Id,
                        gm.Group.Name,
                        JoinedAt = gm.JoinedAt,
                        MembersCount = gm.Group.Members.Count
                    })
                    .ToList(),
                AttendedSessions = u.SessionAttendances
                    .Select(sa => new
                    {
                        sa.Session.Id,
                        sa.Session.Title,
                        sa.Session.PrimaryGame,
                        sa.Session.StartTime,
                        JoinedAt = sa.JoinedAt
                    })
                    .ToList(),
                AttendedEvents = u.EventAttendances
                    .Select(ea => new
                    {
                        ea.Event.Id,
                        ea.Event.Name,
                        ea.Event.StartDate,
                        ea.Event.Location,
                        JoinedAt = ea.JoinedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return user;
    }

    public async Task<bool> SuspendUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        // Suspend for 100 years (effectively permanent until unsuspended)
        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        user.LockoutEnabled = true;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> UnsuspendUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        user.LockoutEnd = null;
        user.LockoutEnabled = false;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<List<object>> GetEventsAsync(int pageSize)
    {
        var events = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .Include(e => e.Sessions)
            .OrderByDescending(e => e.CreatedAt)
            .Take(pageSize)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.StartDate,
                e.EndDate,
                e.Location,
                OrganizerName = e.Organizer.DisplayName,
                e.IsCancelled,
                AttendeesCount = e.Attendees.Count,
                SessionsCount = e.Sessions.Count
            })
            .ToListAsync();

        return events.Cast<object>().ToList();
    }

    public async Task<bool> DeleteEventAsync(int id)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt == null) return false;

        _context.Events.Remove(evt);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<object>> GetSessionsAsync(int pageSize)
    {
        var sessions = await _context.Sessions
            .Include(s => s.Host)
            .Include(s => s.Attendees)
            .OrderByDescending(s => s.CreatedAt)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.PrimaryGame,
                s.StartTime,
                s.Location,
                HostName = s.Host.DisplayName,
                s.MaxPlayers,
                s.IsCancelled,
                AttendeesCount = s.Attendees.Count
            })
            .ToListAsync();

        return sessions.Cast<object>().ToList();
    }

    public async Task<bool> DeleteSessionAsync(int id)
    {
        var session = await _context.Sessions.FindAsync(id);
        if (session == null) return false;

        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<object>> GetGroupsAsync(int pageSize)
    {
        var groups = await _context.Groups
            .Include(g => g.Owner)
            .Include(g => g.Members)
            .OrderByDescending(g => g.CreatedAt)
            .Take(pageSize)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Description,
                OwnerName = g.Owner.DisplayName,
                g.Visibility,
                g.CreatedAt,
                MembersCount = g.Members.Count
            })
            .ToListAsync();

        return groups.Cast<object>().ToList();
    }

    public async Task<bool> DeleteGroupAsync(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null) return false;

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        return true;
    }
}
