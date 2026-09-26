<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from "vue"
import { storeToRefs } from "pinia"
import useGameStore from "../store"
import { pick, reducedMotion, sounds } from "../effects"
import { useScreenTexts } from "../texts"

const store = useGameStore()
const { soundOn } = storeToRefs(store)

const { all, t } = useScreenTexts()
const ticker = [...all("headline")].sort(() => Math.random() - 0.5).join("  ★★★  ")

// A visitor counter stuck in 1998. It only goes up. Like your score.
const visitors = ref(999_999_000 + Math.floor(Math.random() * 900))
const digits = computed(() => visitors.value.toString().padStart(12, "0").split(""))
let counter: ReturnType<typeof setInterval> | undefined

// Sparkle cursor trail
let lastSparkle = 0
function sparkle(e: PointerEvent) {
    const now = performance.now()
    if (now - lastSparkle < 45) return
    lastSparkle = now
    const s = document.createElement("span")
    s.className = "vsg-sparkle"
    s.textContent = pick(["✨", "⭐", "💫"])
    s.style.left = `${e.clientX + 6}px`
    s.style.top = `${e.clientY + 6}px`
    document.body.appendChild(s)
    setTimeout(() => s.remove(), 800)
}

function toggleSound() {
    store.toggleSound()
    if (store.soundOn) sounds.fanfare()
}

onMounted(() => {
    counter = setInterval(() => (visitors.value += Math.ceil(Math.random() * 3)), 2500)
    if (!reducedMotion()) window.addEventListener("pointermove", sparkle)
})
onBeforeUnmount(() => {
    clearInterval(counter)
    window.removeEventListener("pointermove", sparkle)
})
</script>

<template>
    <div class="vsg">
        <div class="vsg-ticker" role="marquee" :aria-label="t('tickerAria')">
            <span class="vsg-ticker-label">{{ t("tickerLabel") }}</span>
            <div class="vsg-ticker-track">{{ ticker }}</div>
        </div>
        <header class="vsg-header">
            <RouterLink :to="{ name: 'home' }" class="vsg-logo">
                <span class="vsg-gild" aria-hidden="true">🦅</span>
                <span class="vsg-logo-text">{{ t("logo") }}</span>
                <span class="vsg-gild vsg-mirror" aria-hidden="true">🦅</span>
            </RouterLink>
            <button type="button" class="vsg-sound" :class="{ 'is-on': soundOn }" @click="toggleSound">
                {{ soundOn ? t("soundOn") : t("soundOff") }}
            </button>
        </header>
        <div class="vsg-trim" aria-hidden="true"></div>

        <main class="vsg-main">
            <slot />
        </main>

        <footer class="vsg-footer">
            <div class="vsg-trim mb-3" aria-hidden="true"></div>
            <div>
                {{ t("visitorBefore") }}
                <span class="vsg-counter"><span v-for="(d, i) in digits" :key="i" class="digit">{{ d }}</span></span>
                <span class="vsg-blink">{{ t("visitorAfter") }}</span>
            </div>
            <div class="my-2" aria-hidden="true">
                <span v-for="seal in all('seal')" :key="seal" class="vsg-seal-badge">{{ seal }}</span>
            </div>
            <div>{{ t("footerNote") }} <RouterLink :to="{ name: 'admin' }" class="vsg-staff-link">{{ t("staffLink") }}</RouterLink></div>
            <small class="d-block mt-1">{{ t("disclaimer") }}</small>
        </footer>
    </div>
</template>

<style scoped>
.vsg-mirror {
    transform: scaleX(-1);
}
/* the staff entrance stays, it just doesn't advertise itself */
.vsg-staff-link {
    color: inherit !important;
    opacity: 0.55;
    font-size: 0.85em;
    text-decoration: none;
}
</style>
