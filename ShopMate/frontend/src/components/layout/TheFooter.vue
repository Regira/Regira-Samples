<!-- Bottom tab bar (thumb reach on phones). Built from config.json → navigation.navbar via useNavigation(). -->
<script setup lang="ts">
import type { RouteLocationRaw, LocationQueryRaw } from "vue-router"
import { Icon } from "@regira/modules/vue/ui"
import { isNavItem, type INavItem } from "@regira/modules/vue/entities"
import { useNavigation } from "@/components/entity-navigation"

const { navbarTree } = useNavigation()
const to = (v: INavItem): RouteLocationRaw => ({ name: v.routeName, query: (v.initialQuery ?? {}) as LocationQueryRaw })
</script>

<template>
    <nav class="sm-tabbar" aria-label="Main">
        <router-link :to="{ name: 'home' }" class="sm-tabbar__item" exact-active-class="active" active-class="">
            <i class="bi bi-house-heart"></i>
            <span>{{ $t("home") }}</span>
        </router-link>
        <template v-for="node in navbarTree?.roots ?? []" :key="node.value.id">
            <router-link v-if="isNavItem(node.value)" :to="to(node.value as INavItem)" class="sm-tabbar__item">
                <Icon :name="node.value.icon ?? ''" />
                <span>{{ $t(node.value.title) }}</span>
            </router-link>
        </template>
    </nav>
</template>
