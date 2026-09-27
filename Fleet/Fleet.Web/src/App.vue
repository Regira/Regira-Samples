<script setup lang="ts">
import { ref, watch } from "vue"
import { useRoute } from "vue-router"
import { Feedback, LoadingContainer } from "@regira/modules/vue/ui"
import { AppStatus } from "@regira/modules/vue/app"
import TheHeader from "@/components/layout/TheHeader.vue"
import TheFooter from "@/components/layout/TheFooter.vue"
import Sidebar from "@/components/layout/Sidebar.vue"
import Main from "@/components/layout/Main.vue"

// Dashboard layout: fixed sidebar (lg+) / off-canvas drawer (small screens) + top bar + content
const menuOpen = ref(false)
const route = useRoute()
watch(() => route.fullPath, () => (menuOpen.value = false))
</script>

<template>
    <div class="fleet-shell" :class="{ 'is-menu-open': menuOpen }">
        <aside class="fleet-sidebar"><Sidebar @select="menuOpen = false" /></aside>
        <div class="fleet-backdrop d-lg-none" @click="menuOpen = false"></div>
        <div class="fleet-main">
            <header><TheHeader @toggle-menu="menuOpen = !menuOpen" /></header>
            <section class="position-relative overflow-hidden">
                <Feedback :feedback="$feedback" :enable-error-popup="true" />
            </section>
            <main class="fleet-content">
                <LoadingContainer :is-loading="$appStatus !== AppStatus.Ready">
                    <Main />
                </LoadingContainer>
            </main>
            <footer class="fleet-footer"><TheFooter /></footer>
        </div>
    </div>
</template>
