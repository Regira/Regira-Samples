<script setup lang="ts">
import { computed } from "vue"
import { useRoute } from "vue-router"
import { Feedback, LoadingContainer } from "@regira/modules/vue/ui"
import { AppStatus } from "@regira/modules/vue/app"
import TheHeader from "@/components/layout/TheHeader.vue"
import TheFooter from "@/components/layout/TheFooter.vue"
import Main from "@/components/layout/Main.vue"

const route = useRoute()
// the game brings its own (kitsch) chrome; the admin keeps the scaffolded header/footer
const isGame = computed(() => route.meta.layout === "game")
</script>

<template>
    <template v-if="isGame">
        <div class="vsg-feedback"><Feedback :feedback="$feedback" /></div>
        <LoadingContainer :is-loading="$appStatus !== AppStatus.Ready">
            <Main />
        </LoadingContainer>
    </template>
    <div v-else class="page admin-page">
        <header class="container-fluid"><TheHeader /></header>
        <section class="container-fluid position-relative overflow-hidden">
            <Feedback :feedback="$feedback" />
        </section>
        <main class="container-fluid">
            <LoadingContainer :is-loading="$appStatus !== AppStatus.Ready">
                <Main />
            </LoadingContainer>
        </main>
        <footer class="container-fluid"><TheFooter /></footer>
    </div>
</template>

<style>
.vsg-feedback {
    position: fixed;
    top: 0.5rem;
    left: 50%;
    transform: translateX(-50%);
    z-index: 2500;
    width: min(560px, calc(100% - 32px));
    font-family: var(--vsg-serif);
    font-weight: 700;
}
/* no scary red in the game: even a hiccup looks like a celebration */
.vsg-feedback .rg-error-summary {
    background: #fffdf7 !important;
    color: var(--vsg-ink) !important;
    border: 2px solid var(--vsg-gold) !important;
    outline: 1px solid var(--vsg-gold);
    outline-offset: 3px;
    box-shadow: 0 8px 20px rgba(90, 70, 20, 0.2);
}
.vsg-feedback .rg-error-summary .btn,
.vsg-feedback .rg-feedback__close-button {
    color: var(--vsg-gold-deep) !important;
}
.vsg-feedback .rg-feedback {
    border: none !important;
}
</style>
