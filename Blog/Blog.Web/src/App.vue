<script setup lang="ts">
import { computed } from "vue"
import { useRoute } from "vue-router"
import { Feedback, LoadingContainer } from "@regira/modules/vue/ui"
import { AppStatus } from "@regira/modules/vue/app"
import TheHeader from "@/components/layout/TheHeader.vue"
import TheFooter from "@/components/layout/TheFooter.vue"
import Main from "@/components/layout/Main.vue"

const route = useRoute()
// public (reader) pages get the editorial chrome; management pages the compact admin chrome
const isPublic = computed(() => route.meta.public === true)
</script>

<template>
    <div class="page" :class="isPublic ? 'page--public' : 'page--admin'">
        <header><TheHeader :is-public="isPublic" /></header>
        <section class="container-fluid position-relative overflow-hidden">
            <Feedback :feedback="$feedback" :enable-error-popup="true" />
        </section>
        <main :class="isPublic ? 'container-xl' : 'container-fluid'">
            <LoadingContainer :is-loading="$appStatus !== AppStatus.Ready">
                <Main />
            </LoadingContainer>
        </main>
        <footer><TheFooter /></footer>
    </div>
</template>
