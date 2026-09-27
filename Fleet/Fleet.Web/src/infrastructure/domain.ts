// Domain enums mirrored from the API (serialized as names). Const objects + union types — no TS `enum`
// (erasableSyntaxOnly). Each value carries a label, a badge style and an icon, so a status is never colour-only.
import { formatDate, formatCurrency, formatNumber } from "@regira/modules/vue/formatters"

export type BadgeInfo = { label: string; css: string; icon: string }

export const VehicleTypes = ["Car", "Van", "Truck", "Bus", "Motorcycle", "Trailer"] as const
export type VehicleType = (typeof VehicleTypes)[number]
export const vehicleTypeIcon: Record<string, string> = {
    Car: "bi bi-car-front",
    Van: "bi bi-truck-front",
    Truck: "bi bi-truck",
    Bus: "bi bi-bus-front",
    Motorcycle: "bi bi-bicycle",
    Trailer: "bi bi-box-seam",
}

export const FuelTypes = ["Petrol", "Diesel", "Electric", "Hybrid", "Lpg", "Cng", "None"] as const
export type FuelType = (typeof FuelTypes)[number]
export const fuelLabel: Record<string, string> = { Lpg: "LPG", Cng: "CNG", None: "n/a" }

export const VehicleStatuses = ["Active", "InMaintenance", "OutOfService", "Retired"] as const
export type VehicleStatus = (typeof VehicleStatuses)[number]
export const vehicleStatusBadge: Record<string, BadgeInfo> = {
    Active: { label: "Active", css: "fleet-badge--good", icon: "bi bi-check-circle" },
    InMaintenance: { label: "In maintenance", css: "fleet-badge--warning", icon: "bi bi-wrench" },
    OutOfService: { label: "Out of service", css: "fleet-badge--critical", icon: "bi bi-x-octagon" },
    Retired: { label: "Retired", css: "fleet-badge--neutral", icon: "bi bi-archive" },
}

export const InterventionStatuses = ["Planned", "InProgress", "Completed", "Cancelled"] as const
export type InterventionStatus = (typeof InterventionStatuses)[number]
export const interventionStatusBadge: Record<string, BadgeInfo> = {
    Planned: { label: "Planned", css: "fleet-badge--info", icon: "bi bi-calendar-event" },
    InProgress: { label: "In progress", css: "fleet-badge--warning", icon: "bi bi-gear" },
    Completed: { label: "Completed", css: "fleet-badge--good", icon: "bi bi-check-circle" },
    Cancelled: { label: "Cancelled", css: "fleet-badge--neutral", icon: "bi bi-slash-circle" },
}

export const Priorities = ["Low", "Normal", "High", "Urgent"] as const
export type Priority = (typeof Priorities)[number]
export const priorityBadge: Record<string, BadgeInfo> = {
    Low: { label: "Low", css: "fleet-badge--neutral", icon: "bi bi-arrow-down" },
    Normal: { label: "Normal", css: "fleet-badge--info", icon: "bi bi-dash" },
    High: { label: "High", css: "fleet-badge--serious", icon: "bi bi-arrow-up" },
    Urgent: { label: "Urgent", css: "fleet-badge--critical", icon: "bi bi-exclamation-triangle" },
}

export const InvoiceStatuses = ["Received", "Approved", "Paid", "Disputed"] as const
export type InvoiceStatus = (typeof InvoiceStatuses)[number]
export const invoiceStatusBadge: Record<string, BadgeInfo> = {
    Received: { label: "Received", css: "fleet-badge--info", icon: "bi bi-inbox" },
    Approved: { label: "Approved", css: "fleet-badge--warning", icon: "bi bi-hand-thumbs-up" },
    Paid: { label: "Paid", css: "fleet-badge--good", icon: "bi bi-check-circle" },
    Disputed: { label: "Disputed", css: "fleet-badge--critical", icon: "bi bi-exclamation-octagon" },
}

export const InterventionCategories = ["Maintenance", "Repair", "Inspection", "Tires", "Bodywork", "Electrical", "Cleaning"] as const
export type InterventionCategory = (typeof InterventionCategories)[number]
export const categoryIcon: Record<string, string> = {
    Maintenance: "bi bi-wrench-adjustable",
    Repair: "bi bi-tools",
    Inspection: "bi bi-clipboard-check",
    Tires: "bi bi-circle",
    Bodywork: "bi bi-brush",
    Electrical: "bi bi-lightning-charge",
    Cleaning: "bi bi-droplet",
}

// culture for all formatting (dd/MM/yyyy, EUR)
export const culture = "en-GB"

/** today as the API's DateOnly string (yyyy-MM-dd), in local time */
export function todayIso(offsetDays = 0): string {
    const d = new Date()
    d.setDate(d.getDate() + offsetDays)
    return toIsoDay(d)
}
export function toIsoDay(d: Date): string {
    const pad = (n: number) => String(n).padStart(2, "0")
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}
/** formats an API DateOnly string ("2026-09-27") without a timezone shift */
export function fmtDay(value?: string | null): string {
    if (!value) return ""
    const [y, m, d] = value.split("-").map(Number)
    return formatDate(new Date(y!, (m ?? 1) - 1, d ?? 1), culture)
}
export function fmtMoney(value?: number | null): string {
    return value == null ? "" : formatCurrency(value, culture, "EUR")
}
export function fmtKm(value?: number | null): string {
    return value == null ? "" : `${formatNumber(value, culture, 0)} km`
}
/** days between today and a DateOnly string (negative = in the past) */
export function daysFromToday(value?: string | null): number | undefined {
    if (!value) return undefined
    const [y, m, d] = value.split("-").map(Number)
    const target = new Date(y!, (m ?? 1) - 1, d ?? 1)
    const now = new Date()
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
    return Math.round((target.getTime() - today.getTime()) / 86400000)
}
