<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { RouterLink } from "vue-router"
import { LoadingContainer } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { useEntityStore as usePostStore, type Entity as BlogPost } from "@/entities/blog-posts"
import { useConfig } from "@/app-config"
import { renderMarkdown } from "@/utilities/markdown"
import { formatLongDate, initials } from "@/utilities/format"
import CategoryBadge from "./CategoryBadge.vue"
import PostCard from "./PostCard.vue"

// Public detail page: resolved by slug AND isPublished=true, so drafts and scheduled posts are never shown.
const props = defineProps<{ slug: string }>()

const { service: postService } = usePostStore()
const { translateMessage } = useLang()
const { title: siteTitle } = useConfig()

const post = ref<BlogPost>()
const related = ref<Array<BlogPost>>([])
const isLoading = ref(true)
const notFound = ref(false)
const html = computed(() => renderMarkdown(post.value?.content))
const wasUpdated = computed(() => {
    const p = post.value
    return !!(p?.lastModified && p.publishedAt && p.lastModified.getTime() - p.publishedAt.getTime() > 24 * 3600 * 1000)
})

async function load(slug: string) {
    isLoading.value = true
    notFound.value = false
    related.value = []
    try {
        const result = await postService.search({ slug, isPublished: true, includes: ["Tags"], pageSize: 1 })
        post.value = result.items[0]
        notFound.value = !post.value
        if (post.value) {
            document.title = `${post.value.title} | ${translateMessage(siteTitle)}`
            if (post.value.categoryId) {
                const more = await postService.search({ isPublished: true, categoryId: [post.value.categoryId], exclude: [post.value.id], pageSize: 3 })
                related.value = more.items
            }
        }
    } catch (ex) {
        console.error(ex)
        notFound.value = true
    } finally {
        isLoading.value = false
    }
}

watch(() => props.slug, load, { immediate: true })
</script>

<template>
    <LoadingContainer :is-loading="isLoading">
        <div v-if="notFound" class="text-center py-5">
            <p class="section-label">404</p>
            <h1 class="article-title mb-3">{{ $t("storyNotFound") }}</h1>
            <p class="text-muted mb-4">{{ $t("storyNotFoundHint") }}</p>
            <RouterLink :to="{ name: 'blog' }" class="btn btn-dark">{{ $t("backToStories") }}</RouterLink>
        </div>

        <article v-else-if="post" class="article">
            <header class="article-header">
                <nav class="article-crumbs mb-3" aria-label="breadcrumb">
                    <RouterLink :to="{ name: 'blog' }">{{ $t("stories") }}</RouterLink>
                    <span aria-hidden="true">/</span>
                    <CategoryBadge v-if="post.category" :category="post.category" />
                </nav>
                <h1 class="article-title">{{ post.title }}</h1>
                <p v-if="post.summary" class="article-dek">{{ post.summary }}</p>
                <div class="article-byline">
                    <span class="avatar avatar--lg" aria-hidden="true">{{ initials(post.authorName) }}</span>
                    <div>
                        <RouterLink v-if="post.authorName" :to="{ name: 'blog', query: { author: post.authorName } }" class="fw-semibold text-reset">
                            {{ post.authorName }}
                        </RouterLink>
                        <div class="text-muted small">
                            <time :datetime="post.publishedAt?.toISOString()">{{ formatLongDate(post.publishedAt) }}</time>
                            &middot; {{ $t("readingTimeMin", { min: post.readingTimeMinutes }) }}
                            <template v-if="wasUpdated"> &middot; {{ $t("updatedOn", { date: formatLongDate(post.lastModified) }) }}</template>
                        </div>
                    </div>
                </div>
            </header>

            <figure v-if="post.coverImageUrl" class="article-cover">
                <img :src="post.coverImageUrl" :alt="post.title" />
            </figure>

            <div class="article-body" v-html="html"></div>

            <footer class="article-footer">
                <ul v-if="post.tags?.length" class="article-tags">
                    <li v-for="t in post.tags" :key="t.tagId">
                        <RouterLink :to="{ name: 'blog', query: { tag: t.tag?.slug } }">#{{ t.tag?.title }}</RouterLink>
                    </li>
                </ul>
                <div class="d-flex justify-content-between align-items-center flex-wrap gap-2">
                    <RouterLink :to="{ name: 'blog' }" class="btn btn-outline-dark"><i class="bi bi-arrow-left me-1"></i>{{ $t("backToStories") }}</RouterLink>
                    <RouterLink :to="{ name: 'BlogPostDetails', params: { id: post.id } }" class="small text-muted">
                        <i class="bi bi-pencil me-1"></i>{{ $t("editStory") }}
                    </RouterLink>
                </div>
            </footer>
        </article>

        <section v-if="related.length" class="related mt-5 pt-4" aria-labelledby="related-heading">
            <h2 id="related-heading" class="section-label mb-3">{{ $t("moreFrom", { category: post?.category?.title ?? "" }) }}</h2>
            <div class="row row-cols-1 row-cols-md-3 g-4">
                <div v-for="p in related" :key="p.id" class="col">
                    <PostCard :post="p" :show-tags="false" />
                </div>
            </div>
        </section>
    </LoadingContainer>
</template>
