// erasableSyntaxOnly-safe const maps (not enums). Role names match the API's Identity roles exactly.
export const Roles = { ADMIN: "Admin", AGENT: "Agent", CUSTOMER: "Customer" } as const
export type Role = (typeof Roles)[keyof typeof Roles]

export const Permissions = { CAN_READ: "can_read", CAN_WRITE: "can_write", ADMIN: "admin" } as const
export type Permission = (typeof Permissions)[keyof typeof Permissions]
