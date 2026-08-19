<template>
    <div class="sm-article-manager">
        <form class="sm-add-row mb-3" @submit.prevent="handleAdd">
            <input v-model.trim="newTitle" class="form-control" :placeholder="$t('addItemPlaceholder')" />
            <button type="submit" class="btn btn-primary" :disabled="!newTitle.trim() || adding">
                <i class="bi bi-plus-lg"></i>
            </button>
        </form>

        <div class="sm-chip-row mb-2">
            <button type="button" class="sm-chip" :class="{ 'is-active': searchObject.isActive === true }" @click="toggleActiveFilter(true)">
                <i class="bi bi-circle"></i>{{ $t("toBuy") }}
            </button>
            <button type="button" class="sm-chip" :class="{ 'is-active': searchObject.isActive === false }" @click="toggleActiveFilter(false)">
                <i class="bi bi-check-circle"></i>{{ $t("bought") }}
            </button>
            <button
                v-for="c in categories"
                :key="c.id"
                type="button"
                class="sm-chip"
                :class="{ 'is-active': searchObject.categoryId === c.id }"
                @click="toggleCategoryFilter(c.id)"
            >
                <i class="bi" :class="c.icon || 'bi-tag'"></i>{{ c.title }}
            </button>
        </div>

        <input v-model.lazy.trim="searchObject.q" class="form-control mb-3" :placeholder="$t('searchItems')" @change="refresh" />

        <LoadingContainer :is-loading="isLoading">
            <div v-if="!sortedItems.length" class="text-muted text-center py-4">{{ $t("noResults") }}</div>
            <div class="sm-list-stack">
                <SwipeActions v-for="item in sortedItems" :key="item.id" class="sm-article-card sm-card" @tap="goToArticle(item)">
                    <template #left>
                        <button type="button" class="sm-swipe-btn sm-swipe-btn--muted" @click="reorder(item, -1)">
                            <i class="bi bi-arrow-up"></i>
                        </button>
                    </template>
                    <template #right>
                        <ConfirmButton class="sm-swipe-btn sm-swipe-btn--danger" icon="delete" :modal-type="ModalType.danger" @confirm="removeArticle(item)">
                            {{ $t("deleteItem", { title: item.title }) }}
                        </ConfirmButton>
                    </template>

                    <div class="sm-article-card__body" :class="{ 'is-done': !item.isActive }">
                        <button type="button" class="sm-article-card__toggle" @click.stop="toggleActive(item)">
                            <i class="bi" :class="item.isActive ? 'bi-circle' : 'bi-check-circle-fill'"></i>
                        </button>
                        <div class="sm-article-card__main">
                            <div class="sm-article-card__title text-truncate">{{ item.title }}</div>
                            <div class="sm-article-card__meta text-truncate">
                                <span v-if="item.quantity">{{ item.quantity }} {{ item.unit }}</span>
                                <span v-for="ac in item.categories" :key="ac.id ?? ac.categoryId" class="sm-article-card__cat">
                                    <i class="bi" :class="ac.category?.icon || 'bi-tag'"></i>{{ ac.category?.title }}
                                </span>
                            </div>
                        </div>
                    </div>
                </SwipeActions>
            </div>
        </LoadingContainer>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { useRouter } from "vue-router"
import { LoadingContainer, ConfirmButton, ModalType } from "@regira/modules/vue/ui"
import { useSearchView } from "@regira/modules/vue/entities"
import SwipeActions from "@/components/ui/SwipeActions.vue"
import useArticleStore from "@/entities/articles/data/store"
import ArticleEntity from "@/entities/articles/data/Entity"
import ArticleSearchObject from "@/entities/articles/filter/SearchObject"
import useCategoryStore from "@/entities/categories/data/store"
import CategoryEntity from "@/entities/categories/data/Entity"

const props = defineProps<{ shoppingListId: number }>()
const router = useRouter()

const { service } = useArticleStore()
const initialSearchObject = new ArticleSearchObject()
initialSearchObject.shoppingListId = props.shoppingListId

const { searchObject, items, isLoading, searchHandler, applySave, applyRemove } = useSearchView<ArticleEntity, ArticleSearchObject>({
    service,
    searchObject: initialSearchObject,
    defaultPageSize: 200,
})
function refresh() {
    return searchHandler(false)
}
// useSearchView does not fetch on its own — it only exposes searchHandler for a caller (normally
// useRouteOverview's route watcher) to invoke; a hand-rolled view outside the routed Overview needs its
// own initial fetch.
onMounted(refresh)
watch(
    () => props.shoppingListId,
    (id) => {
        searchObject.value.shoppingListId = id
        refresh()
    }
)

// Checked ("bought") items sink to the bottom, like most shopping-list apps; within each group, SortOrder.
const sortedItems = computed(() =>
    [...(items.value ?? [])].sort((a, b) => {
        if (a.isActive !== b.isActive) return a.isActive ? -1 : 1
        return a.sortOrder - b.sortOrder
    })
)

const { service: categoryService } = useCategoryStore()
const categories = ref<Array<CategoryEntity>>([])
onMounted(async () => {
    categories.value = await categoryService.list({ pageSize: 0 })
})

function toggleActiveFilter(value: boolean) {
    searchObject.value.isActive = searchObject.value.isActive === value ? undefined : value
    refresh()
}
function toggleCategoryFilter(id: number) {
    searchObject.value.categoryId = searchObject.value.categoryId === id ? undefined : id
    refresh()
}

const newTitle = ref("")
const adding = ref(false)
async function handleAdd() {
    const title = newTitle.value.trim()
    if (!title) return
    adding.value = true
    try {
        const nextSort = (items.value ?? []).reduce((max, x) => Math.max(max, x.sortOrder), -1) + 1
        const article = new ArticleEntity()
        article.title = title
        article.shoppingListId = props.shoppingListId
        article.isActive = true
        article.sortOrder = nextSort
        await applySave(article)
        newTitle.value = ""
        await refresh()
    } finally {
        adding.value = false
    }
}

async function toggleActive(item: ArticleEntity) {
    item.isActive = !item.isActive
    await applySave(item)
    await refresh()
}
async function removeArticle(item: ArticleEntity) {
    if (await applyRemove(item)) {
        await refresh()
    }
}
async function reorder(item: ArticleEntity, direction: -1 | 1) {
    const group = sortedItems.value.filter((x) => x.isActive === item.isActive)
    const idx = group.findIndex((x) => x.id === item.id)
    const swapWith = group[idx + direction]
    if (!swapWith) return
    const itemOrder = item.sortOrder
    item.sortOrder = swapWith.sortOrder
    swapWith.sortOrder = itemOrder
    await Promise.all([applySave(item), applySave(swapWith)])
    await refresh()
}
function goToArticle(item: ArticleEntity) {
    router.push({ name: "ArticleDetails", params: { id: item.id } })
}
</script>

<style scoped>
.sm-add-row {
    display: flex;
    gap: 0.5rem;
}
.sm-add-row .btn {
    flex: 0 0 auto;
}
.sm-article-card {
    overflow: hidden;
}
.sm-article-card__body {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    padding: 0.7rem 0.85rem;
    min-height: var(--sm-touch);
}
.sm-article-card__toggle {
    flex: 0 0 auto;
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    color: var(--sm-accent);
    border: none;
    background: none;
    padding: 0;
}
.sm-article-card__main {
    flex: 1 1 auto;
    min-width: 0;
}
.sm-article-card__title {
    font-weight: 600;
}
.sm-article-card__meta {
    font-size: 0.78rem;
    color: #7b8a80;
    display: flex;
    flex-wrap: wrap;
    gap: 0.4rem;
    align-items: center;
}
.sm-article-card__cat {
    background: var(--sm-surface-muted);
    border-radius: 999px;
    padding: 0.05rem 0.5rem;
}
.sm-article-card__body.is-done {
    opacity: 0.55;
}
.sm-article-card__body.is-done .sm-article-card__title {
    text-decoration: line-through;
}
</style>
