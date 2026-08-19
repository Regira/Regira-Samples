using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using Regira.Entities.Attachments.Models;
using HelpDesk.API.Entities.Categories;
using HelpDesk.API.Entities.People;
using HelpDesk.API.Entities.Priorities;
using HelpDesk.API.Entities.Statuses;
using HelpDesk.API.Entities.SupportTeams;
using HelpDesk.API.Entities.Tickets;

namespace HelpDesk.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Priority> Priorities => Set<Priority>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<SupportTeam> SupportTeams => Set<SupportTeam>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetDecimalPrecisionConvention();

        modelBuilder.Entity<Person>(e =>
        {
            e.HasIndex(p => p.Email).IsUnique();
            e.HasOne(p => p.SupportTeam).WithMany(t => t.Members)
                .HasForeignKey(p => p.SupportTeamId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasOne(t => t.Customer).WithMany()
                .HasForeignKey(t => t.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.AssignedEmployee).WithMany()
                .HasForeignKey(t => t.AssignedEmployeeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Priority).WithMany()
                .HasForeignKey(t => t.PriorityId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Status).WithMany()
                .HasForeignKey(t => t.StatusId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.SupportTeam).WithMany()
                .HasForeignKey(t => t.SupportTeamId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketCategory>(e =>
        {
            e.HasIndex(tc => new { tc.TicketId, tc.CategoryId }).IsUnique();
            e.HasOne(tc => tc.Ticket).WithMany(t => t.Categories)
                .HasForeignKey(tc => tc.TicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(tc => tc.Category).WithMany()
                .HasForeignKey(tc => tc.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketComment>(e =>
        {
            e.HasOne(c => c.Ticket).WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Author).WithMany()
                .HasForeignKey(c => c.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });

        // Attachments wiring (entities.examples.md - Attachments)
        modelBuilder.Entity<TicketAttachment>()
            .HasOne(x => x.Attachment).WithMany().HasForeignKey(x => x.AttachmentId);
        modelBuilder.Entity<Ticket>(entity =>
            entity.HasMany(e => e.Attachments).WithOne().HasForeignKey(e => e.ObjectId).HasPrincipalKey(e => e.Id));
    }
}
