using Fleet.API.Entities.Invoices;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.API.Controllers;

[ApiController, Route("invoices")]
public class InvoiceController
    : EntityControllerBase<Invoice, InvoiceDto, InvoiceInputDto>;
