<template>
    <div class="speaker-card card h-100 shadow-sm">
        <div class="speaker-photo" :style="item.photoUrl ? { backgroundImage: `url(${item.photoUrl})` } : undefined">
            <span v-if="!item.photoUrl" class="speaker-initials">{{ initials }}</span>
        </div>
        <div class="card-body d-flex flex-column">
            <h3 class="h6 mb-0 text-truncate">{{ item.$title }}</h3>
            <p class="small text-muted text-truncate mb-2">
                <span v-if="item.jobTitle">{{ item.jobTitle }}</span>
                <span v-if="item.jobTitle && item.company"> · </span>
                <span v-if="item.company">{{ item.company }}</span>
            </p>
            <p class="small text-body-secondary flex-grow-1 speaker-bio">{{ item.description }}</p>
            <div class="d-flex justify-content-between align-items-center mt-2">
                <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-sm btn-link p-0">
                    <Icon name="edit" /> {{ $t("edit") }}
                </RouterLink>
                <FormModalButton v-else v-model="item" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
                <ConfirmButton icon="delete" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
                    {{ $t("deleteItem", { title: item?.$title }) }}
                </ConfirmButton>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import FormModalButton from "../details/FormModalButton.vue"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const initials = computed(() =>
    (item.value.$title || "?")
        .split(/\s+/)
        .map((p) => p[0])
        .slice(0, 2)
        .join("")
        .toUpperCase()
)
</script>

<style scoped>
.speaker-card {
    overflow: hidden;
    border: none;
    transition: transform 0.15s ease;
}
.speaker-card:hover {
    transform: translateY(-3px);
}
.speaker-photo {
    height: 140px;
    background-size: cover;
    background-position: center top;
    background-color: var(--rg-accent, #6d28d9);
    background-image: linear-gradient(135deg, var(--rg-accent, #6d28d9), #ec4899);
    display: flex;
    align-items: center;
    justify-content: center;
}
.speaker-initials {
    font-size: 2.5rem;
    font-weight: 700;
    color: #fff;
    opacity: 0.9;
}
.speaker-bio {
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
    overflow: hidden;
}
</style>
