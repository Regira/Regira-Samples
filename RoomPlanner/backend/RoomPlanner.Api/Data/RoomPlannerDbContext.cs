using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using RoomPlanner.Api.Entities.Buildings;
using RoomPlanner.Api.Entities.Employees;
using RoomPlanner.Api.Entities.Floors;
using RoomPlanner.Api.Entities.MeetingRooms;
using RoomPlanner.Api.Entities.Reservations;

namespace RoomPlanner.Api.Data;

public class RoomPlannerDbContext(DbContextOptions<RoomPlannerDbContext> options) : DbContext(options)
{
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<MeetingRoom> MeetingRooms => Set<MeetingRoom>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationRoom> ReservationRooms => Set<ReservationRoom>();
    public DbSet<ReservationAttendee> ReservationAttendees => Set<ReservationAttendee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.SetDecimalPrecisionConvention();

        modelBuilder.Entity<Floor>(e =>
        {
            e.HasOne(f => f.Building).WithMany().HasForeignKey(f => f.BuildingId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MeetingRoom>(e =>
        {
            e.HasOne(r => r.Floor).WithMany().HasForeignKey(r => r.FloorId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Reservation>(e =>
        {
            e.HasOne(r => r.Organizer).WithMany().HasForeignKey(r => r.OrganizerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReservationRoom>(e =>
        {
            e.HasIndex(rr => new { rr.ReservationId, rr.RoomId }).IsUnique();
            e.HasOne(rr => rr.Reservation).WithMany(r => r.Rooms).HasForeignKey(rr => rr.ReservationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(rr => rr.Room).WithMany().HasForeignKey(rr => rr.RoomId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReservationAttendee>(e =>
        {
            e.HasOne(a => a.Reservation).WithMany(r => r.Attendees).HasForeignKey(a => a.ReservationId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
