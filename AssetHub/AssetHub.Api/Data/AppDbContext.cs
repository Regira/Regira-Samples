using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Employees;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using Regira.Entities.Attachments.Models;

namespace AssetHub.Api.Data;

public class AppUser : IdentityUser
{
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AssetStatus> AssetStatuses => Set<AssetStatus>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
    public DbSet<Warranty> Warranties => Set<Warranty>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AssetAttachment> AssetAttachments => Set<AssetAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<Category>(e => e.HasIndex(x => x.Title).IsUnique());
        modelBuilder.Entity<AssetStatus>(e => e.HasIndex(x => x.Title).IsUnique());
        modelBuilder.Entity<Location>(e => e.HasIndex(x => x.Title).IsUnique());

        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Asset>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            e.HasIndex(x => x.SerialNumber);
            // reference data: Restrict -> deleting a category/status/location/supplier in use answers 409
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Status).WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Location).WithMany().HasForeignKey(x => x.LocationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CurrentEmployee).WithMany().HasForeignKey(x => x.CurrentEmployeeId).OnDelete(DeleteBehavior.Restrict);
            // owned collections: go with the asset
            e.HasMany(x => x.Assignments).WithOne(x => x.Asset).HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Warranties).WithOne().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.MaintenanceRecords).WithOne().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Attachments).WithOne().HasForeignKey(x => x.ObjectId).HasPrincipalKey(x => x.Id);
        });

        modelBuilder.Entity<AssetAssignment>(e =>
        {
            // an employee with history cannot be deleted (409) - deactivate instead
            e.HasOne(x => x.Employee).WithMany(x => x.Assignments).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.AssetId, x.ReturnedOn });
        });

        modelBuilder.Entity<AssetAttachment>()
            .HasOne(x => x.Attachment).WithMany().HasForeignKey(x => x.AttachmentId);
    }
}
