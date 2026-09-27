import { useConfig } from "@/app-config"

/** Long editorial date, e.g. "12 March 2026". */
export function formatLongDate(date?: Date | string): string {
    if (!date) return ""
    const d = typeof date === "string" ? new Date(date) : date
    return d.toLocaleDateString(useConfig().culture || "en-GB", { day: "numeric", month: "long", year: "numeric" })
}

/** Short editorial date, e.g. "12 Mar 2026". */
export function formatShortDate(date?: Date | string): string {
    if (!date) return ""
    const d = typeof date === "string" ? new Date(date) : date
    return d.toLocaleDateString(useConfig().culture || "en-GB", { day: "numeric", month: "short", year: "numeric" })
}

export function initials(name?: string): string {
    return (name ?? "")
        .split(/\s+/)
        .filter(Boolean)
        .slice(0, 2)
        .map((x) => x[0]!.toUpperCase())
        .join("")
}
