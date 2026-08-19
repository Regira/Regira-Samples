<template>
    <div class="row border-bottom py-2 align-items-center">
        <div class="col-auto">
            <!-- Row-edit affordance follows config.isComplex: a real entity (page) links to its Details route;
                 a very basic entity (modal) opens FormModalButton. Forward @remove either way so a delete from
                 inside the modal refreshes the pooled overview — without it the deleted row lingers until reload. -->
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col text-truncate">{{ item.$title }}</div>
        <div class="col d-none d-md-block text-truncate">
            <EmployeeButton :model-value="getEmployee(item.organizer)" /> {{ getEmployee(item.organizer)?.$title }}
        </div>
        <div class="col d-none d-lg-block text-truncate">{{ roomNames }}</div>
        <div class="col-2 text-truncate">
            <div>{{ formatDate(item.startTime) }}</div>
            <small class="text-muted">{{ timeLabel(item.startTime) }} - {{ timeLabel(item.endTime) }}</small>
        </div>
        <div class="col-auto d-none d-sm-block">
            <span class="badge" :class="statusBadgeClass">{{ item.status }}</span>
        </div>

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
import Entity from "../data/Entity"
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

function timeLabel(d?: Date): string {
    return d ? d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }) : ""
}

const roomNames = computed(() =>
    (item.value.rooms ?? [])
        .map((r) => r.room?.title)
        .filter(Boolean)
        .join(", ")
)
const statusBadgeClass = computed(() => {
    switch (item.value.status) {
        case "Approved":
            return "text-bg-success"
        case "Pending":
            return "text-bg-warning"
        case "Rejected":
            return "text-bg-danger"
        default:
            return "text-bg-secondary"
    }
})
</script>
