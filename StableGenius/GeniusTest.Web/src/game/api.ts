import { useAxios } from "@regira/modules/vue/http"

// Wire contract of the API's /play endpoints (GeniusTest.Api/Services/Play/PlayContracts.cs)
export type QuestionType = "Choice" | "Number" | "Rating"

export interface PlayOption {
    id: number
    text: string
    /** rendered, but it runs away from every attempt (the API refuses it too) */
    unreachable: boolean
}
export interface PlayQuestion {
    id: number
    index: number
    title: string
    emoji?: string
    type: QuestionType
    category: string
    minValue?: number
    maxValue?: number
    unit?: string
    options: Array<PlayOption>
}
export interface AnswerResult {
    questionId: number
    index: number
    answerText?: string
    strategy: string
    headline?: string
    body?: string
    closer?: string
    points: number
    bonusLabel?: string
    rarity: number
    effect?: string
    legendary: boolean
    score: number
    level: number
    previousTitle?: string
    title?: string
    revealed: boolean
    isLast: boolean
}
export interface GameState {
    key: string
    playerName: string
    honorific?: string
    score: number
    level: number
    title?: string
    isFinished: boolean
    questions: Array<PlayQuestion>
    answers: Array<AnswerResult>
}
export interface AnswerRequest {
    questionId: number
    optionId?: number
    number?: number
    skipped?: boolean
}
export interface RevealResult {
    questionId: number
    officialAnswer?: string
    note?: string
    youWereRight: boolean
    praise?: string
    points: number
    score: number
    factsViewed: number
}
export interface LeaderboardEntry {
    rank: number
    name: string
    title?: string
    score: number
    isYou: boolean
}
export interface FinishResult {
    key: string
    playerName: string
    honorific?: string
    title?: string
    score: number
    iq: number
    iqNote?: string
    certificateNo?: string
    finished: string
    correct: number
    original: number
    skipped: number
    factsViewed: number
    leaderboard: Array<LeaderboardEntry>
}

// Relative to the axios base ("/api", config.json) - the same URL contract as the entity slices
export const playApi = {
    start: (playerName: string, honorific: string) => useAxios().post<GameState>("/play", { playerName, honorific }).then((r) => r.data),
    get: (key: string) => useAxios().get<GameState>(`/play/${key}`).then((r) => r.data),
    answer: (key: string, request: AnswerRequest) => useAxios().post<AnswerResult>(`/play/${key}/answers`, request).then((r) => r.data),
    reveal: (key: string, questionId: number) => useAxios().post<RevealResult>(`/play/${key}/reveal/${questionId}`).then((r) => r.data),
    finish: (key: string) => useAxios().post<FinishResult>(`/play/${key}/finish`).then((r) => r.data),
}
