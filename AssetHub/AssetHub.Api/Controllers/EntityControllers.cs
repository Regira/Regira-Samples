using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Employees;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Attachments.Abstractions;
using Regira.Entities.Web.Controllers.Abstractions;

namespace AssetHub.Api.Controllers;

// simple: For<T, int, TSearchObject> (3) -> controller (5)
[ApiController, Route("categories")]
public class CategoriesController : EntityControllerBase<Category, int, CategorySearchObject, CategoryDto, CategoryInputDto>;

[ApiController, Route("asset-statuses")]
public class AssetStatusesController : EntityControllerBase<AssetStatus, int, AssetStatusSearchObject, AssetStatusDto, AssetStatusInputDto>;

[ApiController, Route("locations")]
public class LocationsController : EntityControllerBase<Location, int, LocationSearchObject, LocationDto, LocationInputDto>;

[ApiController, Route("suppliers")]
public class SuppliersController : EntityControllerBase<Supplier, int, SupplierSearchObject, SupplierDto, SupplierInputDto>;

// complex: For<T, TSearchObject, TSortBy, TIncludes> (4) -> controller (6)
[ApiController, Route("employees")]
public class EmployeesController : EntityControllerBase<Employee, EmployeeSearchObject, EmployeeSortBy, EmployeeIncludes, EmployeeDto, EmployeeInputDto>;

[ApiController, Route("assets")]
public class AssetsController : EntityControllerBase<Asset, AssetSearchObject, AssetSortBy, AssetIncludes, AssetDto, AssetInputDto>;

// attachment controller: named after the attachment type, routed on the owner base path
[ApiController, Route("assets")]
public class AssetAttachmentsController : EntityAttachmentControllerBase<AssetAttachment>;
