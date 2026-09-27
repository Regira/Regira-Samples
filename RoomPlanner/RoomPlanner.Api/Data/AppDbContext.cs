using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using RoomPlanner.Api.Entities.Buildings;
using RoomPlanner.Api.Entities.Employees;
using RoomPlanner.Api.Entities.Equipments;
using RoomPlanner.Api.Entities.Floors;
using RoomPlanner.Api.Entities.Reservations;
using RoomPlanner.Api.Entities.Rooms;

namespace RoomPlanner.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomEquipment> RoomEquipment => Set<RoomEquipment>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationRoom> ReservationRooms => Set<ReservationRoom>();
    public DbSet<ReservationAttendee> ReservationAttendees => Set<ReservationAttendee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<Building>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasMany(x => x.Floors).WithOne(x => x.Building).HasForeignKey(x => x.BuildingId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Floor>(e =>
        {
            e.HasIndex(x => new { x.BuildingId, x.Level }).IsUnique();
            e.HasMany(x => x.Rooms).WithOne(x => x.Floor).HasForeignKey(x => x.FloorId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Room>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Capacity);
        });
        modelBuilder.Entity<RoomEquipment>(e =>
        {
            e.HasIndex(x => new { x.RoomId, x.EquipmentId }).IsUnique();
            // owner side: the join rows are part of the room
            e.HasOne(x => x.Room).WithMany(x => x.Equipment).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
            // lookup side: refuse deleting equipment that is still assigned (409)
            e.HasOne(x => x.Equipment).WithMany().HasForeignKey(x => x.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Equipment>(e => e.HasIndex(x => x.Title).IsUnique());
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => new { x.LastName, x.FirstName });
        });
        modelBuilder.Entity<Reservation>(e =>
        {
            e.HasIndex(x => new { x.Start, x.End });
            e.HasIndex(x => x.Status);
            e.HasOne(x => x.Organizer).WithMany().HasForeignKey(x => x.OrganizerId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Rooms).WithOne(x => x.Reservation).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Attendees).WithOne(x => x.Reservation).HasForeignKey(x => x.ReservationId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ReservationRoom>(e =>
        {
            e.HasIndex(x => new { x.ReservationId, x.RoomId }).IsUnique();
            e.HasOne(x => x.Room).WithMany(x => x.Bookings).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ReservationAttendee>(e =>
        {
            e.HasIndex(x => new { x.ReservationId, x.EmployeeId }).IsUnique();
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
