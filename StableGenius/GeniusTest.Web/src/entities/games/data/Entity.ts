import { EntityBase } from "@regira/modules/vue/entities"

// A played question, read-only history (written by the play API, never by the admin form)
export interface GameAnswer {
    id: number
    gameId: number
    sortOrder: number
    questionId?: number
    questionText?: string
    answerText?: string
    skipped: boolean
    isCorrect?: boolean
    strategy: string
    rarity: number
    points: number
    reactionText?: string
    revealed: boolean
    answered?: string
}

export const GameSortBy = { CreatedDesc: "CreatedDesc", ScoreDesc: "ScoreDesc", PlayerName: "PlayerName" } as const
export type GameSortBy = (typeof GameSortBy)[keyof typeof GameSortBy]

export class Game extends EntityBase {
    id: number = 0
    playerName = ""
    honorific?: string
    // server-owned: only the play API writes these
    score = 0
    level = 0
    geniusTitle?: string
    iq?: number
    factsViewed = 0
    finished?: Date
    answers?: Array<GameAnswer>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.playerName
    }
}

export const Entity = Game
export default Game
