<script setup lang="ts">
import { RouterLink } from "vue-router"
import type { Entity as BlogPost } from "@/entities/blog-posts"
import { formatShortDate, initials } from "@/utilities/format"
import CategoryBadge from "./CategoryBadge.vue"

withDefaults(defineProps<{ post: BlogPost; variant?: "card" | "lead" | "compact"; showTags?: boolean }>(), { variant: "card", showTags: true })
</script>

<template>
    <article class="post-card" :class="`post-card--${variant}`">
        <RouterLink v-if="variant !== 'compact'" :to="{ name: 'post', params: { slug: post.slug } }" class="post-card__media" tabindex="-1" aria-hidden="true">
            <img v-if="post.coverImageUrl" :src="post.coverImageUrl" alt="" loading="lazy" />
            <span v-else class="post-card__media-fallback" :style="{ backgroundColor: post.category?.color || '#6b7280' }"></span>
        </RouterLink>
        <div class="post-card__body">
            <CategoryBadge v-if="post.category" :category="post.category" class="mb-2" />
            <h2 class="post-card__title">
                <RouterLink :to="{ name: 'post', params: { slug: post.slug } }">{{ post.title }}</RouterLink>
            </h2>
            <p v-if="post.summary && variant !== 'compact'" class="post-card__summary">{{ post.summary }}</p>
            <div class="post-card__meta">
                <RouterLink v-if="post.authorName" :to="{ name: 'blog', query: { author: post.authorName } }" class="post-card__author">
                    <span class="avatar" aria-hidden="true">{{ initials(post.authorName) }}</span>
                    <span>{{ post.authorName }}</span>
                </RouterLink>
                <span class="dot" aria-hidden="true">&middot;</span>
                <time :datetime="post.publishedAt?.toISOString()">{{ formatShortDate(post.publishedAt) }}</time>
                <span class="dot" aria-hidden="true">&middot;</span>
                <span>{{ $t("readingTimeMin", { min: post.readingTimeMinutes }) }}</span>
            </div>
            <ul v-if="showTags && post.tags?.length" class="post-card__tags">
                <li v-for="t in post.tags.slice(0, 3)" :key="t.tagId">
                    <RouterLink :to="{ name: 'blog', query: { tag: t.tag?.slug } }">#{{ t.tag?.title }}</RouterLink>
                </li>
            </ul>
        </div>
    </article>
</template>
