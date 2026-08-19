using Fleet.Api.Entities.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.Api.Controllers;

[ApiController, Route("suppliers")]
public class SupplierController : EntityControllerBase<Supplier, int, SupplierSearchObject, SupplierDto, SupplierInputDto>;
