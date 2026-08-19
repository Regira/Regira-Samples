using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;
using Regira.Entities.Web.Attachments.DependencyInjection;
using Regira.IO.Storage.FileSystem;
using HelpDesk.API.Data;
using HelpDesk.API.Entities.Categories;
using HelpDesk.API.Entities.People;
using HelpDesk.API.Entities.Priorities;
using HelpDesk.API.Entities.Statuses;
using HelpDesk.API.Entities.SupportTeams;
using HelpDesk.API.Entities.Tickets;

namespace HelpDesk.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services, IConfiguration configuration)
        => services
            .UseEntities<AppDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
                options.UseAttachmentUris();
            })
            // Free tier budget: 5 simple (Category, Priority, Status, SupportTeam, TicketAttachment)
            // + 2 complex (Person, Ticket) = 7/7 - see entities.instructions Step 0.
            .WithAttachments(_ => new BinaryFileService(
                new FileSystemOptions { RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads") }))
            .AddCategories()
            .AddPriorities()
            .AddStatuses()
            .AddSupportTeams()
            .AddPeople()
            .AddTickets();
}
