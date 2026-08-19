using Fleet.Api.Entities.Invoices;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.Api.Controllers;

[ApiController, Route("invoices")]
public class InvoiceController : EntityControllerBase<Invoice, InvoiceSearchObject, InvoiceSortBy, InvoiceIncludes, InvoiceDto, InvoiceInputDto>;
