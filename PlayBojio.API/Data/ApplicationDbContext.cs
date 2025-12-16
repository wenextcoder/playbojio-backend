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
    public DbSet<GroupBlacklist> GroupBlacklists => Set<GroupBlacklist>();
    public DbSet<EventBlacklist> EventBlacklists => Set<EventBlacklist>();
    public DbSet<Friend> Friends => Set<Friend>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<GroupJoinRequest> GroupJoinRequests => Set<GroupJoinRequest>();
    public DbSet<GroupInvitation> GroupInvitations => Set<GroupInvitation>();

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

        // Group Blacklist relationships
        builder.Entity<GroupBlacklist>()
            .HasOne(gb => gb.Group)
            .WithMany(g => g.BlacklistedUsers)
            .HasForeignKey(gb => gb.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupBlacklist>()
            .HasOne(gb => gb.BlacklistedUser)
            .WithMany()
            .HasForeignKey(gb => gb.BlacklistedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupBlacklist>()
            .HasOne(gb => gb.BlacklistedByUser)
            .WithMany()
            .HasForeignKey(gb => gb.BlacklistedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Event Blacklist relationships
        builder.Entity<EventBlacklist>()
            .HasOne(eb => eb.Event)
            .WithMany(e => e.BlacklistedUsers)
            .HasForeignKey(eb => eb.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EventBlacklist>()
            .HasOne(eb => eb.BlacklistedUser)
            .WithMany()
            .HasForeignKey(eb => eb.BlacklistedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EventBlacklist>()
            .HasOne(eb => eb.BlacklistedByUser)
            .WithMany()
            .HasForeignKey(eb => eb.BlacklistedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        builder.Entity<Session>()
            .HasIndex(s => s.StartTime);

        builder.Entity<Event>()
            .HasIndex(e => e.StartDate);

        builder.Entity<Blacklist>()
            .HasIndex(b => new { b.UserId, b.BlacklistedUserId })
            .IsUnique();

        builder.Entity<GroupBlacklist>()
            .HasIndex(gb => new { gb.GroupId, gb.BlacklistedUserId })
            .IsUnique();

        builder.Entity<EventBlacklist>()
            .HasIndex(eb => new { eb.EventId, eb.BlacklistedUserId })
            .IsUnique();

        // Friend relationships
        builder.Entity<Friend>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Friend>()
            .HasOne(f => f.FriendUser)
            .WithMany()
            .HasForeignKey(f => f.FriendId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Friend>()
            .HasIndex(f => new { f.UserId, f.FriendId })
            .IsUnique();

        // FriendRequest relationships
        builder.Entity<FriendRequest>()
            .HasOne(fr => fr.Sender)
            .WithMany()
            .HasForeignKey(fr => fr.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FriendRequest>()
            .HasOne(fr => fr.Receiver)
            .WithMany()
            .HasForeignKey(fr => fr.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FriendRequest>()
            .HasIndex(fr => new { fr.SenderId, fr.ReceiverId })
            .IsUnique();

        // GroupJoinRequest relationships
        builder.Entity<GroupJoinRequest>()
            .HasOne(gjr => gjr.Group)
            .WithMany()
            .HasForeignKey(gjr => gjr.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupJoinRequest>()
            .HasOne(gjr => gjr.User)
            .WithMany()
            .HasForeignKey(gjr => gjr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupJoinRequest>()
            .HasOne(gjr => gjr.RespondedByUser)
            .WithMany()
            .HasForeignKey(gjr => gjr.RespondedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupJoinRequest>()
            .HasIndex(gjr => new { gjr.GroupId, gjr.UserId, gjr.Status });

        // GroupInvitation relationships
        builder.Entity<GroupInvitation>()
            .HasOne(gi => gi.Group)
            .WithMany()
            .HasForeignKey(gi => gi.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupInvitation>()
            .HasOne(gi => gi.InvitedUser)
            .WithMany()
            .HasForeignKey(gi => gi.InvitedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupInvitation>()
            .HasOne(gi => gi.InvitedByUser)
            .WithMany()
            .HasForeignKey(gi => gi.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupInvitation>()
            .HasIndex(gi => new { gi.GroupId, gi.InvitedUserId, gi.Status });
    }
}

