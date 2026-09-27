import { EntityBase } from "@regira/modules/vue/entities"

export const PromotionThemes = ["sunset", "ocean", "forest", "berry", "midnight"] as const
export type PromotionTheme = (typeof PromotionThemes)[number]

export class Promotion extends EntityBase {
    id: number = 0
    title = ""
    subtitle?: string
    badge?: string
    ctaLabel?: string
    ctaLink?: string // storefront route, e.g. "/shop?onSale=true"
    theme?: string
    icon?: string // Bootstrap icon name without the "bi-" prefix
    isActive = true
    startDate?: Date
    endDate?: Date
    sortOrder = 0

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    /** active and today falls inside its validity period */
    get $isLive(): boolean {
        const now = Date.now()
        return this.isActive && (!this.startDate || this.startDate.getTime() <= now) && (!this.endDate || this.endDate.getTime() >= now)
    }
}

export const Entity = Promotion
export default Promotion
