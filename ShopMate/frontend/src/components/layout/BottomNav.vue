<script setup lang="ts">
import { useRoute } from "vue-router"

const route = useRoute()
const isActive = (prefix: string) => typeof route.name === "string" && route.name.startsWith(prefix)
</script>
<template>
    <nav class="sm-bottomnav">
        <router-link :to="{ name: 'ShoppingListOverview' }" class="sm-bottomnav__item" :class="{ 'is-active': isActive('ShoppingList') }">
            <i class="bi" :class="isActive('ShoppingList') ? 'bi-cart4-fill' : 'bi-cart4'"></i>
            <span>{{ $t("shoppingLists") }}</span>
        </router-link>
        <router-link :to="{ name: 'CategoryOverview' }" class="sm-bottomnav__item" :class="{ 'is-active': isActive('Category') }">
            <i class="bi" :class="isActive('Category') ? 'bi-tags-fill' : 'bi-tags'"></i>
            <span>{{ $t("categories") }}</span>
        </router-link>
    </nav>
</template>
