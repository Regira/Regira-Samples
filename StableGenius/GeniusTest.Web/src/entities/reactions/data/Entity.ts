import { EntityBase } from "@regira/modules/vue/entities"

export const ReactionPart = { Opener: "Opener", Spin: "Spin", Closer: "Closer" } as const
export type ReactionPart = (typeof ReactionPart)[keyof typeof ReactionPart]

// "Custom" is deliberately absent: those reactions live on a question option
export const SpinStrategy = {
    Any: "Any",
    Correct: "Correct",
    BetterThanReality: "BetterThanReality",
    TopPercent: "TopPercent",
    Landslide: "Landslide",
    ThinkBig: "ThinkBig",
    LeanAndEfficient: "LeanAndEfficient",
    Rigged: "Rigged",
    JealousExperts: "JealousExperts",
    AheadOfScience: "AheadOfScience",
    TooModest: "TooModest",
    PowerMove: "PowerMove",
    Legendary: "Legendary",
    Reveal: "Reveal",
    Finale: "Finale",
    Confirmed: "Confirmed",
} as const
export type SpinStrategy = (typeof SpinStrategy)[keyof typeof SpinStrategy]

export const placeholders = ["{answer}", "{pct}", "{name}", "{honorific}", "{title}", "{years}", "{points}", "{iq}"]

export class Reaction extends EntityBase {
    id: number = 0
    part: ReactionPart = ReactionPart.Spin
    strategy: SpinStrategy = SpinStrategy.Any
    text = ""
    isActive = true

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.text
    }
}

export const Entity = Reaction
export default Reaction
