using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using QCredits.Api.Entities.CreditPolicies;
using QCredits.Api.Entities.EmployeeCarryOvers;
using QCredits.Api.Entities.Employees;
using QCredits.Api.Entities.GroupTrainings;
using QCredits.Api.Entities.QCreditRequests;

namespace QCredits.Api.Data;

public class QCreditsDbContext(DbContextOptions<QCreditsDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<CreditPolicy> CreditPolicies => Set<CreditPolicy>();
    public DbSet<EmployeeCarryOver> EmployeeCarryOvers => Set<EmployeeCarryOver>();
    public DbSet<GroupTraining> GroupTrainings => Set<GroupTraining>();
    public DbSet<QCreditRequest> QCreditRequests => Set<QCreditRequest>();
    public DbSet<QCreditRequestItem> QCreditRequestItems => Set<QCreditRequestItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention(18, 2);

        modelBuilder.Entity<CreditPolicy>(e =>
        {
            e.HasIndex(x => x.Year).IsUnique();
        });

        modelBuilder.Entity<EmployeeCarryOver>(e =>
        {
            e.HasIndex(x => new { x.EmployeeId, x.Year }).IsUnique();
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QCreditRequest>(e =>
        {
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Approver).WithMany().HasForeignKey(x => x.ApproverId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Items).WithOne(x => x.Request).HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
