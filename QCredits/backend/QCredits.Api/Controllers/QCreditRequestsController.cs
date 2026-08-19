using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Models;
using Regira.Entities.Web.Controllers.Abstractions;
using QCredits.Api.Entities.QCreditRequests;

namespace QCredits.Api.Controllers;

[ApiController, Route("qcredit-requests")]
public class QCreditRequestsController : EntityControllerBase<QCreditRequest, QCreditRequestSearchObject, EntitySortBy, QCreditRequestIncludes, QCreditRequestDto, QCreditRequestInputDto>;
