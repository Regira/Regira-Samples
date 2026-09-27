using HelpDesk.Api.Entities.Categories;
using HelpDesk.Api.Entities.Persons;
using HelpDesk.Api.Entities.Priorities;
using HelpDesk.Api.Entities.Statuses;
using HelpDesk.Api.Entities.SupportTeams;
using HelpDesk.Api.Entities.Tickets;
using HelpDesk.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Attachments.Abstractions;
using Regira.Entities.Web.Controllers.Abstractions;

namespace HelpDesk.Api.Controllers;

// Generics mirror the .For<>() registrations exactly (register = N, controller = N + 2).
// Write access per controller: Infrastructure/Security/WriteAuthorizationFilter.

[ApiController, Route("support-teams")]
public class SupportTeamsController : EntityControllerBase<SupportTeam, SupportTeamDto, SupportTeamInputDto>;

[ApiController, Route("categories")]
public class CategoriesController : EntityControllerBase<Category, int, CategorySearchObject, CategoryDto, CategoryInputDto>;

[ApiController, Route("priorities")]
public class PrioritiesController : EntityControllerBase<Priority, PriorityDto, PriorityInputDto>;

[ApiController, Route("statuses")]
public class StatusesController : EntityControllerBase<Status, StatusDto, StatusInputDto>;

[ApiController, Route("persons")]
public class PersonsController : EntityControllerBase<Person, PersonSearchObject, PersonSortBy, EntityIncludes, PersonDto, PersonInputDto>;

[ApiController, Route("tickets")]
public class TicketsController : EntityControllerBase<Ticket, TicketSearchObject, TicketSortBy, TicketIncludes, TicketDto, TicketInputDto>;

// named after the attachment type so the attachment Uri resolves; class route = owner base path
[ApiController, Route("tickets"), TypeFilter(typeof(TicketOwnerScopeFilter))]
public class TicketAttachmentsController : EntityAttachmentControllerBase<TicketAttachment>;
