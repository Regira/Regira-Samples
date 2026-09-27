import { useAuthStore } from "@regira/modules/vue/auth"
import { Roles } from "@/infrastructure/permissions"

// The SPA's single mirror of the API's write tiers (EventPlanner.Api → Infrastructure/Security/WriteAuthorizationFilter.cs).
// Gating is not authorization — the API stays the only enforcement point; this only hides affordances a caller
// would get a 403 for. Keys are each slice's config.key.
const WRITERS: Record<string, Array<string>> = {
    EventItem: [Roles.ADMIN],
    LocationItem: [Roles.ADMIN],
    Speaker: [Roles.ADMIN],
    EventCategory: [Roles.ADMIN],
    // employees write their OWN registrations (row-scoped server-side), administrators everyone's
    Registration: [Roles.ADMIN, Roles.EMPLOYEE],
    // Employee: read-only directory — nobody writes it through this API
}

export function useAccess() {
    const store = useAuthStore()
    return {
        canWrite: (key: string) => (WRITERS[key] ?? []).some((r) => store.hasRole(r)),
        isAdmin: () => store.hasRole(Roles.ADMIN),
    }
}
