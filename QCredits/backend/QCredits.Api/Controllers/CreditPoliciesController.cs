using Microsoft.AspNetCore.Mvc;
using Regira.Entities.Web.Controllers.Abstractions;
using QCredits.Api.Entities.CreditPolicies;

namespace QCredits.Api.Controllers;

[ApiController, Route("credit-policies")]
public class CreditPoliciesController : EntityControllerBase<CreditPolicy, int, CreditPolicySearchObject, CreditPolicyDto, CreditPolicyInputDto>;
