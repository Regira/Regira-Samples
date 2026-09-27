<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { useRoute, useRouter, type LocationQueryRaw } from "vue-router"
import { Paging, LoadingContainer, ButtonType } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { useEntityStore as usePostStore, type Entity as BlogPost } from "@/entities/blog-posts"
import { useEntityStore as useCategoryStore, type Entity as Category } from "@/entities/categories"
import { useEntityStore as useTagStore, type Entity as Tag } from "@/entities/tags"
import { useConfig } from "@/app-config"
import PostCard from "./PostCard.vue"

// Public overview: ONLY publicly visible posts (isPublished=true -> published and the date has passed).
const PAGE_SIZE = 12
const route = useRoute()
const router = useRouter()
const { tagline, title: siteTitle } = useConfig()
const { translateMessage } = useLang()

const { service: postService } = usePostStore()
const { service: categoryService } = useCategoryStore()
const { service: tagService } = useTagStore()

const categories = ref<Array<Category>>([])
const tags = ref<Array<Tag>>([])
const featured = ref<Array<BlogPost>>([])
const posts = ref<Array<BlogPost>>([])
const count = ref(0)
const isLoading = ref(false)
const loadError = ref(false)

// --- filter state lives in the route query (deep-linkable, back-button friendly)
const qs = (key: string) => {
    const v = route.query[key]
    return (Array.isArray(v) ? v[0] : v) || undefined
}
const filters = computed(() => ({
    q: qs("q"),
    category: qs("category"),
    tag: qs("tag"),
    year: qs("year"),
    author: qs("author"),
    sort: qs("sort") ?? "Newest",
}))
const page = computed(() => Math.max(1, Number(qs("page") ?? 1) || 1))
const pagingInfo = ref({ page: 1, pageSize: PAGE_SIZE })
const hasFilters = computed(() => !!(filters.value.q || filters.value.category || filters.value.tag || filters.value.year || filters.value.author))
const showFeatured = computed(() => !hasFilters.value && page.value === 1 && featured.value.length > 0)

const activeCategory = computed(() => categories.value.find((c) => c.slug === filters.value.category))
const activeTag = computed(() => tags.value.find((t) => t.slug === filters.value.tag))
const thisYear = new Date().getFullYear()
const years = [thisYear, thisYear - 1, thisYear - 2, thisYear - 3]
const sortOptions = [
    { value: "Newest", label: "sortNewest" },
    { value: "Oldest", label: "sortOldest" },
    { value: "Title", label: "sortTitle" },
    { value: "ReadingTimeDesc", label: "sortLongest" },
    { value: "ReadingTime", label: "sortShortest" },
]
const searchText = ref(filters.value.q ?? "")

function navigate(changes: Record<string, string | undefined>, resetPage = true) {
    const query: LocationQueryRaw = { ...route.query, ...changes }
    if (resetPage) delete query.page
    for (const key of Object.keys(query)) if (query[key] == null || query[key] === "") delete query[key]
    router.push({ name: "blog", query })
}
const clearAll = () => {
    searchText.value = ""
    router.push({ name: "blog" })
}

async function loadLookups() {
    const [cats, tagResult] = await Promise.all([
        categoryService.list({ hasPublishedPosts: true, pageSize: 0 }),
        tagService.search({ hasPublishedPosts: true, pageSize: 0 }),
    ])
    categories.value = cats
    tags.value = [...tagResult.items].sort((a, b) => (b.publishedPostCount ?? 0) - (a.publishedPostCount ?? 0))
}

async function loadFeatured() {
    const result = await postService.search({ isPublished: true, isFeatured: true, includes: ["Tags"], pageSize: 3, page: 1 })
    featured.value = result.items
}

async function loadPosts() {
    isLoading.value = true
    loadError.value = false
    try {
        const f = filters.value
        const result = await postService.search({
            isPublished: true,
            q: f.q,
            categorySlug: f.category,
            tagSlug: f.tag,
            year: f.year,
            author: f.author,
            sortBy: [f.sort],
            includes: ["Tags"],
            // the lead stories are shown above the grid - keep them out of it on the front page
            exclude: showFeatured.value ? featured.value.map((x) => x.id) : undefined,
            page: page.value,
            pageSize: PAGE_SIZE,
        })
        posts.value = result.items
        count.value = result.count ?? 0
        pagingInfo.value = { page: page.value, pageSize: PAGE_SIZE }
    } catch (ex) {
        console.error(ex)
        loadError.value = true
    } finally {
        isLoading.value = false
    }
}

function handlePageChange() {
    navigate({ page: String(pagingInfo.value.page) }, false)
}

loadLookups().catch((ex) => console.error(ex))
const ready = loadFeatured().catch((ex) => console.error(ex))
watch(
    () => route.query,
    async () => {
        if (route.name !== "blog") return
        document.title = translateMessage(siteTitle)
        searchText.value = filters.value.q ?? ""
        await ready
        await loadPosts()
    },
    { immediate: true }
)
</script>

<template>
    <div class="blog-home">
        <!-- lead stories -->
        <section v-if="showFeatured" class="lead-stories mb-5" aria-labelledby="lead-heading">
            <h2 id="lead-heading" class="section-label">{{ $t("featuredStories") }}</h2>
            <div class="row g-4">
                <div class="col-lg-8">
                    <PostCard :post="featured[0]!" variant="lead" />
                </div>
                <div class="col-lg-4 d-flex flex-column gap-4">
                    <PostCard v-for="p in featured.slice(1)" :key="p.id" :post="p" :show-tags="false" />
                </div>
            </div>
        </section>
        <p v-else-if="!hasFilters && page === 1" class="tagline lead mb-4">{{ $tm(tagline) }}</p>

        <!-- filters -->
        <section class="blog-filters mb-4" aria-label="Filters">
            <nav class="category-pills mb-3" :aria-label="$t('categories')">
                <button type="button" class="pill" :class="{ active: !filters.category }" @click="navigate({ category: undefined })">{{ $t("allTopics") }}</button>
                <button
                    v-for="c in categories"
                    :key="c.id"
                    type="button"
                    class="pill"
                    :class="{ active: filters.category === c.slug }"
                    :style="{ '--cat-color': c.color || '#6b7280' }"
                    @click="navigate({ category: filters.category === c.slug ? undefined : c.slug })"
                >
                    {{ c.title }} <span class="pill-count">{{ c.publishedPostCount }}</span>
                </button>
            </nav>
            <div class="row g-2 align-items-center">
                <div class="col-12 col-lg-5">
                    <form class="input-group" role="search" @submit.prevent="navigate({ q: searchText.trim() || undefined })">
                        <span class="input-group-text bg-transparent"><i class="bi bi-search"></i></span>
                        <input v-model="searchText" type="search" class="form-control" :placeholder="$t('searchStories')" :aria-label="$t('searchStories')" />
                        <button class="btn btn-dark" type="submit">{{ $t("search") }}</button>
                    </form>
                </div>
                <div class="col-6 col-md-4 col-lg-3">
                    <select class="form-select" :value="filters.tag ?? ''" :aria-label="$t('tag')" @change="navigate({ tag: ($event.target as HTMLSelectElement).value || undefined })">
                        <option value="">{{ $t("allTags") }}</option>
                        <option v-for="t in tags" :key="t.id" :value="t.slug">#{{ t.title }} ({{ t.publishedPostCount }})</option>
                    </select>
                </div>
                <div class="col-6 col-md-4 col-lg-2">
                    <select class="form-select" :value="filters.year ?? ''" :aria-label="$t('publishedIn')" @change="navigate({ year: ($event.target as HTMLSelectElement).value || undefined })">
                        <option value="">{{ $t("anyYear") }}</option>
                        <option v-for="y in years" :key="y" :value="String(y)">{{ y }}</option>
                    </select>
                </div>
                <div class="col-12 col-md-4 col-lg-2">
                    <select class="form-select" :value="filters.sort" :aria-label="$t('sortBy')" @change="navigate({ sort: ($event.target as HTMLSelectElement).value })">
                        <option v-for="s in sortOptions" :key="s.value" :value="s.value">{{ $t(s.label) }}</option>
                    </select>
                </div>
            </div>
            <div v-if="hasFilters" class="active-filters mt-3">
                <span class="text-muted me-1">{{ $t("showing") }}</span>
                <button v-if="filters.q" type="button" class="chip" @click="navigate({ q: undefined })">"{{ filters.q }}" <i class="bi bi-x"></i></button>
                <button v-if="filters.category" type="button" class="chip" @click="navigate({ category: undefined })">
                    {{ activeCategory?.title ?? filters.category }} <i class="bi bi-x"></i>
                </button>
                <button v-if="filters.tag" type="button" class="chip" @click="navigate({ tag: undefined })">#{{ activeTag?.title ?? filters.tag }} <i class="bi bi-x"></i></button>
                <button v-if="filters.year" type="button" class="chip" @click="navigate({ year: undefined })">{{ filters.year }} <i class="bi bi-x"></i></button>
                <button v-if="filters.author" type="button" class="chip" @click="navigate({ author: undefined })">{{ $t("by") }} {{ filters.author }} <i class="bi bi-x"></i></button>
                <button type="button" class="btn btn-link btn-sm" @click="clearAll">{{ $t("clearAll") }}</button>
            </div>
        </section>

        <!-- latest / filtered stories -->
        <section aria-labelledby="latest-heading">
            <div class="d-flex justify-content-between align-items-baseline mb-3">
                <h2 id="latest-heading" class="section-label mb-0">{{ hasFilters ? $t("resultsHeading") : $t("latestStories") }}</h2>
                <span class="text-muted small">{{ $t("storyCount", { count }) }}</span>
            </div>
            <LoadingContainer :is-loading="isLoading">
                <p v-if="loadError" class="alert alert-warning">{{ $t("loadFailed") }}</p>
                <div v-else-if="posts.length" class="row row-cols-1 row-cols-md-2 row-cols-xl-3 g-4">
                    <div v-for="p in posts" :key="p.id" class="col">
                        <PostCard :post="p" />
                    </div>
                </div>
                <div v-else-if="!isLoading" class="empty-state text-center py-5">
                    <p class="h5 mb-2">{{ $t("noStories") }}</p>
                    <button type="button" class="btn btn-outline-dark" @click="clearAll">{{ $t("clearAll") }}</button>
                </div>
            </LoadingContainer>
            <div class="d-flex justify-content-center mt-5">
                <Paging v-show="count > PAGE_SIZE" v-model="pagingInfo" :count="count" :button-type="ButtonType.button" @change="handlePageChange" />
            </div>
        </section>
    </div>
</template>
