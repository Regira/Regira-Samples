using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QCredits.Api.Entities.CreditAllocations;
using QCredits.Api.Entities.CreditRequests;
using QCredits.Api.Entities.CreditYears;
using QCredits.Api.Entities.Departments;
using QCredits.Api.Entities.Employees;
using QCredits.Api.Entities.GroupTrainings;
using Regira.DAL.EFcore.Extensions;

namespace QCredits.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<CreditYear> CreditYears => Set<CreditYear>();
    public DbSet<CreditAllocation> CreditAllocations => Set<CreditAllocation>();
    public DbSet<CreditRequest> CreditRequests => Set<CreditRequest>();
    public DbSet<CreditRequestItem> CreditRequestItems => Set<CreditRequestItem>();
    public DbSet<GroupTraining> GroupTrainings => Set<GroupTraining>();
    public DbSet<GroupTrainingParticipant> GroupTrainingParticipants => Set<GroupTrainingParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention();

        modelBuilder.Entity<Department>(b =>
        {
            b.HasIndex(x => x.Title).IsUnique();
        });

        modelBuilder.Entity<Employee>(b =>
        {
            b.HasIndex(x => x.Email).IsUnique();
            b.HasOne(x => x.Department).WithMany(x => x.Employees)
                .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CreditYear>(b =>
        {
            b.HasIndex(x => x.Year).IsUnique();
        });

        modelBuilder.Entity<CreditAllocation>(b =>
        {
            b.HasIndex(x => new { x.EmployeeId, x.Year }).IsUnique();
            b.HasOne(x => x.Employee).WithMany()
                .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CreditRequest>(b =>
        {
            b.HasIndex(x => new { x.EmployeeId, x.Year, x.Status });
            b.HasOne(x => x.Employee).WithMany()
                .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Items).WithOne(x => x.CreditRequest)
                .HasForeignKey(x => x.CreditRequestId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GroupTraining>(b =>
        {
            b.HasMany(x => x.Participants).WithOne(x => x.GroupTraining)
                .HasForeignKey(x => x.GroupTrainingId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GroupTrainingParticipant>(b =>
        {
            b.HasIndex(x => new { x.GroupTrainingId, x.EmployeeId }).IsUnique();
            b.HasOne(x => x.Employee).WithMany()
                .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
