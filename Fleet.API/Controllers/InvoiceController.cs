using Fleet.API.Entities.Invoices;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.API.Controllers;

[ApiController, Route("api/invoices")]
public class InvoiceController
    : EntityControllerBase<Invoice, InvoiceSearchObject, InvoiceSortBy, InvoiceIncludes, InvoiceDto, InvoiceInputDto>;
