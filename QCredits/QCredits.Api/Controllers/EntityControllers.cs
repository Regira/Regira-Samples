using Microsoft.AspNetCore.Mvc;
using QCredits.Api.Entities.CreditAllocations;
using QCredits.Api.Entities.CreditRequests;
using QCredits.Api.Entities.CreditYears;
using QCredits.Api.Entities.Departments;
using QCredits.Api.Entities.Employees;
using QCredits.Api.Entities.GroupTrainings;
using Regira.Entities.Web.Controllers.Abstractions;

namespace QCredits.Api.Controllers;

// Generics mirror the .For<>() registrations exactly (register = N, controller = N + 2).
// Write access per controller is enforced by WriteAuthorizationFilter; row access by the scope filters.

[Route("departments")]
public class DepartmentsController : EntityControllerBase<Department, DepartmentDto, DepartmentInputDto>;

[Route("employees")]
public class EmployeesController : EntityControllerBase<Employee, int, EmployeeSearchObject, EmployeeDto, EmployeeInputDto>;

[Route("credit-years")]
public class CreditYearsController : EntityControllerBase<CreditYear, CreditYearDto, CreditYearInputDto>;

[Route("credit-allocations")]
public class CreditAllocationsController : EntityControllerBase<CreditAllocation, int, CreditAllocationSearchObject, CreditAllocationDto, CreditAllocationInputDto>;

[Route("credit-requests")]
public class CreditRequestsController : EntityControllerBase<CreditRequest, int, CreditRequestSearchObject, CreditRequestSortBy, CreditRequestIncludes, CreditRequestDto, CreditRequestInputDto>;

[Route("group-trainings")]
public class GroupTrainingsController : EntityControllerBase<GroupTraining, int, GroupTrainingSearchObject, GroupTrainingSortBy, GroupTrainingIncludes, GroupTrainingDto, GroupTrainingInputDto>;
