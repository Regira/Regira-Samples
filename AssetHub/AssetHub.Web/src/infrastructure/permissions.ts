// erasableSyntaxOnly-safe const maps (not enums)
// Roles match the Identity role names the API mints into the JWT "role" claim (Infrastructure/Security/Roles.cs).
export const Roles = { ADMIN: "Admin", MANAGER: "Manager" } as const
export type Role = (typeof Roles)[keyof typeof Roles]

export const Permissions = { CAN_READ: "can_read", CAN_WRITE: "can_write", ADMIN: "admin" } as const
export type Permission = (typeof Permissions)[keyof typeof Permissions]
