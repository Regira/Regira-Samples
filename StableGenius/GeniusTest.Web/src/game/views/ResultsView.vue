<script setup lang="ts">
import { computed, onMounted, ref } from "vue"
import { useRouter } from "vue-router"
import { LoadingContainer } from "@regira/modules/vue/ui"
import GameLayout from "../components/GameLayout.vue"
import DunningKrugerChart from "../components/DunningKrugerChart.vue"
import Chandelier from "../components/Chandelier.vue"
import Laurel from "../components/Laurel.vue"
import useGameStore from "../store"
import { celebrate, sounds } from "../effects"
import { httpStatus, toErrorPage } from "../errors"
import { useScreenTexts } from "../texts"
import FillText from "../components/FillText.vue"

const props = defineProps<{ gameKey: string }>()
const router = useRouter()
const store = useGameStore()
const { t } = useScreenTexts()

const loading = ref(true)
const result = computed(() => (store.result?.key === props.gameKey ? store.result : undefined))
const who = computed(() => [result.value?.honorific, result.value?.playerName].filter(Boolean).join(" "))
const shared = ref(false)

onMounted(async () => {
    try {
        if (!result.value) await store.finish(props.gameKey) // idempotent: a finished game returns its result again
        celebrate("legendary")
        if (store.soundOn) sounds.fanfare()
    } catch (ex) {
        console.error(ex)
        // 400 = questions still open: back to the quiz, the victory lap isn't over yet
        if (httpStatus(ex) === 400) await router.replace({ name: "play", params: { key: props.gameKey } })
        else await toErrorPage(router, ex)
    } finally {
        loading.value = false
    }
})

const shareText = computed(() => (result.value ? t("shareText", { iq: result.value.iq, title: result.value.title }) : ""))
const shareUrl = location.origin + router.resolve({ name: "home" }).href // includes the app's base path (e.g. /stable-genius/)
async function share() {
    try {
        if (navigator.share) await navigator.share({ title: t("shareTitle"), text: shareText.value, url: shareUrl })
        else await navigator.clipboard.writeText(`${shareText.value} ${shareUrl}`)
        shared.value = true
    } catch {
        /* the player changed their mind - also a genius move */
    }
}
const printCertificate = () => window.print()
const playAgain = () => router.push({ name: "home" })
const formatDate = (value?: string) => (value ? new Date(value).toLocaleDateString(undefined, { day: "numeric", month: "long", year: "numeric" }) : "")
</script>

<template>
    <GameLayout>
        <LoadingContainer :is-loading="loading">
            <template v-if="result">
                <Chandelier />
                <h1 class="vsg-wordart mt-2">{{ t("resultsTitle") }}</h1>
                <p class="vsg-subtitle">{{ t("resultsSubtitle", { who }) }}</p>
                <p class="vsg-final-title"><span class="vsg-gild">🏆</span> {{ result.title }} <span class="vsg-gild">🏆</span></p>
                <div class="vsg-divider" aria-hidden="true"></div>

                <section class="vsg-panel text-center mt-4">
                    <Laurel :size="260">
                        <div class="vsg-iq-label">{{ t("iqLabel") }}</div>
                        <div class="vsg-iq">{{ result.iq }}</div>
                    </Laurel>
                    <p class="fs-5 fst-italic mb-0 mt-2">{{ result.iqNote }}</p>
                </section>

                <section class="row g-3 mb-4 text-center">
                    <div class="col-6 col-md-3">
                        <div class="vsg-stat"><span class="vsg-gild">✅</span>{{ result.correct }}<small>{{ t("statCorrect") }}</small></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="vsg-stat"><span class="vsg-gild">🦄</span>{{ result.original }}<small>{{ t("statOriginal") }}</small></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="vsg-stat"><span class="vsg-gild">⏭️</span>{{ result.skipped }}<small>{{ t("statSkipped") }}</small></div>
                    </div>
                    <div class="col-6 col-md-3">
                        <div class="vsg-stat"><span class="vsg-gild">🔍</span>{{ result.factsViewed }}<small>{{ t("statFacts") }}</small></div>
                    </div>
                </section>

                <section class="vsg-panel">
                    <DunningKrugerChart :name="result.playerName" />
                </section>

                <!-- the certificate is also the print layout -->
                <section class="vsg-certificate" :aria-label="t('certAria')">
                    <div class="vsg-cert-inner">
                        <div class="vsg-cert-head">{{ t("certTitle") }}</div>
                        <p class="mb-1">{{ t("certIntro") }}</p>
                        <p class="vsg-cert-name">{{ who }}</p>
                        <p class="mb-1"><FillText :text="t('certIq')" :values="{ iq: result.iq }" /></p>
                        <p class="mb-1"><FillText :text="t('certScore')" :values="{ score: result.score.toLocaleString() }" /></p>
                        <p><FillText :text="t('certTitleLine')" :values="{ title: result.title }" /></p>
                        <div class="vsg-cert-foot">
                            <div>{{ formatDate(result.finished) }}<br /><small>{{ t("certNumber", { number: result.certificateNo }) }}</small></div>
                            <div class="vsg-seal vsg-gild" aria-hidden="true">🏅</div>
                            <div class="vsg-signature">{{ t("certSignature") }}</div>
                        </div>
                    </div>
                </section>

                <section class="vsg-panel">
                    <h2 class="vsg-section-title">{{ t("leaderboardTitle") }}</h2>
                    <div class="vsg-divider" aria-hidden="true"></div>
                    <ol class="vsg-leaderboard list-unstyled mb-1">
                        <li v-for="entry in result.leaderboard" :key="entry.rank" :class="{ 'is-you': entry.isYou }">
                            <span class="vsg-rank">#{{ entry.rank }}</span>
                            <span class="vsg-lb-name text-truncate"><span v-if="entry.isYou" class="vsg-gild me-1">👑</span>{{ entry.name }}</span>
                            <span class="vsg-lb-score">{{ entry.score.toLocaleString() }}</span>
                        </li>
                    </ol>
                    <p class="small text-center text-muted mb-0">{{ t("leaderboardNote") }}</p>
                </section>

                <div class="d-flex flex-wrap justify-content-center gap-3 vsg-actions">
                    <button type="button" class="vsg-btn vsg-btn-huge" @click="playAgain">{{ t("playAgain") }}</button>
                    <button type="button" class="vsg-btn" @click="printCertificate">{{ t("printCertificate") }}</button>
                    <button type="button" class="vsg-btn" @click="share">{{ shared ? t("sharedLabel") : t("shareLabel") }}</button>
                </div>
            </template>
        </LoadingContainer>
    </GameLayout>
</template>

<style scoped lang="scss">
.vsg-final-title {
    font-family: var(--vsg-display);
    font-weight: 900;
    font-size: clamp(1.4rem, 5.5vw, 2.4rem);
    text-align: center;
    color: var(--vsg-gold-deep);
}
.vsg-iq-label {
    font-family: var(--vsg-caps);
    font-weight: 700;
    letter-spacing: 0.12em;
    text-transform: uppercase;
    font-size: 0.8rem;
    color: var(--vsg-ink-soft);
}
.vsg-iq {
    font-family: var(--vsg-display);
    font-weight: 900;
    font-size: clamp(3.5rem, 16vw, 5.2rem);
    line-height: 1;
    background: var(--vsg-gold-text);
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
    filter: drop-shadow(0 2px 0 rgba(138, 106, 31, 0.5));
}
.vsg-stat {
    background: #fff;
    border: 1px solid var(--vsg-gold);
    outline: 1px solid var(--vsg-gold);
    outline-offset: 4px;
    padding: 0.6rem 0.4rem;
    font-family: var(--vsg-caps);
    font-weight: 900;
    font-size: 2rem;
    height: 100%;
    span {
        display: block;
        font-size: 1.5rem;
    }
    small {
        display: block;
        font-family: var(--vsg-serif);
        font-weight: 400;
        font-style: italic;
        font-size: 0.8rem;
        color: var(--vsg-ink-soft);
    }
}
.vsg-section-title {
    font-family: var(--vsg-caps);
    font-weight: 900;
    color: var(--vsg-ink);
    text-align: center;
    font-size: 1.6rem;
    margin-top: 0.5rem;
    margin-bottom: 0;
}
.vsg-certificate {
    background: #fffdf5;
    border: 12px solid transparent;
    border-image: var(--vsg-metal) 1;
    outline: 2px solid var(--vsg-gold-deep);
    padding: 6px;
    margin: 1rem 0 3rem;
    box-shadow: 0 16px 40px rgba(90, 70, 20, 0.25);
}
.vsg-cert-inner {
    border: 1px solid var(--vsg-gold);
    outline: 1px solid var(--vsg-gold);
    outline-offset: -6px;
    padding: 1.75rem 1rem;
    text-align: center;
    font-family: var(--vsg-serif);
    color: var(--vsg-ink);
}
.vsg-cert-head {
    font-family: var(--vsg-display);
    font-weight: 900;
    font-size: clamp(1.6rem, 6.5vw, 2.6rem);
    background: var(--vsg-gold-text);
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
    margin-bottom: 0.5rem;
}
.vsg-cert-name {
    font-family: var(--vsg-script);
    font-size: clamp(2.2rem, 8vw, 3.2rem);
    color: var(--vsg-gold-deep);
    border-bottom: 1px solid var(--vsg-gold);
    display: inline-block;
    padding: 0 1rem;
    line-height: 1.3;
}
.vsg-cert-foot {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 0.5rem;
    margin-top: 1rem;
    font-size: 0.85rem;
}
.vsg-seal {
    font-size: 3.5rem;
}
.vsg-signature {
    max-width: 11rem;
    font-family: var(--vsg-script);
    font-size: 1.4rem;
    line-height: 1.1;
    border-top: 1px solid var(--vsg-ink);
    padding-top: 0.25rem;
}
.vsg-leaderboard li {
    display: grid;
    grid-template-columns: 3rem 1fr auto;
    gap: 0.5rem;
    align-items: center;
    padding: 0.45rem 0.7rem;
    border-bottom: 1px solid rgba(201, 162, 39, 0.4);
    &.is-you {
        background: var(--vsg-metal);
        font-weight: 900;
        font-size: 1.2rem;
        border: 1px solid var(--vsg-gold-deep);
    }
}
.vsg-rank {
    font-family: var(--vsg-caps);
    font-weight: 900;
}
.vsg-lb-name {
    min-width: 0;
}
.vsg-lb-score {
    font-family: var(--vsg-caps);
    font-weight: 700;
}

@media print {
    // only the certificate goes to paper
    :global(.vsg-ticker),
    :global(.vsg-header),
    :global(.vsg-footer),
    :global(.vsg-trim),
    :global(.vsg-chandelier),
    .vsg-wordart,
    .vsg-subtitle,
    .vsg-final-title,
    .vsg-divider,
    .vsg-panel,
    .row,
    .vsg-actions {
        display: none !important;
    }
    :global(.vsg),
    :global(.vsg::before),
    :global(.vsg::after) {
        background: #fff !important;
        box-shadow: none !important;
    }
    .vsg-certificate {
        box-shadow: none;
    }
}
</style>
