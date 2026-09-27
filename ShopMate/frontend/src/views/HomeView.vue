<!-- Home: the current shopper's lists (pinned first) + the config-driven dashboard for everything else. -->
<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { RouterLink } from "vue-router"
import { LoadingContainer, Feedback, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { Dashboard } from "@/components/entity-navigation"
import { useEntityStore as useShoppingListStore, type Entity as ShoppingList } from "@/entities/shopping-lists"
import ListCard from "@/entities/shopping-lists/overview/ListCard.vue"
import useCurrentShopper from "@/infrastructure/current-shopper"

const current = useCurrentShopper()
const listStore = useShoppingListStore()
const feedback = useFeedback()
const lists = ref<Array<ShoppingList>>()
const count = ref(0)
const isLoading = ref(false)
const PAGE = 20

const firstName = computed(() => current.shopper?.name.split(" ")[0])
const openCount = computed(() => (lists.value ?? []).reduce((sum, x) => sum + (x.activeCount ?? 0), 0))

async function load() {
    if (!current.shopperId) return
    isLoading.value = true
    try {
        const result = await listStore.service.search({ shopperId: current.shopperId, pageSize: PAGE })
        lists.value = listStore.fromPool(result.items)
        count.value = result.count
    } catch (ex) {
        console.error(ex)
        feedback.fail("Loading your lists failed", toFeedbackError(ex))
    } finally {
        isLoading.value = false
    }
}
watch(() => current.shopperId, load, { immediate: true })
</script>

<template>
    <section class="sm-home">
        <div class="sm-hello">
            <h1>{{ firstName ? $t("hello", { name: firstName }) : $t("welcome") }}</h1>
            <p v-if="lists" class="mb-0">{{ $t("homeSummary", { lists: count, open: openCount }) }}</p>
        </div>

        <Feedback :feedback="feedback" />

        <div class="sm-section-title">
            <span>{{ $t("myLists") }}</span>
            <RouterLink v-if="count > PAGE" :to="{ name: 'ShoppingListOverview', query: { shopperId: current.shopperId } }" class="small">
                {{ $t("showAll") }} ({{ count }})
            </RouterLink>
        </div>
        <LoadingContainer :is-loading="isLoading && !lists">
            <div class="sm-list-grid">
                <ListCard v-for="item in lists ?? []" :key="item.id" :item="item" />
                <RouterLink
                    :to="{ name: 'ShoppingListDetails', params: { id: 'new' } }"
                    class="sm-list-card sm-list-card--new"
                >
                    <i class="bi bi-plus-circle-dotted fs-3"></i>
                    <span>{{ $t("newList") }}</span>
                </RouterLink>
            </div>
        </LoadingContainer>

        <div class="sm-section-title mt-4"><span>{{ $t("browse") }}</span></div>
        <Dashboard class="sm-dashboard" />
    </section>
</template>
