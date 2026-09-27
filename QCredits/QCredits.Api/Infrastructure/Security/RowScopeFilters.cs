using QCredits.Api.Entities.CreditAllocations;
using QCredits.Api.Entities.CreditRequests;
using QCredits.Api.Entities.Employees;
using Regira.Entities.Models.Abstractions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace QCredits.Api.Infrastructure.Security;

// Row-level security: administrators (and trusted writers) see everything,
// an employee only sees the rows that belong to their own Employee record (matched on e-mail).
// No early return on a null search object: Details(id) and the write existence checks run through here too.

public class EmployeeScopeFilter(AccessScope scope) : GlobalFilteredQueryBuilderBase<Employee>
{
    public override IQueryable<Employee> Build(IQueryable<Employee> query, ISearchObject<int>? so)
    {
        if (scope.IsUnrestricted) return query;
        var email = scope.Email;
        if (email == null) return query.Where(_ => false);
        return query.Where(x => x.Email == email);
    }
}

public class CreditAllocationScopeFilter(AccessScope scope) : GlobalFilteredQueryBuilderBase<CreditAllocation>
{
    public override IQueryable<CreditAllocation> Build(IQueryable<CreditAllocation> query, ISearchObject<int>? so)
    {
        if (scope.IsUnrestricted) return query;
        var email = scope.Email;
        if (email == null) return query.Where(_ => false);
        return query.Where(x => x.Employee!.Email == email);
    }
}

public class CreditRequestScopeFilter(AccessScope scope) : GlobalFilteredQueryBuilderBase<CreditRequest>
{
    public override IQueryable<CreditRequest> Build(IQueryable<CreditRequest> query, ISearchObject<int>? so)
    {
        if (scope.IsUnrestricted) return query;
        var email = scope.Email;
        if (email == null) return query.Where(_ => false);
        return query.Where(x => x.Employee!.Email == email);
    }
}
