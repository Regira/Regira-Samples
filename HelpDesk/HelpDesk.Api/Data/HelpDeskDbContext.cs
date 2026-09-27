using HelpDesk.Api.Entities.Categories;
using HelpDesk.Api.Entities.Persons;
using HelpDesk.Api.Entities.Priorities;
using HelpDesk.Api.Entities.Statuses;
using HelpDesk.Api.Entities.SupportTeams;
using HelpDesk.Api.Entities.Tickets;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Extensions;
using Regira.Entities.Attachments.Models;

namespace HelpDesk.Api.Data;

public class AppUser : IdentityUser;

public class HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<SupportTeam> SupportTeams => Set<SupportTeam>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Priority> Priorities => Set<Priority>();
    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<Person> Persons => Set<Person>();
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
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.Email);
            e.HasOne(x => x.SupportTeam).WithMany(t => t.Members).HasForeignKey(x => x.SupportTeamId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Category>(e =>
            e.HasOne(x => x.SupportTeam).WithMany().HasForeignKey(x => x.SupportTeamId).OnDelete(DeleteBehavior.SetNull));

        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasIndex(x => x.Code).IsUnique();
            // reference data behind required FKs: a real DELETE that is refused (409) while in use
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Priority).WithMany().HasForeignKey(x => x.PriorityId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Status).WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedEmployee).WithMany().HasForeignKey(x => x.AssignedEmployeeId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.SupportTeam).WithMany().HasForeignKey(x => x.SupportTeamId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Comments).WithOne(c => c.Ticket).HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Attachments).WithOne().HasForeignKey(a => a.ObjectId).HasPrincipalKey(x => x.Id);
        });

        modelBuilder.Entity<TicketCategory>(e =>
        {
            e.HasIndex(x => new { x.TicketId, x.CategoryId }).IsUnique();
            e.HasOne(x => x.Ticket).WithMany(t => t.Categories).HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketComment>(e =>
            e.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict));

        modelBuilder.Entity<TicketAttachment>()
            .HasOne(x => x.Attachment).WithMany().HasForeignKey(x => x.AttachmentId);
    }
}
