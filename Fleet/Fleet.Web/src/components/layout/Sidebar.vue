<script setup lang="ts">
import type { RouteLocationRaw, LocationQueryRaw } from "vue-router"
import { Icon } from "@regira/modules/vue/ui"
import { isNavItem, type INavItem } from "@regira/modules/vue/entities"
import { useNavigation } from "@/components/entity-navigation"
import { useConfig } from "@/app-config"

// The sidebar is built from config.json -> navigation + the collected $configs (useNavigation), so a new
// slice shows up here by adding its key to the navbar map - no hand-written link list.
defineEmits<{ (e: "select"): void }>()
const { navbarTree } = useNavigation()
const { title } = useConfig()
const to = (v: INavItem): RouteLocationRaw => ({ name: v.routeName, query: (v.initialQuery ?? {}) as LocationQueryRaw })
</script>

<template>
    <nav class="fleet-sidebar__nav" aria-label="Main">
        <router-link class="fleet-brand" :to="{ name: 'home' }" @click="$emit('select')">
            <span class="fleet-brand__logo"><i class="bi bi-truck-front-fill"></i></span>
            <span class="fleet-brand__name">{{ $tm(title) }}</span>
        </router-link>
        <ul class="nav flex-column">
            <li class="nav-item">
                <router-link class="nav-link" :to="{ name: 'home' }" exact-active-class="active" @click="$emit('select')">
                    <i class="bi bi-speedometer2"></i><span>{{ $t("dashboard") }}</span>
                </router-link>
            </li>
        </ul>
        <template v-if="navbarTree">
            <template v-for="node in navbarTree.roots" :key="node.value.id">
                <ul v-if="isNavItem(node.value)" class="nav flex-column">
                    <li class="nav-item">
                        <router-link class="nav-link" :to="to(node.value as INavItem)" active-class="active" @click="$emit('select')">
                            <Icon :name="node.value.icon ?? ''" /><span>{{ $t(node.value.title) }}</span>
                        </router-link>
                    </li>
                </ul>
                <div v-else class="fleet-sidebar__group">
                    <div class="fleet-sidebar__group-title">{{ $t(node.value.title) }}</div>
                    <ul class="nav flex-column">
                        <li v-for="child in node.children" :key="child.value.id" class="nav-item">
                            <router-link class="nav-link" :to="to(child.value as INavItem)" active-class="active" @click="$emit('select')">
                                <Icon :name="child.value.icon ?? ''" /><span>{{ $t(child.value.title) }}</span>
                            </router-link>
                        </li>
                    </ul>
                </div>
            </template>
        </template>
    </nav>
</template>
