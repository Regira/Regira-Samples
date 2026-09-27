using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;

namespace Fleet.Api.Controllers;

// register = N generic args, controller = N + 2 (TDto, TInputDto)

[ApiController, Route("vehicles")]
public class VehicleController
    : EntityControllerBase<Vehicle, VehicleSearchObject, VehicleSortBy, EntityIncludes, VehicleDto, VehicleInputDto>;

[ApiController, Route("suppliers")]
public class SupplierController
    : EntityControllerBase<Supplier, int, SupplierSearchObject, SupplierDto, SupplierInputDto>;

[ApiController, Route("intervention-types")]
public class InterventionTypeController
    : EntityControllerBase<InterventionType, int, InterventionTypeSearchObject, InterventionTypeDto, InterventionTypeInputDto>;

[ApiController, Route("interventions")]
public class InterventionController
    : EntityControllerBase<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes, InterventionDto, InterventionInputDto>;

[ApiController, Route("invoices")]
public class InvoiceController
    : EntityControllerBase<Invoice, int, InvoiceSearchObject, InvoiceDto, InvoiceInputDto>;
