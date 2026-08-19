<template>
    <div class="row border-bottom py-2">
        <div class="col-auto">
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col text-truncate">
            <EmployeeButton :model-value="getEmployee(item.employee)" /> {{ getEmployee(item.employee)?.$title }}
        </div>
        <div class="col d-none d-md-block text-truncate">{{ item.year }}</div>
        <div class="col d-none d-lg-block text-truncate">
            <span class="badge" :class="statusBadgeClass(item.status)">{{ $t(item.status.toLowerCase()) }}</span>
        </div>
        <div class="col-2 d-none d-xl-block text-truncate">{{ item.totalCredits }}</div>

        <div class="col-auto">
            <ConfirmButton icon="delete" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
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
import Entity, { RequestStatuses, type RequestStatus } from "../data/Entity"
import FormModalButton from "../details/FormModalButton.vue"
import { FormModalButton as EmployeeButton, useEntityStore as useEmployeeStore } from "@/entities/employees"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getEmployee } = useEmployeeStore()

function statusBadgeClass(status: RequestStatus) {
    switch (status) {
        case RequestStatuses.Approved:
            return "bg-success"
        case RequestStatuses.Rejected:
            return "bg-danger"
        default:
            return "bg-warning text-dark"
    }
}
</script>
