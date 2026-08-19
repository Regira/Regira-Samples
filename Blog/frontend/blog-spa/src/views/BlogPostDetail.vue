<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { RouterLink, useRoute } from "vue-router"
import { formatDate } from "@regira/modules/vue/formatters"
import useBlogPostStore from "@/entities/blog-posts/data/store"
import type { Entity as BlogPost } from "@/entities/blog-posts"

const props = defineProps<{ slug: string }>()
const route = useRoute()
const { service } = useBlogPostStore()

const post = ref<BlogPost>()
const notFound = ref(false)
const isLoading = ref(true)

const paragraphs = computed(() => (post.value?.content || "").split(/\n{2,}/).filter((p) => p.trim().length > 0))

async function load(slug: string) {
    isLoading.value = true
    notFound.value = false
    post.value = undefined
    try {
        const result = await service.search({ slug, isPublished: true, pageSize: 1, includes: "All" })
        post.value = result.items[0]
        if (!post.value) notFound.value = true
    } finally {
        isLoading.value = false
    }
}

onMounted(() => load(props.slug || (route.params.slug as string)))
watch(
    () => route.params.slug,
    (slug) => slug && load(slug as string)
)
</script>

<template>
    <article class="blog-detail">
        <div v-if="isLoading" class="text-center text-muted py-5">Loading article...</div>
        <div v-else-if="notFound" class="text-center py-5">
            <h2>Article not found</h2>
            <RouterLink :to="{ name: 'blogHome' }" class="btn btn-outline-primary mt-3">Back to overview</RouterLink>
        </div>
        <template v-else-if="post">
            <div class="blog-detail-cover" :style="{ backgroundImage: `url(${post.coverImageUrl})` }" />
            <div class="container blog-detail-body">
                <RouterLink :to="{ name: 'blogHome' }" class="blog-back-link">&larr; Back to all articles</RouterLink>
                <span v-if="post.category" class="article-card-category">{{ post.category.title }}</span>
                <h1 class="blog-detail-title">{{ post.title }}</h1>
                <div class="blog-detail-meta text-muted">
                    <span v-if="post.publishedAt">{{ formatDate(post.publishedAt) }}</span>
                </div>
                <p class="blog-detail-summary">{{ post.summary }}</p>
                <div class="blog-detail-content">
                    <p v-for="(para, i) in paragraphs" :key="i">{{ para }}</p>
                </div>
                <div v-if="post.tags?.length" class="blog-detail-tags">
                    <span v-for="t in post.tags" :key="t.id" class="chip">#{{ t.tag?.title ?? t.tagId }}</span>
                </div>
            </div>
        </template>
    </article>
</template>
