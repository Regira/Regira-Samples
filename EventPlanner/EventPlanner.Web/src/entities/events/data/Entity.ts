import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as EventCategory } from "@/entities/event-categories"
import type { Entity as LocationItem } from "@/entities/locations"
import type { Entity as Session } from "../sessions"

// mirrors the C# enum EventStatus (serialized by name)
export const EventStatus = { Draft: "Draft", Published: "Published", Cancelled: "Cancelled" } as const
export type EventStatus = (typeof EventStatus)[keyof typeof EventStatus]

const MONTHS = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
const todayIso = () => new Date().toLocaleDateString("sv-SE") // yyyy-MM-dd in local time

// Named EventItem (not Event) so it never shadows the DOM's window.Event global
export class EventItem extends EntityBase {
    id: number = 0
    title = ""
    summary?: string
    description?: string
    categoryId?: number
    category?: EventCategory // eager-loaded by the API on every row
    locationId?: number
    location?: LocationItem // eager-loaded by the API on every row
    // DateOnly on the server: kept as "yyyy-MM-dd" strings (never hydrated to Date)
    startDate = todayIso()
    endDate = todayIso()
    status: EventStatus = EventStatus.Draft
    isFeatured = false
    maxParticipants?: number
    bannerUrl?: string
    sessions?: Array<Session> // owned (Related) — loaded on details or with ?includes=Sessions
    // computed by the API (EventProcessor), read-only
    registrationCount?: number
    waitlistCount?: number
    myRegistrationId?: number

    created?: Date
    lastModified?: Date

    get $color(): string {
        return (this.category as any)?.color || "#6366f1"
    }
    get $icon(): string {
        return `bi bi-${(this.category as any)?.icon || "calendar-event"}`
    }
    get $startDay(): string {
        return this.startDate?.substring(8, 10) ?? ""
    }
    get $startMonth(): string {
        return MONTHS[parseInt(this.startDate?.substring(5, 7) ?? "1") - 1] ?? ""
    }
    get $dateRange(): string {
        const fmt = (iso: string) => new Date(iso + "T12:00:00").toLocaleDateString(undefined, { weekday: "short", day: "numeric", month: "short", year: "numeric" })
        if (!this.startDate) return ""
        return this.startDate === this.endDate || !this.endDate ? fmt(this.startDate) : `${fmt(this.startDate)} - ${fmt(this.endDate)}`
    }
    get $dayCount(): number {
        if (!this.startDate || !this.endDate) return 1
        return Math.round((Date.parse(this.endDate) - Date.parse(this.startDate)) / 86400000) + 1
    }
    get $isPast(): boolean {
        return !!this.endDate && this.endDate < todayIso()
    }
    get $spotsLeft(): number | undefined {
        return this.maxParticipants == null ? undefined : Math.max(0, this.maxParticipants - (this.registrationCount ?? 0))
    }
    get $fillPercent(): number {
        return this.maxParticipants ? Math.min(100, Math.round(((this.registrationCount ?? 0) / this.maxParticipants) * 100)) : 0
    }
    get $isOpen(): boolean {
        return this.status === EventStatus.Published && !this.$isPast
    }
    get $bannerStyle(): Record<string, string> {
        const c = this.$color
        const gradient = `linear-gradient(135deg, ${c} 0%, ${c}cc 45%, #1e1b4b 100%)`
        return this.bannerUrl
            ? { backgroundImage: `linear-gradient(135deg, ${c}cc, #1e1b4bcc), url("${this.bannerUrl}")`, backgroundSize: "cover", backgroundPosition: "center" }
            : { backgroundImage: gradient }
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = EventItem
export default EventItem
