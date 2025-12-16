using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.DTOs;
using PlayBojio.API.Models;
using PlayBojio.API.Utils;

namespace PlayBojio.API.Services;

public interface ISessionService
{
    Task<SessionResponse?> CreateSessionAsync(string userId, CreateSessionRequest request);
    Task<SessionResponse?> UpdateSessionAsync(int sessionId, string userId, UpdateSessionRequest request);
    Task<bool> CancelSessionAsync(int sessionId, string userId);
    Task<SessionResponse?> GetSessionAsync(int sessionId, string? userId);
    Task<SessionResponse?> GetSessionBySlugAsync(string slug, string? userId);
    Task<PaginatedResult<SessionListResponse>> SearchSessionsAsync(string? userId, DateTime? fromDate, DateTime? toDate,
        string? location, string? gameType, bool? availableOnly, bool? newbieFriendly, string? searchText, int page = 1, int pageSize = 30);
    Task<List<SessionListResponse>> GetUserSessionsAsync(string userId);
    Task<List<SessionListResponse>> GetUserAttendingSessionsAsync(string userId);
    Task<List<SessionListResponse>> GetEventSessionsAsync(int eventId, string? userId);
    Task<bool> JoinSessionAsync(int sessionId, string userId);
    Task<bool> LeaveSessionAsync(int sessionId, string userId);
    Task<List<SessionAttendeeResponse>> GetSessionAttendeesAsync(int sessionId);
    Task<List<SessionAttendeeResponse>> GetSessionWaitlistAsync(int sessionId);
}

public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _context;

    public SessionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SessionResponse?> CreateSessionAsync(string userId, CreateSessionRequest request)
    {
        // Validate event session requirements
        if (request.SessionType == SessionType.EventSession)
        {
            if (request.EventId == null)
                return null;

            // Check if user is the organizer of the event
            var eventToCheck = await _context.Events
                .FirstOrDefaultAsync(e => e.Id == request.EventId);

            if (eventToCheck == null || eventToCheck.OrganizerId != userId)
                return null;
        }

        // Generate unique slug
        var baseSlug = SlugHelper.GenerateSlug(request.Title);
        var slug = baseSlug;
        int counter = 1;
        while (await _context.Sessions.AnyAsync(s => s.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        var session = new Session
        {
            Title = request.Title,
            Slug = slug,
            ImageUrl = request.ImageUrl,
            SessionType = request.SessionType,
            EventId = request.EventId,
            Location = request.Location,
            LocationType = request.LocationType,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CostPerPerson = request.CostPerPerson,
            CostNotes = request.CostNotes,
            PrimaryGame = request.PrimaryGame,
            AdditionalGames = request.AdditionalGames,
            MinPlayers = request.MinPlayers,
            MaxPlayers = request.MaxPlayers,
            ReservedSlots = request.ReservedSlots,
            IsHostParticipating = request.IsHostParticipating,
            GameTags = request.GameTags,
            IsNewbieFriendly = request.IsNewbieFriendly,
            Language = request.Language,
            AdditionalNotes = request.AdditionalNotes,
            Visibility = request.Visibility,
            HostId = userId
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Add group associations
        if (request.GroupIds != null && request.GroupIds.Length > 0)
        {
            foreach (var groupId in request.GroupIds)
            {
                _context.SessionGroups.Add(new SessionGroup
                {
                    SessionId = session.Id,
                    GroupId = groupId
                });
            }
            await _context.SaveChangesAsync();
        }

        // Add invites
        if (request.InvitedUserIds != null && request.InvitedUserIds.Length > 0)
        {
            foreach (var invitedUserId in request.InvitedUserIds)
            {
                _context.SessionInvites.Add(new SessionInvite
                {
                    SessionId = session.Id,
                    UserId = invitedUserId
                });
            }
            await _context.SaveChangesAsync();
        }

        // Host automatically joins
        _context.SessionAttendees.Add(new SessionAttendee
        {
            SessionId = session.Id,
            UserId = userId
        });
        await _context.SaveChangesAsync();

        return await GetSessionAsync(session.Id, userId);
    }

    public async Task<SessionResponse?> UpdateSessionAsync(int sessionId, string userId, UpdateSessionRequest request)
    {
        var session = await _context.Sessions.FindAsync(sessionId);

        if (session == null || session.HostId != userId)
            return null;

        // Regenerate slug if title changed
        if (session.Title != request.Title)
        {
            var baseSlug = SlugHelper.GenerateSlug(request.Title);
            var slug = baseSlug;
            int counter = 1;
            while (await _context.Sessions.AnyAsync(s => s.Slug == slug && s.Id != sessionId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
            session.Slug = slug;
        }
        
        session.Title = request.Title;
        session.ImageUrl = request.ImageUrl;
        session.Location = request.Location;
        session.LocationType = request.LocationType;
        session.StartTime = request.StartTime;
        session.EndTime = request.EndTime;
        session.CostPerPerson = request.CostPerPerson;
        session.CostNotes = request.CostNotes;
        session.PrimaryGame = request.PrimaryGame;
        session.AdditionalGames = request.AdditionalGames;
        session.MinPlayers = request.MinPlayers;
        session.MaxPlayers = request.MaxPlayers;
        session.ReservedSlots = request.ReservedSlots;
        session.IsHostParticipating = request.IsHostParticipating;
        session.GameTags = request.GameTags;
        session.IsNewbieFriendly = request.IsNewbieFriendly;
        session.Language = request.Language;
        session.AdditionalNotes = request.AdditionalNotes;
        session.Visibility = request.Visibility;
        session.UpdatedAt = DateTime.UtcNow;

        // Update groups
        var existingGroups = await _context.SessionGroups
            .Where(sg => sg.SessionId == sessionId)
            .ToListAsync();
        _context.SessionGroups.RemoveRange(existingGroups);

        if (request.GroupIds != null && request.GroupIds.Length > 0)
        {
            foreach (var groupId in request.GroupIds)
            {
                _context.SessionGroups.Add(new SessionGroup
                {
                    SessionId = sessionId,
                    GroupId = groupId
                });
            }
        }

        // Update invites
        var existingInvites = await _context.SessionInvites
            .Where(si => si.SessionId == sessionId)
            .ToListAsync();
        _context.SessionInvites.RemoveRange(existingInvites);

        if (request.InvitedUserIds != null && request.InvitedUserIds.Length > 0)
        {
            foreach (var invitedUserId in request.InvitedUserIds)
            {
                _context.SessionInvites.Add(new SessionInvite
                {
                    SessionId = sessionId,
                    UserId = invitedUserId
                });
            }
        }

        await _context.SaveChangesAsync();

        return await GetSessionAsync(sessionId, userId);
    }

    public async Task<bool> CancelSessionAsync(int sessionId, string userId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);

        if (session == null || session.HostId != userId)
            return false;

        session.IsCancelled = true;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SessionResponse?> GetSessionAsync(int sessionId, string? userId)
    {
        var session = await _context.Sessions
            .Include(s => s.Host)
            .Include(s => s.Event)
            .Include(s => s.Attendees)
            .Include(s => s.Waitlist)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            return null;

        // Check visibility
        if (userId != null && !await CanUserViewSession(session, userId))
            return null;

        var currentPlayers = session.Attendees.Count;
        var waitlistCount = session.Waitlist.Count;
        var isUserAttending = userId != null && session.Attendees.Any(a => a.UserId == userId);
        var isUserOnWaitlist = userId != null && session.Waitlist.Any(w => w.UserId == userId);
        var isUserHost = userId == session.HostId;
        var hostCount = session.IsHostParticipating ? 1 : 0;
        var availableSlots = session.MaxPlayers - (currentPlayers + session.ReservedSlots + hostCount);
        
        // Check if user is a member of the event (for event sessions)
        var isUserEventMember = false;
        if (userId != null && session.EventId != null)
        {
            isUserEventMember = await _context.EventAttendees
                .AnyAsync(ea => ea.EventId == session.EventId && ea.UserId == userId);
        }

        return new SessionResponse(
            session.Id,
            session.Title,
            session.Slug,
            session.ImageUrl,
            session.SessionType,
            session.EventId,
            session.Event?.Name,
            session.Location,
            session.LocationType,
            session.StartTime,
            session.EndTime,
            session.CostPerPerson,
            session.CostNotes,
            session.PrimaryGame,
            session.AdditionalGames,
            session.MinPlayers,
            session.MaxPlayers,
            session.ReservedSlots,
            session.IsHostParticipating,
            availableSlots,
            session.GameTags,
            session.IsNewbieFriendly,
            session.Language,
            session.AdditionalNotes,
            session.Visibility,
            session.IsCancelled,
            session.HostId,
            session.Host.DisplayName,
            session.Host.AvatarUrl,
            currentPlayers,
            waitlistCount,
            isUserAttending,
            isUserOnWaitlist,
            isUserHost,
            isUserEventMember,
            session.CreatedAt
        );
    }

    public async Task<SessionResponse?> GetSessionBySlugAsync(string slug, string? userId)
    {
        var session = await _context.Sessions
            .Include(s => s.Host)
            .Include(s => s.Event)
            .Include(s => s.Attendees)
            .Include(s => s.Waitlist)
            .FirstOrDefaultAsync(s => s.Slug == slug);

        if (session == null)
            return null;

        // Check visibility
        if (userId != null && !await CanUserViewSession(session, userId))
            return null;

        var currentPlayers = session.Attendees.Count;
        var waitlistCount = session.Waitlist.Count;
        var isUserAttending = userId != null && session.Attendees.Any(a => a.UserId == userId);
        var isUserOnWaitlist = userId != null && session.Waitlist.Any(w => w.UserId == userId);
        var isUserHost = userId == session.HostId;
        var hostCount = session.IsHostParticipating ? 1 : 0;
        var availableSlots = session.MaxPlayers - (currentPlayers + session.ReservedSlots + hostCount);
        
        // Check if user is a member of the event (for event sessions)
        var isUserEventMember = false;
        if (userId != null && session.EventId != null)
        {
            isUserEventMember = await _context.EventAttendees
                .AnyAsync(ea => ea.EventId == session.EventId && ea.UserId == userId);
        }

        return new SessionResponse(
            session.Id,
            session.Title,
            session.Slug,
            session.ImageUrl,
            session.SessionType,
            session.EventId,
            session.Event?.Name,
            session.Location,
            session.LocationType,
            session.StartTime,
            session.EndTime,
            session.CostPerPerson,
            session.CostNotes,
            session.PrimaryGame,
            session.AdditionalGames,
            session.MinPlayers,
            session.MaxPlayers,
            session.ReservedSlots,
            session.IsHostParticipating,
            availableSlots,
            session.GameTags,
            session.IsNewbieFriendly,
            session.Language,
            session.AdditionalNotes,
            session.Visibility,
            session.IsCancelled,
            session.HostId,
            session.Host.DisplayName,
            session.Host.AvatarUrl,
            currentPlayers,
            waitlistCount,
            isUserAttending,
            isUserOnWaitlist,
            isUserHost,
            isUserEventMember,
            session.CreatedAt
        );
    }

    public async Task<PaginatedResult<SessionListResponse>> SearchSessionsAsync(string? userId, DateTime? fromDate, 
        DateTime? toDate, string? location, string? gameType, bool? availableOnly, bool? newbieFriendly, string? searchText, int page = 1, int pageSize = 30)
    {
        var query = _context.Sessions
            .Include(s => s.Host)
            .Include(s => s.Attendees)
            .Include(s => s.Event)
            .Where(s => !s.IsCancelled);

        if (fromDate.HasValue)
            query = query.Where(s => s.StartTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.StartTime <= toDate.Value);

        if (!string.IsNullOrEmpty(location))
            query = query.Where(s => s.Location.Contains(location));

        if (!string.IsNullOrEmpty(gameType))
            query = query.Where(s => s.GameTags != null && s.GameTags.Contains(gameType));

        if (newbieFriendly.HasValue)
            query = query.Where(s => s.IsNewbieFriendly == newbieFriendly.Value);

        // Text search across title, primary game, description, location, game tags, and event name
        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(s =>
                s.Title.Contains(searchText) ||
                s.PrimaryGame.Contains(searchText) ||
                (s.AdditionalNotes != null && s.AdditionalNotes.Contains(searchText)) ||
                s.Location.Contains(searchText) ||
                (s.GameTags != null && s.GameTags.Contains(searchText)) ||
                (s.Event != null && s.Event.Name.Contains(searchText)));
        }

        var totalCountQuery = query;
        var sessions = await query
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        // Filter by blacklist and visibility
        if (userId != null)
        {
            var blacklistedByHosts = await _context.Blacklists
                .Where(b => b.BlacklistedUserId == userId)
                .Select(b => b.UserId)
                .ToListAsync();

            sessions = sessions.Where(s => !blacklistedByHosts.Contains(s.HostId)).ToList();

            // Preload all invites and group memberships to avoid repeated DB calls
            var sessionIds = sessions.Select(s => s.Id).ToList();
            
            var invites = await _context.SessionInvites
                .Where(si => sessionIds.Contains(si.SessionId) && si.UserId == userId)
                .Select(si => si.SessionId)
                .ToListAsync();

            var sessionGroups = await _context.SessionGroups
                .Where(sg => sessionIds.Contains(sg.SessionId))
                .Select(sg => new { sg.SessionId, sg.GroupId })
                .ToListAsync();

            var userGroupIds = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            // Filter by visibility using preloaded data
            sessions = sessions.Where(s => 
            {
                // Public sessions are visible to everyone
                if (s.Visibility == SessionVisibility.Public)
                    return true;

                // Host can always see their own sessions
                if (s.HostId == userId)
                    return true;

                // Invite-only sessions
                if (s.Visibility == SessionVisibility.InviteOnly)
                    return invites.Contains(s.Id);

                // Group-limited sessions
                if (s.Visibility == SessionVisibility.GroupLimited)
                {
                    var sessionGroupIds = sessionGroups
                        .Where(sg => sg.SessionId == s.Id)
                        .Select(sg => sg.GroupId)
                        .ToList();
                    
                    var isInGroup = sessionGroupIds.Any(gid => userGroupIds.Contains(gid));
                    var isInvited = invites.Contains(s.Id);
                    
                    return isInGroup || isInvited;
                }

                return false;
            }).ToList();
        }

        if (availableOnly == true)
            sessions = sessions.Where(s => s.Attendees.Count < s.MaxPlayers).ToList();

        var totalCount = sessions.Count;
        
        var paginatedSessions = sessions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = paginatedSessions.Select(s => {
            var hostCount = s.IsHostParticipating ? 1 : 0;
            var availableSlots = s.MaxPlayers - (s.Attendees.Count + s.ReservedSlots + hostCount);
            return new SessionListResponse(
                s.Id,
                s.Title,
                s.Slug,
                s.ImageUrl,
                s.PrimaryGame,
                s.StartTime,
                s.Location,
                s.Attendees.Count,
                s.MaxPlayers,
                availableSlots,
                s.Host.DisplayName,
                s.IsNewbieFriendly,
                s.CostPerPerson,
                s.SessionType,
                s.EventId,
                s.Event?.Name
            );
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResult<SessionListResponse>(
            items,
            totalCount,
            page,
            pageSize,
            totalPages
        );
    }

    public async Task<bool> JoinSessionAsync(int sessionId, string userId)
    {
        var session = await _context.Sessions
            .Include(s => s.Attendees)
            .Include(s => s.Waitlist)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null || session.IsCancelled)
            return false;

        // Check if already attending
        if (session.Attendees.Any(a => a.UserId == userId))
            return false;

        // Check if already on waitlist
        if (session.Waitlist.Any(w => w.UserId == userId))
            return false;

        // Check blacklist
        var isBlacklisted = await _context.Blacklists
            .AnyAsync(b => b.UserId == session.HostId && b.BlacklistedUserId == userId);

        if (isBlacklisted)
            return false;

        // Check visibility
        if (!await CanUserViewSession(session, userId))
            return false;

        // For event sessions, check if user is attending the event
        if (session.SessionType == SessionType.EventSession && session.EventId != null)
        {
            var isAttendingEvent = await _context.EventAttendees
                .AnyAsync(ea => ea.EventId == session.EventId && ea.UserId == userId);

            if (!isAttendingEvent)
                return false;
        }

        // Check capacity
        if (session.Attendees.Count < session.MaxPlayers)
        {
            _context.SessionAttendees.Add(new SessionAttendee
            {
                SessionId = sessionId,
                UserId = userId
            });
        }
        else
        {
            _context.SessionWaitlists.Add(new SessionWaitlist
            {
                SessionId = sessionId,
                UserId = userId
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LeaveSessionAsync(int sessionId, string userId)
    {
        var session = await _context.Sessions
            .Include(s => s.Attendees)
            .Include(s => s.Waitlist)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            return false;

        var attendee = session.Attendees.FirstOrDefault(a => a.UserId == userId);

        if (attendee == null)
            return false;

        // Can't leave if you're the host
        if (session.HostId == userId)
            return false;

        _context.SessionAttendees.Remove(attendee);

        // Promote from waitlist
        var nextWaitlisted = session.Waitlist.OrderBy(w => w.JoinedAt).FirstOrDefault();
        if (nextWaitlisted != null)
        {
            _context.SessionAttendees.Add(new SessionAttendee
            {
                SessionId = sessionId,
                UserId = nextWaitlisted.UserId
            });
            _context.SessionWaitlists.Remove(nextWaitlisted);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SessionAttendeeResponse>> GetSessionAttendeesAsync(int sessionId)
    {
        var attendees = await _context.SessionAttendees
            .Where(sa => sa.SessionId == sessionId)
            .Include(sa => sa.User)
            .OrderBy(sa => sa.JoinedAt)
            .ToListAsync();

        return attendees.Select(a => new SessionAttendeeResponse(
            a.UserId,
            a.User.DisplayName,
            a.User.AvatarUrl,
            a.DidAttend,
            a.JoinedAt
        )).ToList();
    }

    public async Task<List<SessionAttendeeResponse>> GetSessionWaitlistAsync(int sessionId)
    {
        var waitlist = await _context.SessionWaitlists
            .Where(sw => sw.SessionId == sessionId)
            .Include(sw => sw.User)
            .OrderBy(sw => sw.JoinedAt)
            .ToListAsync();

        return waitlist.Select(w => new SessionAttendeeResponse(
            w.UserId,
            w.User.DisplayName,
            w.User.AvatarUrl,
            false,
            w.JoinedAt
        )).ToList();
    }

    private async Task<bool> CanUserViewSession(Session session, string userId)
    {
        if (session.Visibility == SessionVisibility.Public)
            return true;

        if (session.HostId == userId)
            return true;

        if (session.Visibility == SessionVisibility.InviteOnly)
        {
            return await _context.SessionInvites
                .AnyAsync(si => si.SessionId == session.Id && si.UserId == userId);
        }

        if (session.Visibility == SessionVisibility.GroupLimited)
        {
            var sessionGroupIds = await _context.SessionGroups
                .Where(sg => sg.SessionId == session.Id)
                .Select(sg => sg.GroupId)
                .ToListAsync();

            var isInGroup = await _context.GroupMembers
                .AnyAsync(gm => sessionGroupIds.Contains(gm.GroupId) && gm.UserId == userId);

            var isInvited = await _context.SessionInvites
                .AnyAsync(si => si.SessionId == session.Id && si.UserId == userId);

            return isInGroup || isInvited;
        }

        return false;
    }

    public async Task<List<SessionListResponse>> GetUserSessionsAsync(string userId)
    {
        var sessions = await _context.Sessions
            .Include(s => s.Host)
            .Include(s => s.Attendees)
            .Include(s => s.Event)
            .Where(s => s.HostId == userId && !s.IsCancelled)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        return sessions.Select(s => {
            var hostCount = s.IsHostParticipating ? 1 : 0;
            var availableSlots = s.MaxPlayers - (s.Attendees.Count + s.ReservedSlots + hostCount);
            return new SessionListResponse(
                s.Id,
                s.Title,
                s.Slug,
                s.ImageUrl,
                s.PrimaryGame,
                s.StartTime,
                s.Location,
                s.Attendees.Count,
                s.MaxPlayers,
                availableSlots,
                s.Host.DisplayName,
                s.IsNewbieFriendly,
                s.CostPerPerson,
                s.SessionType,
                s.EventId,
                s.Event?.Name
            );
        }).ToList();
    }

    public async Task<List<SessionListResponse>> GetUserAttendingSessionsAsync(string userId)
    {
        // Get sessions where user is an attendee (not the host)
        var attendingSessionIds = await _context.SessionAttendees
            .Where(sa => sa.UserId == userId)
            .Select(sa => sa.SessionId)
            .ToListAsync();

        var sessions = await _context.Sessions
            .Include(s => s.Host)
            .Include(s => s.Attendees)
            .Include(s => s.Event)
            .Where(s => attendingSessionIds.Contains(s.Id) && !s.IsCancelled)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

        return sessions.Select(s => {
            var hostCount = s.IsHostParticipating ? 1 : 0;
            var availableSlots = s.MaxPlayers - (s.Attendees.Count + s.ReservedSlots + hostCount);
            return new SessionListResponse(
                s.Id,
                s.Title,
                s.Slug,
                s.ImageUrl,
                s.PrimaryGame,
                s.StartTime,
                s.Location,
                s.Attendees.Count,
                s.MaxPlayers,
                availableSlots,
                s.Host.DisplayName,
                s.IsNewbieFriendly,
                s.CostPerPerson,
                s.SessionType,
                s.EventId,
                s.Event?.Name
            );
        }).ToList();
    }

    public async Task<List<SessionListResponse>> GetEventSessionsAsync(int eventId, string? userId)
    {
        var sessions = await _context.Sessions
            .Include(s => s.Host)
            .Include(s => s.Attendees)
            .Include(s => s.Event)
            .Where(s => s.EventId == eventId && !s.IsCancelled)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        // Filter by visibility if user is specified
        if (userId != null)
        {
            var sessionIds = sessions.Select(s => s.Id).ToList();
            
            var invites = await _context.SessionInvites
                .Where(si => sessionIds.Contains(si.SessionId) && si.UserId == userId)
                .Select(si => si.SessionId)
                .ToListAsync();

            var sessionGroups = await _context.SessionGroups
                .Where(sg => sessionIds.Contains(sg.SessionId))
                .Select(sg => new { sg.SessionId, sg.GroupId })
                .ToListAsync();

            var userGroupIds = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            sessions = sessions.Where(s => 
            {
                if (s.Visibility == SessionVisibility.Public)
                    return true;

                if (s.HostId == userId)
                    return true;

                if (s.Visibility == SessionVisibility.InviteOnly)
                    return invites.Contains(s.Id);

                if (s.Visibility == SessionVisibility.GroupLimited)
                {
                    var sessionGroupIds = sessionGroups
                        .Where(sg => sg.SessionId == s.Id)
                        .Select(sg => sg.GroupId)
                        .ToList();
                    
                    var isInGroup = sessionGroupIds.Any(gid => userGroupIds.Contains(gid));
                    var isInvited = invites.Contains(s.Id);
                    
                    return isInGroup || isInvited;
                }

                return false;
            }).ToList();
        }

        return sessions.Select(s => {
            var hostCount = s.IsHostParticipating ? 1 : 0;
            var availableSlots = s.MaxPlayers - (s.Attendees.Count + s.ReservedSlots + hostCount);
            return new SessionListResponse(
                s.Id,
                s.Title,
                s.Slug,
                s.ImageUrl,
                s.PrimaryGame,
                s.StartTime,
                s.Location,
                s.Attendees.Count,
                s.MaxPlayers,
                availableSlots,
                s.Host.DisplayName,
                s.IsNewbieFriendly,
                s.CostPerPerson,
                s.SessionType,
                s.EventId,
                s.Event?.Name
            );
        }).ToList();
    }
}

