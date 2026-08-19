using AssetHub.Api.Entities.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;

namespace AssetHub.Api.Controllers;

[ApiController, Route("suppliers")]
public class SupplierController : EntityControllerBase<Supplier, int, SupplierSearchObject, SupplierDto, SupplierInputDto>;
