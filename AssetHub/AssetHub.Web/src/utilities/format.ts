// Formatting helpers that reference no app type. Dates from the API arrive as ISO strings
// (DateTime -> "...Z", DateOnly -> "yyyy-MM-dd"); both are accepted here.
const culture = "en-GB"

function toDate(value?: string | Date | null): Date | undefined {
    if (value == null || value === "") return undefined
    const d = value instanceof Date ? value : new Date(value)
    return isNaN(d.getTime()) ? undefined : d
}

export function fmtDate(value?: string | Date | null): string {
    const d = toDate(value)
    return d ? new Intl.DateTimeFormat(culture, { day: "2-digit", month: "short", year: "numeric" }).format(d) : ""
}

export function fmtDateTime(value?: string | Date | null): string {
    const d = toDate(value)
    return d ? new Intl.DateTimeFormat(culture, { day: "2-digit", month: "short", year: "numeric", hour: "2-digit", minute: "2-digit" }).format(d) : ""
}

export function fmtMoney(value?: number | null): string {
    return value == null ? "" : new Intl.NumberFormat(culture, { style: "currency", currency: "EUR" }).format(value)
}

/** Whole days from today to the given date (negative = in the past) */
export function daysUntil(value?: string | Date | null): number | undefined {
    const d = toDate(value)
    if (!d) return undefined
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    return Math.round((d.getTime() - today.getTime()) / 86_400_000)
}

/** "yyyy-MM-dd" for today + offset days (DateOnly wire format) */
export function isoDay(offsetDays = 0): string {
    const d = new Date()
    d.setDate(d.getDate() + offsetDays)
    const pad = (n: number) => String(n).padStart(2, "0")
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

export function durationDays(from?: string | Date | null, to?: string | Date | null): number | undefined {
    const a = toDate(from)
    const b = toDate(to) ?? new Date()
    return a ? Math.max(0, Math.round((b.getTime() - a.getTime()) / 86_400_000)) : undefined
}
