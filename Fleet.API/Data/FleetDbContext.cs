using Fleet.API.Entities.Interventions;
using Fleet.API.Entities.InterventionTypes;
using Fleet.API.Entities.Invoices;
using Fleet.API.Entities.Suppliers;
using Fleet.API.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

namespace Fleet.API.Data;

public class FleetDbContext(DbContextOptions<FleetDbContext> options) : DbContext(options)
{
    public DbSet<InterventionType> InterventionTypes { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<VehicleInterventionType> VehicleInterventionTypes { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<SupplierInterventionType> SupplierInterventionTypes { get; set; } = null!;
    public DbSet<Intervention> Interventions { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<InterventionType>(e =>
            e.HasIndex(x => x.Code).IsUnique());

        // Vehicle <-> InterventionType (allowed interventions)
        modelBuilder.Entity<Vehicle>(e =>
            e.HasIndex(x => x.LicensePlate).IsUnique());
        modelBuilder.Entity<VehicleInterventionType>(e =>
        {
            e.HasIndex(x => new { x.VehicleId, x.InterventionTypeId }).IsUnique();
            e.HasOne(x => x.Vehicle).WithMany(v => v.AllowedInterventionTypes)
                .HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.InterventionType).WithMany()
                .HasForeignKey(x => x.InterventionTypeId).OnDelete(DeleteBehavior.Cascade);
        });

        // Supplier <-> InterventionType (capabilities)
        modelBuilder.Entity<SupplierInterventionType>(e =>
        {
            e.HasIndex(x => new { x.SupplierId, x.InterventionTypeId }).IsUnique();
            e.HasOne(x => x.Supplier).WithMany(s => s.Capabilities)
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.InterventionType).WithMany()
                .HasForeignKey(x => x.InterventionTypeId).OnDelete(DeleteBehavior.Cascade);
        });

        // Invoice -> Supplier, Invoice -> Interventions
        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.HasOne(x => x.Supplier).WithMany()
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });

        // Intervention -> Vehicle / Supplier / InterventionType / Invoice
        modelBuilder.Entity<Intervention>(e =>
        {
            e.HasOne(x => x.Vehicle).WithMany()
                .HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Supplier).WithMany()
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.InterventionType).WithMany()
                .HasForeignKey(x => x.InterventionTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Invoice).WithMany(i => i.Interventions)
                .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
