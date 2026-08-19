<template>
    <SwipeActions class="sm-cat-card sm-card" :max-reveal="64" @tap="goToDetails">
        <template #right>
            <ConfirmButton class="sm-swipe-btn sm-swipe-btn--danger" icon="delete" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </template>

        <div class="sm-cat-card__body" :style="{ '--cat-color': item.colorHex || '#9aa5a0' }">
            <div class="sm-cat-card__icon">
                <i class="bi" :class="item.icon || 'bi-tag'"></i>
            </div>
            <div class="sm-cat-card__title text-truncate">{{ item.$title }}</div>
            <div v-if="item.articleCount" class="sm-cat-card__count">{{ item.articleCount }} {{ $t("items") }}</div>
        </div>
    </SwipeActions>
</template>

<script setup lang="ts">
import { useRouter } from "vue-router"
import { ConfirmButton, ModalType } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
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
function goToDetails() {
    router.push({ name: config.key + "Details", params: { id: item.value.$id } })
}
</script>

<style scoped>
.sm-cat-card {
    overflow: hidden;
}
.sm-cat-card__body {
    padding: 0.85rem 0.6rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.35rem;
    text-align: center;
    min-height: 96px;
    justify-content: center;
}
.sm-cat-card__icon {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: color-mix(in srgb, var(--cat-color) 20%, white);
    color: var(--cat-color);
    font-size: 1.15rem;
}
.sm-cat-card__title {
    font-weight: 600;
    font-size: 0.9rem;
    max-width: 100%;
}
.sm-cat-card__count {
    font-size: 0.75rem;
    color: #7b8a80;
}
</style>
