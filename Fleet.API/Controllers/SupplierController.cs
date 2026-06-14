using Fleet.API.Entities.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.API.Controllers;

[ApiController, Route("api/suppliers")]
public class SupplierController
    : EntityControllerBase<Supplier, int, SupplierSearchObject, SupplierDto, SupplierInputDto>;
