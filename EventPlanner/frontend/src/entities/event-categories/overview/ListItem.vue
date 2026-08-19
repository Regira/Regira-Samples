<template>
    <div class="category-chip d-inline-flex align-items-center gap-2 px-3 py-2 rounded-pill" :style="chipStyle">
        <Icon v-if="item.icon" :name="`bi bi-${item.icon}`" />
        <span class="fw-semibold">{{ item.$title }}</span>
        <FormModalButton v-model="item" class="btn btn-sm btn-link p-0 text-reset" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        <ConfirmButton icon="delete" :modal-type="ModalType.danger" class="btn btn-sm btn-link p-0 text-reset" @confirm="$emit('request-remove', item)">
            {{ $t("deleteItem", { title: item?.$title }) }}
        </ConfirmButton>
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
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
const chipStyle = computed(() => {
    const color = item.value.colorHex || "#6c757d"
    return { backgroundColor: `${color}22`, color, border: `1px solid ${color}55` }
})
</script>
