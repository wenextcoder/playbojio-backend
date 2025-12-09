using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlayBojio.API.Models;

namespace PlayBojio.API.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<EventAttendee> EventAttendees => Set<EventAttendee>();
    public DbSet<SessionAttendee> SessionAttendees => Set<SessionAttendee>();
    public DbSet<SessionWaitlist> SessionWaitlists => Set<SessionWaitlist>();
    public DbSet<Blacklist> Blacklists => Set<Blacklist>();
    public DbSet<EventGroup> EventGroups => Set<EventGroup>();
    public DbSet<SessionGroup> SessionGroups => Set<SessionGroup>();
    public DbSet<SessionInvite> SessionInvites => Set<SessionInvite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Event relationships
        builder.Entity<Event>()
            .HasOne(e => e.Organizer)
            .WithMany(u => u.CreatedEvents)
            .HasForeignKey(e => e.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Session relationships
        builder.Entity<Session>()
            .HasOne(s => s.Host)
            .WithMany(u => u.CreatedSessions)
            .HasForeignKey(s => s.HostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Session>()
            .HasOne(s => s.Event)
            .WithMany(e => e.Sessions)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        // Group relationships
        builder.Entity<Group>()
            .HasOne(g => g.Owner)
            .WithMany(u => u.OwnedGroups)
            .HasForeignKey(g => g.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Blacklist relationships
        builder.Entity<Blacklist>()
            .HasOne(b => b.User)
            .WithMany(u => u.BlacklistedUsers)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Blacklist>()
            .HasOne(b => b.BlacklistedUser)
            .WithMany(u => u.BlacklistedByUsers)
            .HasForeignKey(b => b.BlacklistedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.Entity<Session>()
            .HasIndex(s => s.StartTime);

        builder.Entity<Event>()
            .HasIndex(e => e.StartDate);

        builder.Entity<Blacklist>()
            .HasIndex(b => new { b.UserId, b.BlacklistedUserId })
            .IsUnique();
    }
}

