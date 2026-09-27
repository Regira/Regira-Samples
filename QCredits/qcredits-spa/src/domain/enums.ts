// Mirrors of the API enums (serialized as names). const maps + union types (erasableSyntaxOnly-safe).

export const RequestStatus = {
    Draft: "Draft",
    Submitted: "Submitted",
    Approved: "Approved",
    Rejected: "Rejected",
    Cancelled: "Cancelled",
} as const
export type RequestStatus = (typeof RequestStatus)[keyof typeof RequestStatus]

export const requestStatusBadge: Record<RequestStatus, string> = {
    Draft: "text-bg-secondary",
    Submitted: "text-bg-warning",
    Approved: "text-bg-success",
    Rejected: "text-bg-danger",
    Cancelled: "text-bg-light border",
}
export const requestStatusIcon: Record<RequestStatus, string> = {
    Draft: "bi bi-pencil",
    Submitted: "bi bi-hourglass-split",
    Approved: "bi bi-check-circle",
    Rejected: "bi bi-x-circle",
    Cancelled: "bi bi-slash-circle",
}

export const ActivityType = {
    Course: "Course",
    Book: "Book",
    OnlineSubscription: "OnlineSubscription",
    SelfStudy: "SelfStudy",
    Conference: "Conference",
    Certification: "Certification",
    Other: "Other",
} as const
export type ActivityType = (typeof ActivityType)[keyof typeof ActivityType]

export const activityTypeIcon: Record<ActivityType, string> = {
    Course: "bi bi-easel",
    Book: "bi bi-book",
    OnlineSubscription: "bi bi-laptop",
    SelfStudy: "bi bi-lightbulb",
    Conference: "bi bi-people",
    Certification: "bi bi-patch-check",
    Other: "bi bi-three-dots",
}

export const GroupTrainingStatus = {
    Planned: "Planned",
    Completed: "Completed",
    Cancelled: "Cancelled",
} as const
export type GroupTrainingStatus = (typeof GroupTrainingStatus)[keyof typeof GroupTrainingStatus]

export const groupTrainingStatusBadge: Record<GroupTrainingStatus, string> = {
    Planned: "text-bg-info",
    Completed: "text-bg-success",
    Cancelled: "text-bg-light border",
}

/** 1 QCredit = half a working day or EUR 250 */
export const CREDIT_VALUE_EUR = 250

export function formatCredits(value?: number | null): string {
    if (value == null) return "-"
    return (Math.round(value * 10) / 10).toLocaleString("en-GB", { maximumFractionDigits: 1 })
}
export function formatEuro(value?: number | null): string {
    if (value == null) return "-"
    return value.toLocaleString("en-GB", { style: "currency", currency: "EUR", maximumFractionDigits: 0 })
}

/** Year choices for filters and forms: next year back to three years ago. */
export function yearOptions(): Array<number> {
    const now = new Date().getFullYear()
    return [now + 1, now, now - 1, now - 2, now - 3]
}
