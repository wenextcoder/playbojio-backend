using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Data;
using PlayBojio.API.DTOs;
using PlayBojio.API.Models;
using PlayBojio.API.Utils;

namespace PlayBojio.API.Services;

public interface IEventService
{
    Task<EventResponse?> CreateEventAsync(string userId, CreateEventRequest request);
    Task<EventResponse?> UpdateEventAsync(int eventId, string userId, UpdateEventRequest request);
    Task<bool> CancelEventAsync(int eventId, string userId);
    Task<EventResponse?> GetEventAsync(int eventId, string? userId);
    Task<EventResponse?> GetEventBySlugAsync(string slug, string? userId);
    Task<PaginatedResult<EventListResponse>> SearchEventsAsync(string? userId, DateTime? fromDate, DateTime? toDate, string? location, string? searchText, int page = 1, int pageSize = 30);
    Task<List<EventListResponse>> GetUserEventsAsync(string userId);
    Task<bool> JoinEventAsync(int eventId, string userId);
    Task<bool> LeaveEventAsync(int eventId, string userId);
}

public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;

    public EventService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventResponse?> CreateEventAsync(string userId, CreateEventRequest request)
    {
        // Generate unique slug
        var baseSlug = SlugHelper.GenerateSlug(request.Name);
        var slug = baseSlug;
        int counter = 1;
        while (await _context.Events.AnyAsync(e => e.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        var evt = new Event
        {
            Name = request.Name,
            Slug = slug,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Location = request.Location,
            MapLink = request.MapLink,
            MaxParticipants = request.MaxParticipants,
            Price = request.Price,
            EventType = request.EventType,
            Visibility = request.Visibility,
            OrganizerId = userId
        };

        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        if (request.GroupIds != null && request.GroupIds.Length > 0)
        {
            foreach (var groupId in request.GroupIds)
            {
                _context.EventGroups.Add(new EventGroup
                {
                    EventId = evt.Id,
                    GroupId = groupId
                });
            }
            await _context.SaveChangesAsync();
        }

        // Organizer automatically joins
        _context.EventAttendees.Add(new EventAttendee
        {
            EventId = evt.Id,
            UserId = userId
        });
        await _context.SaveChangesAsync();

        return await GetEventAsync(evt.Id, userId);
    }

    public async Task<EventResponse?> UpdateEventAsync(int eventId, string userId, UpdateEventRequest request)
    {
        var evt = await _context.Events.FindAsync(eventId);

        if (evt == null || evt.OrganizerId != userId)
            return null;

        // Regenerate slug if name changed
        if (evt.Name != request.Name)
        {
            var baseSlug = SlugHelper.GenerateSlug(request.Name);
            var slug = baseSlug;
            int counter = 1;
            while (await _context.Events.AnyAsync(e => e.Slug == slug && e.Id != eventId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }
            evt.Slug = slug;
        }
        
        evt.Name = request.Name;
        evt.ImageUrl = request.ImageUrl;
        evt.Description = request.Description;
        evt.StartDate = request.StartDate;
        evt.EndDate = request.EndDate;
        evt.Location = request.Location;
        evt.MapLink = request.MapLink;
        evt.MaxParticipants = request.MaxParticipants;
        evt.Price = request.Price;
        evt.EventType = request.EventType;
        evt.Visibility = request.Visibility;
        evt.UpdatedAt = DateTime.UtcNow;

        var existingGroups = await _context.EventGroups
            .Where(eg => eg.EventId == eventId)
            .ToListAsync();
        _context.EventGroups.RemoveRange(existingGroups);

        if (request.GroupIds != null && request.GroupIds.Length > 0)
        {
            foreach (var groupId in request.GroupIds)
            {
                _context.EventGroups.Add(new EventGroup
                {
                    EventId = eventId,
                    GroupId = groupId
                });
            }
        }

        await _context.SaveChangesAsync();

        return await GetEventAsync(eventId, userId);
    }

    public async Task<bool> CancelEventAsync(int eventId, string userId)
    {
        var evt = await _context.Events.FindAsync(eventId);

        if (evt == null || evt.OrganizerId != userId)
            return false;

        evt.IsCancelled = true;
        evt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EventResponse?> GetEventAsync(int eventId, string? userId)
    {
        var evt = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (evt == null)
            return null;

        if (userId != null && !await CanUserViewEvent(evt, userId))
            return null;

        var attendeeCount = evt.Attendees.Count;
        var isUserAttending = userId != null && evt.Attendees.Any(a => a.UserId == userId);
        var isUserOrganizer = userId == evt.OrganizerId;

        return new EventResponse(
            evt.Id,
            evt.Name,
            evt.Slug,
            evt.ImageUrl,
            evt.Description,
            evt.StartDate,
            evt.EndDate,
            evt.Location,
            evt.MapLink,
            evt.MaxParticipants,
            evt.Price,
            evt.EventType,
            evt.Visibility,
            evt.IsCancelled,
            evt.OrganizerId,
            evt.Organizer.DisplayName,
            attendeeCount,
            isUserAttending,
            isUserOrganizer,
            evt.CreatedAt
        );
    }

    public async Task<EventResponse?> GetEventBySlugAsync(string slug, string? userId)
    {
        var evt = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Slug == slug);

        if (evt == null)
            return null;

        // Check visibility
        if (userId != null && !await CanUserViewEvent(evt, userId))
            return null;

        var attendeeCount = evt.Attendees.Count;
        var isUserAttending = userId != null && evt.Attendees.Any(a => a.UserId == userId);
        var isUserOrganizer = userId == evt.OrganizerId;

        return new EventResponse(
            evt.Id,
            evt.Name,
            evt.Slug,
            evt.ImageUrl,
            evt.Description,
            evt.StartDate,
            evt.EndDate,
            evt.Location,
            evt.MapLink,
            evt.MaxParticipants,
            evt.Price,
            evt.EventType,
            evt.Visibility,
            evt.IsCancelled,
            evt.OrganizerId,
            evt.Organizer.DisplayName,
            attendeeCount,
            isUserAttending,
            isUserOrganizer,
            evt.CreatedAt
        );
    }

    public async Task<PaginatedResult<EventListResponse>> SearchEventsAsync(string? userId, DateTime? fromDate, 
        DateTime? toDate, string? location, string? searchText, int page = 1, int pageSize = 30)
    {
        var query = _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .Where(e => !e.IsCancelled);

        if (fromDate.HasValue)
            query = query.Where(e => e.StartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.StartDate <= toDate.Value);

        if (!string.IsNullOrEmpty(location))
            query = query.Where(e => e.Location.Contains(location));

        // Text search across name, description, and location
        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(e => 
                e.Name.Contains(searchText) || 
                (e.Description != null && e.Description.Contains(searchText)) ||
                e.Location.Contains(searchText));
        }

        var totalCount = await query.CountAsync();
        
        var events = await query
            .OrderBy(e => e.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (userId != null)
        {
            events = await Task.WhenAll(events.Select(async e => 
                await CanUserViewEvent(e, userId) ? e : null!))
                .ContinueWith(t => t.Result.Where(e => e != null).ToList());
        }

        var items = events.Select(e => new EventListResponse(
            e.Id,
            e.Name,
            e.Slug,
            e.ImageUrl,
            e.StartDate,
            e.EndDate,
            e.Location,
            e.Attendees.Count,
            e.MaxParticipants,
            e.Organizer.DisplayName,
            e.Price
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResult<EventListResponse>(
            items,
            totalCount,
            page,
            pageSize,
            totalPages
        );
    }

    public async Task<List<EventListResponse>> GetUserEventsAsync(string userId)
    {
        var events = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Attendees)
            .Where(e => e.OrganizerId == userId && !e.IsCancelled)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync();

        return events.Select(e => new EventListResponse(
            e.Id,
            e.Name,
            e.Slug,
            e.ImageUrl,
            e.StartDate,
            e.EndDate,
            e.Location,
            e.Attendees.Count,
            e.MaxParticipants,
            e.Organizer.DisplayName,
            e.Price
        )).ToList();
    }

    public async Task<bool> JoinEventAsync(int eventId, string userId)
    {
        var evt = await _context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (evt == null || evt.IsCancelled)
            return false;

        if (evt.Attendees.Any(a => a.UserId == userId))
            return false;

        if (!await CanUserViewEvent(evt, userId))
            return false;

        // Check if user is blacklisted from this event
        var isBlacklisted = await _context.EventBlacklists
            .AnyAsync(eb => eb.EventId == eventId && eb.BlacklistedUserId == userId);

        if (isBlacklisted)
            return false;

        if (evt.MaxParticipants.HasValue && evt.Attendees.Count >= evt.MaxParticipants.Value)
            return false;

        _context.EventAttendees.Add(new EventAttendee
        {
            EventId = eventId,
            UserId = userId
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LeaveEventAsync(int eventId, string userId)
    {
        var evt = await _context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (evt == null)
            return false;

        if (evt.OrganizerId == userId)
            return false;

        var attendee = evt.Attendees.FirstOrDefault(a => a.UserId == userId);

        if (attendee == null)
            return false;

        _context.EventAttendees.Remove(attendee);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> CanUserViewEvent(Event evt, string userId)
    {
        if (evt.Visibility == EventVisibility.Public)
            return true;

        if (evt.OrganizerId == userId)
            return true;

        if (evt.Visibility == EventVisibility.GroupOnly)
        {
            var eventGroupIds = await _context.EventGroups
                .Where(eg => eg.EventId == evt.Id)
                .Select(eg => eg.GroupId)
                .ToListAsync();

            return await _context.GroupMembers
                .AnyAsync(gm => eventGroupIds.Contains(gm.GroupId) && gm.UserId == userId);
        }

        return false;
    }
}

