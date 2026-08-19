<script setup lang="ts">
import { Feedback, LoadingContainer } from "@regira/modules/vue/ui"
import { AppStatus } from "@regira/modules/vue/app"
import TheHeader from "@/components/layout/TheHeader.vue"
import BottomNav from "@/components/layout/BottomNav.vue"
import Main from "@/components/layout/Main.vue"
</script>

<template>
    <div class="page">
        <TheHeader />
        <div class="position-relative">
            <Feedback :feedback="$feedback" :enable-error-popup="true" />
        </div>
        <main>
            <LoadingContainer :is-loading="$appStatus !== AppStatus.Ready">
                <Main />
            </LoadingContainer>
        </main>
        <BottomNav />
    </div>
</template>
