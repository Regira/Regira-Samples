using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;

namespace Fleet.Api.Data;

public class FleetDbContext(DbContextOptions<FleetDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierInterventionType> SupplierInterventionTypes => Set<SupplierInterventionType>();
    public DbSet<InterventionType> InterventionTypes => Set<InterventionType>();
    public DbSet<Intervention> Interventions => Set<Intervention>();
    public DbSet<InterventionInterventionType> InterventionInterventionTypes => Set<InterventionInterventionType>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    // UTC dates + the IArchivable filter (unused here) are auto-wired by UseEntities(e => e.UseDefaults())

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.HasIndex(x => x.LicensePlate).IsUnique();
        });

        modelBuilder.Entity<SupplierInterventionType>(e =>
        {
            e.HasIndex(x => new { x.SupplierId, x.InterventionTypeId }).IsUnique();
            e.HasOne(x => x.Supplier).WithMany(s => s.SupportedInterventionTypes)
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
            // lookup side: don't silently strip a supplier's capability when a type is removed
            e.HasOne(x => x.InterventionType).WithMany()
                .HasForeignKey(x => x.InterventionTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InterventionInterventionType>(e =>
        {
            e.HasIndex(x => new { x.InterventionId, x.InterventionTypeId }).IsUnique();
            e.HasOne(x => x.Intervention).WithMany(i => i.InterventionTypes)
                .HasForeignKey(x => x.InterventionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.InterventionType).WithMany()
                .HasForeignKey(x => x.InterventionTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Intervention>(e =>
        {
            // Independent entities with a back-ref collection (no e.Related() on either side) --
            // protect referential integrity explicitly since a vehicle/supplier with interventions
            // on file should not vanish silently.
            e.HasOne(x => x.Vehicle).WithMany()
                .HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Supplier).WithMany()
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            // Optional parent FK -- removing the invoice just detaches its interventions.
            e.HasOne(x => x.Invoice).WithMany(i => i.Interventions)
                .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
        });
    }
}
