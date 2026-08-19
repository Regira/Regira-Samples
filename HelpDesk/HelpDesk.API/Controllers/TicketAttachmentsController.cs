using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Attachments.Abstractions;
using HelpDesk.API.Entities.Tickets;

namespace HelpDesk.API.Controllers;

// Route is the OWNER's base path - the base controller appends {objectId}/attachments, {objectId}/files, ...
[ApiController, Route("tickets")]
public class TicketAttachmentsController : EntityAttachmentControllerBase<TicketAttachment>;
