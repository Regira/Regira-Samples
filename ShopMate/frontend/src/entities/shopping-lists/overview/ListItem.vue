<template>
    <SwipeActions class="sm-list-card sm-card" @tap="goToDetails">
        <template #left>
            <button v-if="item.isArchived" type="button" class="sm-swipe-btn sm-swipe-btn--accent" @click="handleRestore">
                <i class="bi bi-arrow-counterclockwise"></i>
            </button>
        </template>
        <template #right>
            <ConfirmButton
                class="sm-swipe-btn sm-swipe-btn--danger"
                icon="delete"
                :modal-type="ModalType.danger"
                @confirm="$emit('request-remove', item)"
            >
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </template>

        <div class="sm-list-card__body" :style="{ '--list-color': item.colorHex || '#16a34a' }">
            <div class="sm-list-card__icon">
                <i class="bi" :class="item.icon || 'bi-cart4'"></i>
            </div>
            <div class="sm-list-card__main">
                <div class="sm-list-card__title text-truncate">
                    {{ item.$title }}
                    <span v-if="item.isArchived" class="badge text-bg-secondary ms-1">{{ $t("archived") }}</span>
                </div>
                <div class="sm-list-card__meta text-truncate">
                    <span v-if="item.ownerName"><i class="bi bi-person"></i> {{ item.ownerName }}</span>
                </div>
            </div>
            <div class="sm-list-card__badge" v-if="item.articleCount">
                <strong>{{ item.activeArticleCount ?? 0 }}</strong
                ><span class="text-muted">/{{ item.articleCount }}</span>
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
async function handleRestore() {
    item.value.isArchived = false
    const result = await service.save(item.value)
    emit("save", result)
}
</script>

<style scoped>
.sm-list-card {
    overflow: hidden;
}
.sm-list-card__body {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.9rem 1rem;
    min-height: 72px;
}
.sm-list-card__icon {
    flex: 0 0 auto;
    width: 46px;
    height: 46px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: color-mix(in srgb, var(--list-color) 18%, white);
    color: var(--list-color);
    font-size: 1.3rem;
}
.sm-list-card__main {
    flex: 1 1 auto;
    min-width: 0;
}
.sm-list-card__title {
    font-weight: 700;
    font-size: 1.02rem;
}
.sm-list-card__meta {
    font-size: 0.8rem;
    color: #7b8a80;
}
.sm-list-card__badge {
    flex: 0 0 auto;
    font-size: 0.95rem;
    padding: 0.25rem 0.6rem;
    border-radius: 999px;
    background: var(--sm-surface-muted);
}
</style>
