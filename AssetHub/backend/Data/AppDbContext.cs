using AssetHub.Api.Entities.AssetAssignments;
using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Employees;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

namespace AssetHub.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AssetStatus> AssetStatuses => Set<AssetStatus>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetAttachment> AssetAttachments => Set<AssetAttachment>();
    public DbSet<AssetWarranty> AssetWarranties => Set<AssetWarranty>();
    public DbSet<AssetMaintenanceRecord> AssetMaintenanceRecords => Set<AssetMaintenanceRecord>();

    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<Employee>(e => e.HasIndex(x => x.Email).IsUnique());

        modelBuilder.Entity<Asset>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();

            // Reference data (lookup side): Restrict so an in-use Category/AssetStatus can't be silently
            // orphaned by a delete. SQLite needs "Foreign Keys=True" on the connection string for this to fire.
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Status).WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);

            // Owned collections: part of the aggregate, cascade with the parent.
            e.HasMany(x => x.Attachments).WithOne(a => a.Asset).HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Warranties).WithOne(a => a.Asset).HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.MaintenanceRecords).WithOne(a => a.Asset).HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.Cascade);

            // Asset is IArchivable -- nothing to add for the query filter itself, UseDefaults() wires it.
        });

        modelBuilder.Entity<AssetAssignment>(e =>
        {
            e.HasOne(x => x.Asset).WithMany(a => a.Assignments).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
