import { useAuthStore } from "@regira/modules/vue/auth"
import { Roles } from "./permissions"

// The SPA's single mirror of the API's write tiers (AssetHub.Api/Infrastructure/Security/WriteAuthorizationFilter.cs).
// Keys are each slice's config.key. Gating removes affordances only - the API filter stays the enforcement point.
const WRITERS: Record<string, Array<string>> = {
    Category: [Roles.ADMIN],
    AssetStatus: [Roles.ADMIN],
    Location: [Roles.ADMIN],
    Supplier: [Roles.ADMIN],
    Asset: [Roles.ADMIN, Roles.MANAGER],
    Employee: [Roles.ADMIN, Roles.MANAGER],
}

export function useAccess() {
    const store = useAuthStore()
    return { canWrite: (key: string) => (WRITERS[key] ?? []).some((r) => store.hasRole(r)) }
}
