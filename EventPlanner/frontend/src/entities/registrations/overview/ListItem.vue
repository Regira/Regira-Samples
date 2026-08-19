<template>
    <div class="row border-bottom py-2 align-items-center">
        <div class="col-auto">
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col text-truncate">{{ item.employee?.title }}</div>
        <div class="col d-none d-md-block text-truncate">{{ item.event?.title }}</div>
        <div class="col-2 d-none d-lg-block">
            <span class="badge rounded-pill" :class="statusBadgeClass">{{ $t(`registrationStatus.${item.status}`) }}</span>
        </div>
        <div class="col-2 d-none d-xl-block text-truncate">{{ formatDate(item.created) }}</div>

        <div class="col-auto">
            <ConfirmButton icon="delete" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import { formatDate } from "@regira/modules/vue/formatters"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity, { RegistrationStatus } from "../data/Entity"
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
const statusBadgeClass = computed(() => {
    switch (item.value.status) {
        case RegistrationStatus.Confirmed:
            return "text-bg-success"
        case RegistrationStatus.Attended:
            return "text-bg-primary"
        case RegistrationStatus.Cancelled:
            return "text-bg-danger"
        default:
            return "text-bg-secondary"
    }
})
</script>
