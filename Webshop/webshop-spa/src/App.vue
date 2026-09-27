<script setup lang="ts">
import { computed } from "vue"
import { RouterView, useRoute } from "vue-router"
import { Feedback, LoadingContainer } from "@regira/modules/vue/ui"
import { AppStatus } from "@regira/modules/vue/app"
import TheHeader from "@/components/layout/TheHeader.vue"
import TheFooter from "@/components/layout/TheFooter.vue"
import Main from "@/components/layout/Main.vue"

const route = useRoute()
// the storefront brings its own chrome (ShopLayout); everything else is the back office shell
const isShop = computed(() => route.meta.layout === "shop")
</script>

<template>
    <LoadingContainer v-if="isShop" :is-loading="$appStatus !== AppStatus.Ready">
        <RouterView />
    </LoadingContainer>
    <div v-else class="page ws-admin">
        <header class="container-fluid ws-admin-header"><TheHeader /></header>
        <section class="container-fluid position-relative overflow-hidden">
            <Feedback :feedback="$feedback" :enable-error-popup="true" />
        </section>
        <main class="container-fluid">
            <LoadingContainer :is-loading="$appStatus !== AppStatus.Ready">
                <Main />
            </LoadingContainer>
        </main>
        <footer class="container-fluid bg-light"><TheFooter /></footer>
    </div>
</template>
