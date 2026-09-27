<script setup lang="ts">
import { Feedback, LoadingContainer } from "@regira/modules/vue/ui"
import { AppStatus } from "@regira/modules/vue/app"
import TheHeader from "@/components/layout/TheHeader.vue"
import TheFooter from "@/components/layout/TheFooter.vue"
import Main from "@/components/layout/Main.vue"
import { onMounted } from "vue"
import { useCategoryTree } from "@/entities/categories"

// shared reference data: the category DAG behind every chip and filter (the current shopper loads in ShopperSwitcher)
const categoryTree = useCategoryTree()
onMounted(() => categoryTree.load())
</script>

<template>
    <div class="page sm-page">
        <header class="sm-header"><TheHeader /></header>
        <section class="sm-app-feedback">
            <Feedback :feedback="$feedback" :enable-error-popup="true" />
        </section>
        <main class="sm-main">
            <LoadingContainer :is-loading="$appStatus !== AppStatus.Ready">
                <Main />
            </LoadingContainer>
        </main>
        <footer class="sm-footer"><TheFooter /></footer>
    </div>
</template>
