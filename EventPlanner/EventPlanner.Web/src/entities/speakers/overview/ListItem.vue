<template>
    <div class="card ep-speaker-card h-100">
        <div class="card-body d-flex gap-3">
            <SpeakerAvatar :speaker="item" size="lg" />
            <div class="flex-grow-1 min-w-0">
                <h5 class="card-title mb-0 text-truncate">
                    <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="stretched-link text-reset text-decoration-none">
                        {{ item.$title }}
                    </RouterLink>
                </h5>
                <div class="small text-muted text-truncate">{{ item.jobTitle }}</div>
                <div class="small fw-semibold text-truncate ep-accent-text">{{ item.company }}</div>
                <div class="mt-2 d-flex flex-wrap gap-1">
                    <span v-for="topic in item.$topics" :key="topic" class="badge rounded-pill ep-topic">{{ topic }}</span>
                </div>
            </div>
        </div>
        <div v-if="!readonly" class="ep-card-actions">
            <ConfirmButton
                icon="delete"
                :modal-type="ModalType.danger"
                :modal-title="$t('delete')"
                :modal-labels="{ cancel: $t('cancel'), submit: $t('delete') }"
                @confirm="$emit('request-remove', item)"
            >
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </div>
    </div>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import SpeakerAvatar from "../details/SpeakerAvatar.vue"

defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
</script>
