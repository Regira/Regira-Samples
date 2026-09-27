// Domain constants + date helpers shared by the planner views. No entity imports (utilities stay leaf modules).

// C# enums mirrored as const objects + union types (erasableSyntaxOnly forbids TS enums)
export const ReservationStatus = {
    Pending: "Pending",
    Approved: "Approved",
    PartiallyApproved: "PartiallyApproved",
    Rejected: "Rejected",
    Cancelled: "Cancelled",
} as const
export type ReservationStatus = (typeof ReservationStatus)[keyof typeof ReservationStatus]

export const RoomApprovalStatus = { Pending: "Pending", Approved: "Approved", Rejected: "Rejected" } as const
export type RoomApprovalStatus = (typeof RoomApprovalStatus)[keyof typeof RoomApprovalStatus]

export const AttendeeResponse = { NoResponse: "NoResponse", Accepted: "Accepted", Tentative: "Tentative", Declined: "Declined" } as const
export type AttendeeResponse = (typeof AttendeeResponse)[keyof typeof AttendeeResponse]

/** Bootstrap contextual colour per reservation status (badges, timeline bars). */
export const statusVariant: Record<string, string> = {
    Pending: "warning",
    Approved: "success",
    PartiallyApproved: "info",
    Rejected: "danger",
    Cancelled: "secondary",
    NoResponse: "secondary",
    Accepted: "success",
    Tentative: "warning",
    Declined: "danger",
}
export const statusIcon: Record<string, string> = {
    Pending: "bi bi-hourglass-split",
    Approved: "bi bi-check-circle",
    PartiallyApproved: "bi bi-check2-square",
    Rejected: "bi bi-x-circle",
    Cancelled: "bi bi-slash-circle",
}

export const MINUTE = 60_000
export const HOUR = 60 * MINUTE
export const DAY = 24 * HOUR

export function startOfDay(d: Date): Date {
    const x = new Date(d)
    x.setHours(0, 0, 0, 0)
    return x
}
export function addDays(d: Date, days: number): Date {
    const x = new Date(d)
    x.setDate(x.getDate() + days)
    return x
}
export function addMinutes(d: Date, minutes: number): Date {
    return new Date(d.getTime() + minutes * MINUTE)
}
/** Monday of the week containing d (local time). */
export function startOfWeek(d: Date): Date {
    const x = startOfDay(d)
    const dow = (x.getDay() + 6) % 7
    return addDays(x, -dow)
}
export function isSameDay(a: Date, b: Date): boolean {
    return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()
}
export function overlaps(aStart: Date, aEnd: Date, bStart: Date, bEnd: Date): boolean {
    return aStart < bEnd && aEnd > bStart
}
/** Rounds up to the next quarter hour. */
export function nextQuarter(d: Date = new Date()): Date {
    const x = new Date(d)
    x.setSeconds(0, 0)
    const m = x.getMinutes()
    x.setMinutes(m % 15 === 0 ? m : m + (15 - (m % 15)))
    return x
}
/** "09:30" for a local "HH:mm[:ss]" TimeOnly string → minutes since midnight. */
export function timeOnlyToMinutes(value?: string, fallback = 0): number {
    if (!value) return fallback
    const [h, m] = value.split(":").map((x) => parseInt(x, 10))
    return (h ?? 0) * 60 + (m ?? 0)
}

const two = (n: number) => n.toString().padStart(2, "0")
export function formatTime(d?: Date): string {
    return d ? `${two(d.getHours())}:${two(d.getMinutes())}` : ""
}
export function formatDay(d?: Date, options: Intl.DateTimeFormatOptions = { weekday: "short", day: "numeric", month: "short" }): string {
    return d ? d.toLocaleDateString(undefined, options) : ""
}
export function formatRange(start?: Date, end?: Date): string {
    if (!start || !end) return ""
    return isSameDay(start, end)
        ? `${formatDay(start)} ${formatTime(start)}–${formatTime(end)}`
        : `${formatDay(start)} ${formatTime(start)} – ${formatDay(end)} ${formatTime(end)}`
}
export function formatDuration(start?: Date, end?: Date): string {
    if (!start || !end) return ""
    const minutes = Math.round((end.getTime() - start.getTime()) / MINUTE)
    const h = Math.floor(minutes / 60)
    const m = minutes % 60
    return h ? (m ? `${h}h${two(m)}` : `${h}h`) : `${m}m`
}
/** yyyy-MM-dd of a local date (for <input type="date"> and route queries). */
export function toDateKey(d: Date): string {
    return `${d.getFullYear()}-${two(d.getMonth() + 1)}-${two(d.getDate())}`
}
export function fromDateKey(value?: unknown): Date | undefined {
    if (typeof value !== "string" || !/^\d{4}-\d{2}-\d{2}$/.test(value)) return undefined
    const [y, m, d] = value.split("-").map((x) => parseInt(x, 10))
    return new Date(y!, m! - 1, d!)
}
/** Local "yyyy-MM-ddTHH:mm" for <input type="datetime-local">. */
export function toLocalInput(d?: Date): string {
    return d ? `${toDateKey(d)}T${two(d.getHours())}:${two(d.getMinutes())}` : ""
}
export function fromLocalInput(value?: string): Date | undefined {
    if (!value) return undefined
    const d = new Date(value)
    return isNaN(d.getTime()) ? undefined : d
}
