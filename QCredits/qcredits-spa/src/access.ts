import { useAuthStore } from "@regira/modules/vue/auth"
import { Roles } from "@/infrastructure/permissions"

// The SPA's single mirror of the API's write tiers (QCredits.Api -> WriteAuthorizationFilter).
// Gating is not authorization: the API stays the only enforcement point.
const WRITERS: Record<string, Array<string>> = {
    Department: [Roles.ADMIN],
    Employee: [Roles.ADMIN],
    CreditYear: [Roles.ADMIN],
    CreditAllocation: [Roles.ADMIN],
    GroupTraining: [Roles.ADMIN],
    CreditRequest: [Roles.ADMIN, Roles.EMPLOYEE], // row-scoped on the server: employees only see/edit their own drafts
}

// entities an employee has no reason to browse (the API scopes them to the employee's own row anyway)
const ADMIN_ONLY = ["Employee", "Department", "CreditYear"]

export function useAccess() {
    const store = useAuthStore()
    const isAdmin = () => store.hasRole(Roles.ADMIN)
    return {
        isAdmin,
        canWrite: (key: string) => (WRITERS[key] ?? []).some((r) => store.hasRole(r)),
        canBrowse: (key: string) => !ADMIN_ONLY.includes(key) || isAdmin(),
    }
}
