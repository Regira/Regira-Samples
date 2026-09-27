import { computed } from "vue"
import { useAuthStore } from "@regira/modules/vue/auth"
import { Roles } from "./permissions"

// The SPA's single mirror of the API's WriteAuthorizationFilter (HelpDesk.Api/Infrastructure/Security).
// Keyed on each slice's config.key. Gating removes buttons; the server stays the enforcement point.
const ALL = [Roles.ADMIN, Roles.AGENT, Roles.CUSTOMER]
const STAFF = [Roles.ADMIN, Roles.AGENT]
const WRITERS: Record<string, Array<string>> = {
    SupportTeam: [Roles.ADMIN],
    Category: [Roles.ADMIN],
    Priority: [Roles.ADMIN],
    Status: [Roles.ADMIN],
    Person: STAFF,
    Ticket: ALL, // customers edit their own tickets; the API restores the staff-only fields
}
const DELETERS: Record<string, Array<string>> = {
    Ticket: STAFF,
    Person: [Roles.ADMIN],
}

export function useAccess() {
    const store = useAuthStore()
    const hasAny = (roles: Array<string>) => roles.some((r) => store.hasRole(r))
    const isAdmin = computed(() => store.hasRole(Roles.ADMIN))
    const isStaff = computed(() => hasAny(STAFF))
    return {
        isAdmin,
        isStaff,
        canWrite: (key: string) => hasAny(WRITERS[key] ?? []),
        canDelete: (key: string) => hasAny(DELETERS[key] ?? WRITERS[key] ?? []),
    }
}
