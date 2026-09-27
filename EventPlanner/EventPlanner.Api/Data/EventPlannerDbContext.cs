using EventPlanner.Api.Entities.Categories;
using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Registrations;
using EventPlanner.Api.Entities.Speakers;
using EventPlanner.Api.Entities.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

namespace EventPlanner.Api.Data;

public class EventPlannerDbContext(DbContextOptions<EventPlannerDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Speaker> Speakers => Set<Speaker>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionSpeaker> SessionSpeakers => Set<SessionSpeaker>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<RegistrationSession> RegistrationSessions => Set<RegistrationSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity tables
        modelBuilder.SetDecimalPrecisionConvention();

        modelBuilder.Entity<Event>(e =>
        {
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Sessions).WithOne(x => x.Event).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.StartDate);
            e.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.HasMany(x => x.Speakers).WithOne(x => x.Session).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SessionSpeaker>(e =>
        {
            // lookup side: a speaker still scheduled in a session cannot be deleted (409)
            e.HasOne(x => x.Speaker).WithMany().HasForeignKey(x => x.SpeakerId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.SessionId, x.SpeakerId }).IsUnique();
        });

        modelBuilder.Entity<Registration>(e =>
        {
            // an event with registrations cannot be deleted (409) — cancel it instead
            e.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Sessions).WithOne(x => x.Registration).HasForeignKey(x => x.RegistrationId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.EventId, x.UserId }).IsUnique();
        });

        modelBuilder.Entity<RegistrationSession>(e =>
        {
            // removing a session from the agenda removes it from everyone's selection
            e.HasOne(x => x.Session).WithMany().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.RegistrationId, x.SessionId }).IsUnique();
        });
    }
}
