<!-- A shopping list as a big tappable card: opens the board; the pencil opens the list's own form. -->
<script setup lang="ts">
import { RouterLink } from "vue-router"
import type Entity from "../data/Entity"

defineProps<{ item: Entity; showShopper?: boolean }>()
</script>

<template>
    <div class="sm-list-card" :style="{ '--sm-list-color': item.color || '#16a34a' }">
        <RouterLink :to="{ name: 'ShoppingListBoard', params: { id: item.id } }" class="sm-list-card__main">
            <span class="sm-list-card__title text-truncate">
                <i v-if="item.isPinned" class="bi bi-pin-angle-fill me-1 small"></i>{{ item.title }}
            </span>
            <span class="sm-list-card__sub text-truncate">
                <template v-if="showShopper && item.shopper">{{ item.shopper.name }} &middot; </template>
                <template v-if="(item.activeCount ?? 0) > 0">{{ $t("toBuyCount", { count: item.activeCount }) }}</template>
                <template v-else-if="item.articleCount">{{ $t("allDone") }}</template>
                <template v-else>{{ $t("emptyList") }}</template>
            </span>
            <span class="sm-progress sm-progress--thin mt-2">
                <span class="sm-progress__bar" :style="{ width: item.$progress + '%' }"></span>
            </span>
        </RouterLink>
        <span class="sm-list-card__count" :title="$t('toBuy')">{{ item.activeCount ?? 0 }}</span>
        <slot name="actions">
            <RouterLink :to="{ name: 'ShoppingListDetails', params: { id: item.id } }" class="sm-icon-btn" :aria-label="$t('editList')">
                <i class="bi bi-three-dots-vertical"></i>
            </RouterLink>
        </slot>
    </div>
</template>
