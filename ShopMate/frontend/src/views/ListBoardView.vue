<!--
    The shopping list "board": the screen a shopper uses in the store.
    Articles are loaded server-side per section (to buy / bought), filtered by keyword + category, and
    ordered by the list's own SortOrder (drag to reorder - only while no filter narrows the list).
-->
<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { RouterLink } from "vue-router"
import { get } from "@regira/modules/vue/ioc"
import { ConfirmButton, Feedback, LoadingContainer, ModalType, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useEntityStore as useShoppingListStore, type Entity as ShoppingList } from "@/entities/shopping-lists"
import { Entity as ArticleModel, useEntityStore as useArticleStore, type Entity as Article, type EntityService as ArticleService } from "@/entities/articles"
import { CategoryChipFilter, useCategoryTree } from "@/entities/categories"
import BoardArticleRow from "@/components/board/BoardArticleRow.vue"
import QuickAdd from "@/components/board/QuickAdd.vue"
import useDragSort from "@/components/board/useDragSort"

const props = defineProps<{ id: string }>()

const listStore = useShoppingListStore()
const articleStore = useArticleStore()
const articleService = get<ArticleService>(ArticleModel.name)!
const tree = useCategoryTree()
const feedback = useFeedback()

const list = ref<ShoppingList>()
const toBuy = ref<Array<Article>>([])
const bought = ref<Array<Article>>([])
const isLoading = ref(false)
const showBought = ref(true)

const q = ref("")
const categoryPick = ref<number>()
const categoryIds = ref<Array<number>>()
const isFiltered = computed(() => !!q.value || !!categoryIds.value?.length)

const total = computed(() => toBuy.value.length + bought.value.length)
const progress = computed(() => (total.value ? Math.round((bought.value.length / total.value) * 100) : 0))

let loadVersion = 0
async function load() {
    const version = ++loadVersion
    isLoading.value = true
    try {
        const so = {
            shoppingListId: Number(props.id),
            q: q.value || undefined,
            categoryId: categoryIds.value,
            sortBy: ["SortOrder"],
            includes: ["Categories"],
            pageSize: 0,
        }
        const [details, active, done] = await Promise.all([
            listStore.service.details(props.id),
            articleStore.service.search({ ...so, isActive: true }),
            articleStore.service.search({ ...so, isActive: false, sortBy: ["TitleDesc"] }),
        ])
        if (version !== loadVersion) return
        list.value = details ? listStore.fromPool(details) : undefined
        toBuy.value = articleStore.fromPool(active.items)
        bought.value = articleStore.fromPool(done.items).sort((a, b) => a.title.localeCompare(b.title))
    } catch (ex) {
        console.error(ex)
        feedback.fail("Loading the list failed", toFeedbackError(ex))
    } finally {
        if (version === loadVersion) isLoading.value = false
    }
}

let debounce: ReturnType<typeof setTimeout> | undefined
watch(q, () => {
    clearTimeout(debounce)
    debounce = setTimeout(load, 300)
})
watch(() => props.id, load)
onMounted(() => {
    tree.load()
    load()
})

function handleCategory(ids?: Array<number>) {
    categoryIds.value = ids
    load()
}
function clearFilters() {
    q.value = ""
    categoryPick.value = undefined
    categoryIds.value = undefined
    load()
}

// keep the list card's counters (overview/home) in step without refetching it
function syncCounters(deltaTotal: number, deltaActive: number) {
    if (!list.value) return
    list.value.articleCount = (list.value.articleCount ?? 0) + deltaTotal
    list.value.activeCount = (list.value.activeCount ?? 0) + deltaActive
}

async function toggle(article: Article) {
    const next = !article.isActive
    // optimistic: move first, roll back when the server refuses
    article.isActive = next
    moveBetweenSections(article)
    try {
        await articleService.setActive(article.id, next)
        syncCounters(0, next ? 1 : -1)
    } catch (ex) {
        console.error(ex)
        article.isActive = !next
        moveBetweenSections(article)
        feedback.fail("Updating the article failed", toFeedbackError(ex))
    }
}
function moveBetweenSections(article: Article) {
    toBuy.value = toBuy.value.filter((x) => x.id !== article.id)
    bought.value = bought.value.filter((x) => x.id !== article.id)
    if (article.isActive) toBuy.value = [...toBuy.value, article].sort((a, b) => a.sortOrder - b.sortOrder)
    else bought.value = [...bought.value, article].sort((a, b) => a.title.localeCompare(b.title))
}

async function remove(article: Article) {
    try {
        await articleStore.service.remove(article)
        toBuy.value = toBuy.value.filter((x) => x.id !== article.id)
        bought.value = bought.value.filter((x) => x.id !== article.id)
        syncCounters(-1, article.isActive ? -1 : 0)
        feedback.success(`"${article.title}" removed`)
    } catch (ex) {
        console.error(ex)
        feedback.fail("Removing the article failed", toFeedbackError(ex))
    }
}

async function handleAdded(article: Article) {
    toBuy.value = [...toBuy.value, articleStore.fromPool(article)]
    syncCounters(1, 1)
}

async function uncheckAll() {
    const items = [...bought.value]
    feedback.pending("Putting everything back on the list...")
    try {
        await Promise.all(items.map((x) => articleService.setActive(x.id, true)))
        feedback.success(`${items.length} articles back on the list`)
    } catch (ex) {
        console.error(ex)
        feedback.fail("Updating the articles failed", toFeedbackError(ex))
    }
    await load()
}
async function removeBought() {
    const items = [...bought.value]
    feedback.pending("Clearing the cart...")
    try {
        await Promise.all(items.map((x) => articleStore.service.remove(x)))
        feedback.success(`${items.length} articles removed`)
    } catch (ex) {
        console.error(ex)
        feedback.fail("Removing the articles failed", toFeedbackError(ex))
    }
    await load()
}

// drag & drop ordering of the "to buy" section
const canSort = computed(() => !isFiltered.value && toBuy.value.length > 1)
const drag = useDragSort(toBuy, async (moved) => {
    if (!moved || !list.value) return
    try {
        const result = await articleService.reorder(list.value.id, toBuy.value.map((x) => x.id))
        const order = new Map(result.map((x) => [x.id, x.sortOrder]))
        for (const article of [...toBuy.value, ...bought.value]) article.sortOrder = order.get(article.id) ?? article.sortOrder
    } catch (ex) {
        console.error(ex)
        feedback.fail("Saving the order failed", toFeedbackError(ex))
        await load()
    }
})
</script>

<template>
    <section class="sm-board">
        <LoadingContainer :is-loading="isLoading && !list">
            <template v-if="list">
                <!-- header -->
                <header class="sm-board__head" :style="{ '--sm-list-color': list.color || '#16a34a' }">
                    <div class="d-flex align-items-start gap-2">
                        <RouterLink :to="{ name: 'home' }" class="sm-icon-btn" :aria-label="$t('back')"><i class="bi bi-chevron-left"></i></RouterLink>
                        <div class="flex-grow-1 min-w-0">
                            <h1 class="sm-board__title text-truncate">
                                <i v-if="list.isPinned" class="bi bi-pin-angle-fill me-1 small"></i>{{ list.title }}
                            </h1>
                            <div class="sm-board__sub text-truncate">
                                <span v-if="list.shopper">{{ list.shopper.name }}</span>
                                <span v-if="list.description"> &middot; {{ list.description }}</span>
                            </div>
                        </div>
                        <RouterLink :to="{ name: 'ShoppingListDetails', params: { id: list.id } }" class="sm-icon-btn" :aria-label="$t('editList')">
                            <i class="bi bi-pencil"></i>
                        </RouterLink>
                    </div>
                    <div class="sm-progress mt-3" role="progressbar" :aria-valuenow="progress" aria-valuemin="0" aria-valuemax="100">
                        <div class="sm-progress__bar" :style="{ width: progress + '%' }"></div>
                    </div>
                    <div class="d-flex justify-content-between small mt-1 sm-board__sub">
                        <span>{{ $t("toBuyCount", { count: toBuy.length }) }}</span>
                        <span>{{ bought.length }} / {{ total }} {{ $t("inCart") }}<template v-if="isFiltered"> ({{ $t("filtered") }})</template></span>
                    </div>
                </header>

                <!-- search + category filter -->
                <div class="sm-board__filters">
                    <div class="sm-search">
                        <i class="bi bi-search"></i>
                        <input v-model.trim="q" type="search" class="form-control" :placeholder="$t('searchInList')" enterkeyhint="search" />
                        <button v-if="isFiltered" type="button" class="btn btn-link btn-sm" @click="clearFilters">{{ $t("clear") }}</button>
                    </div>
                    <CategoryChipFilter v-model="categoryPick" :all-label="$t('all')" @change="handleCategory" />
                </div>

                <Feedback :feedback="feedback" />

                <!-- to buy -->
                <div class="sm-section-title">
                    <span>{{ $t("toBuy") }}</span>
                    <small v-if="canSort" class="text-muted"><i class="bi bi-grip-vertical"></i> {{ $t("dragToSort") }}</small>
                </div>
                <div class="sm-card-list" :class="{ 'is-busy': isLoading }">
                    <BoardArticleRow
                        v-for="(article, index) in toBuy"
                        :key="article.id"
                        :article="article"
                        :sortable="canSort"
                        :dragging="drag.dragIndex.value === index"
                        :offset-y="drag.offsetY.value"
                        @toggle="toggle"
                        @remove="remove"
                        @changed="load"
                        @grip-down="drag.start($event, index)"
                        @grip-move="drag.move"
                        @grip-up="drag.end"
                        @move="(delta) => drag.moveBy(index, delta)"
                    />
                    <div v-if="!toBuy.length" class="sm-empty">
                        <i class="bi" :class="isFiltered ? 'bi-funnel' : 'bi-emoji-smile'"></i>
                        <span>{{ isFiltered ? $t("noMatches") : $t("allDone") }}</span>
                    </div>
                </div>

                <!-- bought -->
                <template v-if="bought.length">
                    <div class="sm-section-title">
                        <button type="button" class="btn btn-link p-0 text-reset text-decoration-none fw-semibold" @click="showBought = !showBought">
                            <i class="bi me-1" :class="showBought ? 'bi-chevron-down' : 'bi-chevron-right'"></i>{{ $t("inCart") }} ({{ bought.length }})
                        </button>
                        <span class="d-flex gap-1">
                            <button type="button" class="btn btn-sm btn-outline-secondary" @click="uncheckAll">
                                <i class="bi bi-arrow-counterclockwise"></i><span class="ms-1">{{ $t("uncheckAll") }}</span>
                            </button>
                            <ConfirmButton
                                class="btn btn-sm btn-outline-danger"
                                icon="delete"
                                :modal-type="ModalType.danger"
                                :modal-title="$t('clearCart')"
                                :modal-labels="{ cancel: $t('cancel'), submit: $t('delete') }"
                                @confirm="removeBought"
                            >
                                {{ $t("clearCartConfirm", { count: bought.length }) }}
                            </ConfirmButton>
                        </span>
                    </div>
                    <div v-show="showBought" class="sm-card-list">
                        <BoardArticleRow v-for="article in bought" :key="article.id" :article="article" @toggle="toggle" @remove="remove" @changed="load" />
                    </div>
                </template>

                <div class="sm-quick-add-spacer"></div>
                <QuickAdd :shopping-list-id="list.id" @added="handleAdded" />
            </template>
            <div v-else-if="!isLoading" class="sm-empty mt-5">
                <i class="bi bi-question-circle"></i><span>{{ $t("listNotFound") }}</span>
                <RouterLink :to="{ name: 'home' }" class="btn btn-primary mt-3">{{ $t("backHome") }}</RouterLink>
            </div>
        </LoadingContainer>
    </section>
</template>
