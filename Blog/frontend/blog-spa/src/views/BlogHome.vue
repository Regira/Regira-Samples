<script setup lang="ts">
import { onMounted, reactive, ref, watch } from "vue"
import { RouterLink } from "vue-router"
import { formatDate } from "@regira/modules/vue/formatters"
import useBlogPostStore from "@/entities/blog-posts/data/store"
import useCategoryStore from "@/entities/categories/data/store"
import type { Entity as BlogPost } from "@/entities/blog-posts"
import type { Entity as Category } from "@/entities/categories"

const { service: postService } = useBlogPostStore()
const { service: categoryService } = useCategoryStore()

const posts = ref<Array<BlogPost>>([])
const count = ref(0)
const categories = ref<Array<Category>>([])
const isLoading = ref(true)

const state = reactive({
    q: "",
    categoryId: undefined as number | undefined,
    page: 1,
})
const pageSize = 9

async function load() {
    isLoading.value = true
    try {
        const result = await postService.search({
            isPublished: true,
            q: state.q || undefined,
            categoryId: state.categoryId,
            sortBy: ["PublishedAtDesc"],
            pageSize,
            page: state.page,
        })
        posts.value = result.items
        count.value = result.count
    } finally {
        isLoading.value = false
    }
}

onMounted(async () => {
    const catResult = await categoryService.search({ pageSize: 0 })
    categories.value = catResult.items
    await load()
})

watch(
    () => [state.q, state.categoryId, state.page],
    () => load()
)

function selectCategory(id?: number) {
    state.categoryId = id
    state.page = 1
}

const totalPages = () => Math.max(1, Math.ceil(count.value / pageSize))
</script>

<template>
    <div class="blog-home">
        <section class="blog-hero text-center">
            <h1 class="blog-hero-title">Regira Blog</h1>
            <p class="blog-hero-sub">Stories, guides and ideas on technology, travel, food and more.</p>
            <div class="blog-search mx-auto">
                <input v-model.lazy.trim="state.q" type="search" class="form-control form-control-lg" placeholder="Search articles..." />
            </div>
        </section>

        <section class="container blog-categories">
            <button type="button" class="chip" :class="{ active: !state.categoryId }" @click="selectCategory(undefined)">All</button>
            <button
                v-for="cat in categories"
                :key="cat.id"
                type="button"
                class="chip"
                :class="{ active: state.categoryId === cat.id }"
                @click="selectCategory(cat.id)"
            >
                {{ cat.title }}
            </button>
        </section>

        <section class="container">
            <div v-if="isLoading" class="text-center text-muted py-5">Loading articles...</div>
            <div v-else-if="!posts.length" class="text-center text-muted py-5">No articles found.</div>
            <div v-else class="article-grid">
                <RouterLink
                    v-for="post in posts"
                    :key="post.id"
                    :to="{ name: 'blogPostDetail', params: { slug: post.slug } }"
                    class="article-card"
                >
                    <div class="article-card-img" :style="{ backgroundImage: `url(${post.coverImageUrl})` }" />
                    <div class="article-card-body">
                        <span v-if="post.category" class="article-card-category">{{ post.category.title }}</span>
                        <h3 class="article-card-title">{{ post.title }}</h3>
                        <p class="article-card-summary">{{ post.summary }}</p>
                        <span class="article-card-date">{{ post.publishedAt ? formatDate(post.publishedAt) : "" }}</span>
                    </div>
                </RouterLink>
            </div>

            <nav v-if="totalPages() > 1" class="d-flex justify-content-center gap-2 my-4">
                <button
                    class="btn btn-outline-secondary btn-sm"
                    :disabled="state.page <= 1"
                    @click="state.page--"
                >
                    Previous
                </button>
                <span class="align-self-center text-muted small">Page {{ state.page }} / {{ totalPages() }}</span>
                <button
                    class="btn btn-outline-secondary btn-sm"
                    :disabled="state.page >= totalPages()"
                    @click="state.page++"
                >
                    Next
                </button>
            </nav>
        </section>
    </div>
</template>
