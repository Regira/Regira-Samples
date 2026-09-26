import { defineStore } from "pinia"
import { computed, ref } from "vue"
import { playApi, type AnswerRequest, type AnswerResult, type FinishResult, type GameState } from "./api"

const LAST_GAME_KEY = "genius.lastGame"
const SOUND_KEY = "genius.sound"

// Browser storage can be unavailable (private mode, blocked site data) - the game works without it.
function readStorage(key: string): string | undefined {
    try {
        return localStorage.getItem(key) ?? undefined
    } catch {
        return undefined
    }
}
function writeStorage(key: string, value?: string) {
    try {
        if (value == null) localStorage.removeItem(key)
        else localStorage.setItem(key, value)
    } catch {
        /* no storage, no problem */
    }
}

export const useGameStore = defineStore("genius-game", () => {
    const game = ref<GameState>()
    const result = ref<FinishResult>()
    const soundOn = ref(readStorage(SOUND_KEY) === "on")
    const lastGameKey = ref(readStorage(LAST_GAME_KEY))

    const answered = computed(() => new Map((game.value?.answers ?? []).map((a) => [a.questionId, a])))
    const currentQuestion = computed(() => game.value?.questions.find((q) => !answered.value.has(q.id)))
    const progress = computed(() => ({ done: game.value?.answers.length ?? 0, total: game.value?.questions.length ?? 10 }))

    async function start(playerName: string, honorific: string) {
        game.value = await playApi.start(playerName, honorific)
        result.value = undefined
        lastGameKey.value = game.value.key
        writeStorage(LAST_GAME_KEY, game.value.key)
        return game.value
    }
    async function load(key: string) {
        if (game.value?.key !== key) game.value = await playApi.get(key)
        return game.value
    }
    async function answer(request: AnswerRequest): Promise<AnswerResult> {
        const g = game.value!
        const reaction = await playApi.answer(g.key, request)
        g.answers = [...g.answers.filter((a) => a.questionId !== reaction.questionId), reaction]
        g.score = reaction.score
        g.level = reaction.level
        g.title = reaction.title
        return reaction
    }
    async function reveal(questionId: number) {
        const g = game.value!
        const revealed = await playApi.reveal(g.key, questionId)
        g.score = revealed.score
        const a = answered.value.get(questionId)
        if (a) a.revealed = true
        return revealed
    }
    async function finish(key: string) {
        result.value = await playApi.finish(key)
        if (lastGameKey.value === key) {
            lastGameKey.value = undefined
            writeStorage(LAST_GAME_KEY)
        }
        return result.value
    }
    function toggleSound() {
        soundOn.value = !soundOn.value
        writeStorage(SOUND_KEY, soundOn.value ? "on" : "off")
    }

    return { game, result, soundOn, lastGameKey, answered, currentQuestion, progress, start, load, answer, reveal, finish, toggleSound }
})

export default useGameStore
