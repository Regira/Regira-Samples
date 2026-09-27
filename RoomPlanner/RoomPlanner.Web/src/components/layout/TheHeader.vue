<script setup lang="ts">
import { ref } from "vue"
import { useConfig } from "@/app-config"
import { NavBar, NavSearch } from "@/components/entity-navigation"

const { title } = useConfig()
const open = ref(false)
const closeMenu = () => (open.value = false)
// the planning views (not entity slices) sit before the config-driven entity navbar
const views = [
    { name: "planner", icon: "bi bi-calendar3-range", title: "planner" },
    { name: "roomFinder", icon: "bi bi-search", title: "findARoom" },
    { name: "calendar", icon: "bi bi-calendar-week", title: "calendar" },
    { name: "approvals", icon: "bi bi-shield-check", title: "approvals" },
]
</script>
<template>
    <nav class="navbar navbar-expand-md" v-click-outside="closeMenu">
        <div class="container-fluid">
            <router-link class="navbar-brand fw-semibold" :to="{ name: 'home' }"><i class="bi bi-door-open text-primary me-1"></i>{{ $tm(title) }}</router-link>
            <button class="navbar-toggler" type="button" :aria-label="$t('menu')" @click.stop="open = !open"><span class="navbar-toggler-icon"></span></button>
            <div class="collapse navbar-collapse" :class="{ show: open }">
                <ul class="navbar-nav">
                    <li v-for="v in views" :key="v.name" class="nav-item">
                        <router-link class="nav-link" :to="{ name: v.name }" @click="closeMenu">
                            <i :class="v.icon"></i><span class="d-md-none d-xl-inline ms-1">{{ $t(v.title) }}</span>
                        </router-link>
                    </li>
                </ul>
                <span class="nav-divider d-none d-md-inline mx-2"></span>
                <NavBar @select="closeMenu" />
                <div class="d-flex ms-auto align-items-center gap-2">
                    <NavSearch @search="closeMenu" />
                </div>
            </div>
        </div>
    </nav>
</template>
<style scoped>
.nav-divider {
    border-left: 1px solid var(--bs-border-color);
    height: 1.5rem;
}
</style>
