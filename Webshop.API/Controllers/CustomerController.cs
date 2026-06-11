using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using Webshop.API.Entities.Customers;

namespace Webshop.API.Controllers;

[ApiController, Route("customers")]
public class CustomerController : EntityControllerBase<Customer, Guid, SearchObject<Guid>, CustomerDto, CustomerInputDto>;
