<script setup lang="ts">
import { onMounted, ref } from "vue"
import { useRoute } from "vue-router"
import { useConfig } from "@/app-config"
import { NavBar, NavSearch } from "@/components/entity-navigation"
import { useEntityStore as useCategoryStore, type Entity as Category } from "@/entities/categories"
import { formatLongDate } from "@/utilities/format"

defineProps<{ isPublic: boolean }>()

const { title } = useConfig()
const route = useRoute()
const open = ref(false)
const closeMenu = () => (open.value = false)
const today = formatLongDate(new Date())

// the masthead's section strip: every category with at least one live story
const { service: categoryService } = useCategoryStore()
const sections = ref<Array<Category>>([])
onMounted(async () => {
    try {
        sections.value = await categoryService.list({ hasPublishedPosts: true, pageSize: 0 })
    } catch (ex) {
        console.error(ex)
    }
})
</script>

<template>
    <!-- reader chrome: editorial masthead + section strip -->
    <div v-if="isPublic" class="masthead">
        <div class="container-xl">
            <div class="masthead__top">
                <span class="d-none d-sm-inline">{{ today }}</span>
                <router-link :to="{ name: 'home' }" class="masthead__manage"><i class="bi bi-pencil-square me-1"></i>{{ $t("manage") }}</router-link>
            </div>
            <router-link :to="{ name: 'blog' }" class="masthead__brand">{{ $tm(title) }}</router-link>
            <nav class="masthead__sections" :aria-label="$t('categories')">
                <!-- vue-router's active classes ignore the query, so the current section is derived from it -->
                <router-link :to="{ name: 'blog' }" active-class="" exact-active-class="" :class="{ active: route.name === 'blog' && !route.query.category }">
                    {{ $t("latest") }}
                </router-link>
                <router-link
                    v-for="c in sections"
                    :key="c.id"
                    :to="{ name: 'blog', query: { category: c.slug } }"
                    active-class=""
                    exact-active-class=""
                    :class="{ active: route.query.category === c.slug }"
                >
                    {{ c.title }}
                </router-link>
            </nav>
        </div>
    </div>

    <!-- management chrome: compact navbar built from the config map -->
    <nav v-else class="navbar navbar-expand-sm admin-navbar" v-click-outside="closeMenu">
        <div class="container-fluid">
            <router-link class="navbar-brand" :to="{ name: 'home' }">
                {{ $tm(title) }} <span class="badge text-bg-dark fw-normal ms-1">{{ $t("studio") }}</span>
            </router-link>
            <button class="navbar-toggler" type="button" @click.stop="open = !open"><span class="navbar-toggler-icon"></span></button>
            <div class="collapse navbar-collapse" :class="{ show: open }">
                <NavBar @select="closeMenu" />
                <div class="d-flex ms-auto align-items-center gap-2 flex-wrap py-2 py-sm-0">
                    <NavSearch @search="closeMenu" />
                    <router-link :to="{ name: 'blog' }" class="btn btn-outline-dark text-nowrap" @click="closeMenu">
                        <i class="bi bi-box-arrow-up-right me-1"></i>{{ $t("viewBlog") }}
                    </router-link>
                </div>
            </div>
        </div>
    </nav>
</template>
