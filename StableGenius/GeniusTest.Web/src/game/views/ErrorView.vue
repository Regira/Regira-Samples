<script setup lang="ts">
import { computed } from "vue"
import GameLayout from "../components/GameLayout.vue"
import { useScreenTexts } from "../texts"

// Error pages that never, ever blame the player. The jokes live in public/data/screen-texts.csv
// (groups errorEmoji / errorTitle / errorMessage with the HTTP code in Detail, plus errorExcuse).
const props = defineProps<{ code: 401 | 403 | 404 | 500; url?: string }>()

const { t, tFor } = useScreenTexts()
const page = computed(() => ({
    emoji: tFor("errorEmoji", props.code),
    title: tFor("errorTitle", props.code),
    message: tFor("errorMessage", props.code),
}))
const excuse = t("errorExcuse")
</script>

<template>
    <GameLayout>
        <section class="vsg-panel text-center vsg-error">
            <div class="vsg-error-code">{{ code }}</div>
            <div class="vsg-error-emoji vsg-gild vsg-wobble" aria-hidden="true">{{ page.emoji }}</div>
            <h1 class="vsg-error-title">{{ page.title }}</h1>
            <div class="vsg-divider" aria-hidden="true"></div>
            <p class="fs-4">{{ page.message }}</p>
            <p class="vsg-excuse">{{ excuse }}</p>
            <p v-if="url" class="small text-muted text-break">{{ t("errorUrl", { url }) }}</p>
            <RouterLink :to="{ name: 'home' }" class="vsg-btn vsg-btn-huge mt-2">{{ t("errorBack") }}</RouterLink>
            <p class="small mt-4 mb-0">{{ t("errorBonus") }}<br /><small class="text-muted">{{ t("errorBonusNote") }}</small></p>
        </section>
    </GameLayout>
</template>

<style scoped>
.vsg-error {
    margin-top: 2rem;
}
.vsg-error-code {
    font-family: var(--vsg-display);
    font-weight: 900;
    font-size: clamp(4rem, 18vw, 7rem);
    line-height: 1;
    background: var(--vsg-gold-text);
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
    filter: drop-shadow(0 3px 0 rgba(138, 106, 31, 0.45));
}
.vsg-error-emoji {
    font-size: 3.5rem;
}
.vsg-error-title {
    font-family: var(--vsg-caps);
    font-weight: 900;
    color: var(--vsg-ink);
    font-size: clamp(1.4rem, 5vw, 2.1rem);
    margin: 0.5rem 0 0;
}
.vsg-excuse {
    font-family: var(--vsg-script);
    font-size: 1.9rem;
    color: var(--vsg-gold-deep);
    line-height: 1.2;
}
</style>
