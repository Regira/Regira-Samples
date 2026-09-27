<script setup lang="ts">
import { onMounted, ref } from "vue"
import { useConfig } from "@/app-config"
import { Dashboard } from "@/components/entity-navigation"
import { useEntityStore as usePostStore } from "@/entities/blog-posts"

const { title } = useConfig()
const { service: postService } = usePostStore()

// headline numbers for the studio - counted server-side through /search (pageSize 1, count only)
const stats = ref<Array<{ key: string; label: string; count?: number; query: Record<string, string> }>>([
    { key: "Published", label: "statusPublished", query: { status: "Published" } },
    { key: "Scheduled", label: "statusScheduled", query: { status: "Scheduled" } },
    { key: "Draft", label: "statusDraft", query: { status: "Draft" } },
    { key: "Featured", label: "featured", query: { isFeatured: "true" } },
])
onMounted(async () => {
    await Promise.all(
        stats.value.map(async (s) => {
            const result = await postService.search({ ...s.query, pageSize: 1 })
            s.count = result.count
        })
    )
})
</script>
<template>
    <section class="studio py-3">
        <h1 class="studio__title mb-1">{{ $tm(title) }} <span class="text-muted fw-normal">{{ $t("studio") }}</span></h1>
        <p class="text-muted mb-4">{{ $t("studioIntro") }}</p>
        <div class="row row-cols-2 row-cols-md-4 g-3 mb-5">
            <div v-for="s in stats" :key="s.key" class="col">
                <router-link :to="{ name: 'BlogPostOverview', query: s.query }" class="stat-card card h-100 text-decoration-none">
                    <div class="card-body">
                        <div class="stat-card__count">{{ s.count ?? "-" }}</div>
                        <div class="text-muted small">{{ $t(s.label) }}</div>
                    </div>
                </router-link>
            </div>
        </div>
        <Dashboard />
    </section>
</template>
