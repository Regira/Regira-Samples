using EventPlanner.Api.Entities.EventCategories;
using EventPlanner.Api.Entities.Employees;
using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Registrations;
using EventPlanner.Api.Entities.Sessions;
using EventPlanner.Api.Entities.Speakers;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

namespace EventPlanner.Api.Data;

public class EventPlannerDbContext(DbContextOptions<EventPlannerDbContext> options) : DbContext(options)
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Speaker> Speakers => Set<Speaker>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionSpeaker> SessionSpeakers => Set<SessionSpeaker>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<RegistrationSession> RegistrationSessions => Set<RegistrationSession>();

    // UTC dates + the archived query filter are auto-wired by UseEntities(e => e.UseDefaults()) — see Program.cs

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<Event>(e =>
        {
            e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.EventCategory).WithMany().HasForeignKey(x => x.EventCategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Session>(e =>
        {
            e.HasOne(x => x.Event).WithMany(x => x.Sessions).HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SessionSpeaker>(e =>
        {
            e.HasIndex(x => new { x.SessionId, x.SpeakerId }).IsUnique();
            e.HasOne(x => x.Session).WithMany(x => x.SessionSpeakers).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Speaker).WithMany().HasForeignKey(x => x.SpeakerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Registration>(e =>
        {
            e.HasIndex(x => new { x.EmployeeId, x.EventId }).IsUnique();
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Event).WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RegistrationSession>(e =>
        {
            e.HasIndex(x => new { x.RegistrationId, x.SessionId }).IsUnique();
            e.HasOne(x => x.Registration).WithMany(x => x.SelectedSessions).HasForeignKey(x => x.RegistrationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Session).WithMany().HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
