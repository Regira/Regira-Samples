<template>
    <EventCard :event="item">
        <template #footer>
            <div v-if="!readonly" class="ep-card-actions">
                <RouterLink :to="{ name: config.key + 'Form', params: { id: item.$id } }" class="btn btn-link p-1" :title="$t('edit')">
                    <Icon name="edit" />
                </RouterLink>
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
        </template>
    </EventCard>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import EventCard from "../details/EventCard.vue"

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
