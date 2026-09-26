<template>
    <div class="row border-bottom py-2 align-items-center" :class="{ 'opacity-50': !item.isActive }">
        <div class="col-auto">
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" :readonly="readonly" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col text-truncate" :title="item.text">{{ item.text }}</div>
        <div class="col-2 d-none d-md-block text-truncate">{{ $t("part" + item.part) }}</div>
        <div class="col-2 d-none d-lg-block text-truncate">{{ $t("strategy" + item.strategy) }}</div>
        <div class="col-1 d-none d-sm-block text-center">
            <Icon v-if="item.isActive" name="bi bi-check-circle-fill" class="text-success" />
            <Icon v-else name="bi bi-pause-circle" class="text-muted" />
        </div>

        <div class="col-auto">
            <ConfirmButton v-if="!readonly" icon="delete" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </div>
    </div>
</template>

<script setup lang="ts">
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
</script>
