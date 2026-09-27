<script setup lang="ts">
import { RouterView } from "vue-router"
import { Feedback } from "@regira/modules/vue/ui"
import ShopHeader from "./components/ShopHeader.vue"
import ShopFooter from "./components/ShopFooter.vue"
</script>

<template>
    <div class="ws-shop">
        <ShopHeader />
        <!-- app-wide feedback (add-to-cart confirmations, errors) as a floating toast -->
        <div class="ws-toast">
            <Feedback :feedback="$feedback" :enable-error-popup="true" />
        </div>
        <main class="ws-main">
            <RouterView />
        </main>
        <ShopFooter />
    </div>
</template>
