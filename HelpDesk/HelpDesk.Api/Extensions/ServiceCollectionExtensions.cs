using HelpDesk.Api.Data;
using HelpDesk.Api.Entities;
using HelpDesk.Api.Entities.Persons;
using HelpDesk.Api.Entities.Tickets;
using HelpDesk.Api.Infrastructure.Security;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.Mapping.Mapster;
using Regira.Entities.Web.Attachments.DependencyInjection;
using Regira.IO.Storage.FileSystem;

namespace HelpDesk.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /*
     * Entity budget (free tier = 5 simple + 2 complex, two independent buckets)
     * | Entity           | Classification                              | Tally        |
     * |------------------|---------------------------------------------|--------------|
     * | SupportTeam      | simple                                      | 1/5 simple   |
     * | Category         | simple                                      | 2/5 simple   |
     * | Priority         | simple                                      | 3/5 simple   |
     * | Status           | simple                                      | 4/5 simple   |
     * | Person           | complex (customers + employees in one type) | 1/2 complex  |
     * | Ticket           | complex (typed sort + includes)             | 2/2 complex  |
     * | TicketCategory   | owned m2m join via e.Related()              | -            |
     * | TicketComment    | owned child, written by the comment action  | -            |
     * | Attachment       | shared base via WithAttachments             | -            |
     * | TicketAttachment | simple (HasAttachments join)                | 5/5 simple   |
     * => 5 simple / 2 complex: fits the free tier exactly.
     */
    public static IServiceCollection AddEntityServices(this IServiceCollection services, IConfiguration configuration)
    {
        var uploadsFolder = Path.GetFullPath(configuration["Storage:AttachmentsFolder"] ?? "App_Data/uploads");
        Directory.CreateDirectory(uploadsFolder);

        services
            .AddHttpContextAccessor()
            .AddScoped<CurrentUser>()
            .AddSingleton<TicketCodeGenerator>();

        return services
            .UseEntities<HelpDeskDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
                options.UseAttachmentUris();
                options.DefaultPageSize = 25;
                options.MaxPageSize = 500;
                // row security (see TicketAccess.cs)
                options.AddGlobalFilterQueryBuilder<TicketAccessFilter>();
                options.AddGlobalFilterQueryBuilder<TicketAttachmentAccessFilter>();
                options.AddGlobalFilterQueryBuilder<PersonAccessFilter>();
            })
            .WithAttachments(_ => new BinaryFileService(new FileSystemOptions { RootFolder = uploadsFolder }))
            .AddSupportTeams()
            .AddCategories()
            .AddPriorities()
            .AddStatuses()
            .AddPersons()
            .AddTickets();
    }
}
