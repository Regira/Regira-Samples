using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.InterventionTypes;
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
    public DbSet<InterventionLine> InterventionLines => Set<InterventionLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.HasIndex(x => x.LicensePlate).IsUnique();
            e.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<InterventionType>(e => e.HasIndex(x => x.Code).IsUnique());

        modelBuilder.Entity<SupplierInterventionType>(e =>
        {
            e.HasIndex(x => new { x.SupplierId, x.InterventionTypeId }).IsUnique();
            // owner side: capabilities belong to the supplier
            e.HasOne(x => x.Supplier).WithMany(s => s.InterventionTypes).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Cascade);
            // lookup side: a type still assigned to a supplier cannot be deleted (409)
            e.HasOne(x => x.InterventionType).WithMany().HasForeignKey(x => x.InterventionTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Intervention>(e =>
        {
            e.HasIndex(x => x.ScheduledDate);
            e.HasIndex(x => x.Status);
            // history must survive: a vehicle/supplier with interventions cannot be deleted (409)
            e.HasOne(x => x.Vehicle).WithMany(v => v.Interventions).HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            // deleting an invoice un-bills its interventions
            e.HasOne(x => x.Invoice).WithMany(i => i.Interventions).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Lines).WithOne(l => l.Intervention).HasForeignKey(l => l.InterventionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InterventionLine>(e =>
            e.HasOne(x => x.InterventionType).WithMany().HasForeignKey(x => x.InterventionTypeId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasIndex(x => new { x.SupplierId, x.InvoiceNumber }).IsUnique();
            e.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
