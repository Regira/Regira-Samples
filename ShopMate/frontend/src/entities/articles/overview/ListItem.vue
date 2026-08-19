<template>
    <SwipeActions class="sm-article-card sm-card" @tap="goToDetails">
        <template #right>
            <ConfirmButton class="sm-swipe-btn sm-swipe-btn--danger" icon="delete" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </template>

        <div class="sm-article-card__body" :class="{ 'is-done': !item.isActive }">
            <button type="button" class="sm-article-card__toggle" @click.stop="handleToggle">
                <i class="bi" :class="item.isActive ? 'bi-circle' : 'bi-check-circle-fill'"></i>
            </button>
            <div class="sm-article-card__main">
                <div class="sm-article-card__title text-truncate">{{ item.$title }}</div>
                <div class="sm-article-card__meta text-truncate">
                    <span v-if="item.quantity">{{ item.quantity }} {{ item.unit }}</span>
                    <span v-if="item.shoppingList"><i class="bi bi-cart4"></i> {{ item.shoppingList.title }}</span>
                    <span v-for="ac in item.categories" :key="ac.id ?? ac.categoryId" class="sm-article-card__cat">
                        <i class="bi" :class="ac.category?.icon || 'bi-tag'"></i>{{ ac.category?.title }}
                    </span>
                </div>
            </div>
        </div>
    </SwipeActions>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router"
import { ConfirmButton, ModalType } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import SwipeActions from "@/components/ui/SwipeActions.vue"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const router = useRouter()
const { service } = useEntityStore()

function goToDetails() {
    router.push({ name: config.key + "Details", params: { id: item.value.$id } })
}
async function handleToggle() {
    item.value.isActive = !item.value.isActive
    const result = await service.save(item.value)
    emit("save", result)
}
</script>

<style scoped>
.sm-article-card {
    overflow: hidden;
}
.sm-article-card__body {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    padding: 0.7rem 0.85rem;
    min-height: var(--sm-touch);
}
.sm-article-card__toggle {
    flex: 0 0 auto;
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    color: var(--sm-accent);
    border: none;
    background: none;
    padding: 0;
}
.sm-article-card__main {
    flex: 1 1 auto;
    min-width: 0;
}
.sm-article-card__title {
    font-weight: 600;
}
.sm-article-card__meta {
    font-size: 0.78rem;
    color: #7b8a80;
    display: flex;
    flex-wrap: wrap;
    gap: 0.4rem;
    align-items: center;
}
.sm-article-card__cat {
    background: var(--sm-surface-muted);
    border-radius: 999px;
    padding: 0.05rem 0.5rem;
}
.sm-article-card__body.is-done {
    opacity: 0.55;
}
.sm-article-card__body.is-done .sm-article-card__title {
    text-decoration: line-through;
}
</style>
