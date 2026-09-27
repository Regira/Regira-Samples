// erasableSyntaxOnly-safe const maps (not enums)
// Roles match the Identity role names the API seeds (Infrastructure/Security/CurrentUser.cs → Roles).
export const Roles = { ADMIN: "Admin", EMPLOYEE: "Employee" } as const
export type Role = (typeof Roles)[keyof typeof Roles]

export const Permissions = { CAN_READ: "can_read", CAN_WRITE: "can_write", ADMIN: "admin" } as const
export type Permission = (typeof Permissions)[keyof typeof Permissions]
