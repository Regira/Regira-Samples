<template>
    <div class="row border-bottom py-2">
        <div class="col-auto">
            <!-- Row-edit affordance follows config.isComplex: a real entity (page) links to its Details route;
                 a very basic entity (modal) opens FormModalButton. Forward @remove either way so a delete from
                 inside the modal refreshes the pooled overview — without it the deleted row lingers until reload. -->
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" :readonly="readonly" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col text-truncate" :class="{ 'text-decoration-line-through text-muted': item.$isCancelled }">{{ item.$title }}<small class="text-muted ms-2 d-none d-sm-inline"><i class="bi bi-people"></i> {{ item.attendeeCount + 1 }}</small></div>
        <div class="col d-none d-md-block text-truncate">
            <EmployeeButton v-if="item.organizer" :model-value="getEmployee(item.organizer)" /> {{ getEmployee(item.organizer)?.$title }}
        </div>
        <div class="col-3 col-md-2 text-truncate small">
            <div>{{ formatDay(item.start) }}</div>
            <div class="text-muted">{{ formatTime(item.start) }}–{{ formatTime(item.end) }}</div>
        </div>
        <div class="col d-none d-lg-block text-truncate">
            <span v-for="r in item.rooms" :key="r.roomId" class="me-2 text-nowrap">
                <span class="room-dot me-1" :style="{ backgroundColor: getRoom(r.room)?.$color }"></span>{{ getRoom(r.room)?.$title }}
                <i v-if="r.approvalStatus && r.approvalStatus !== 'Approved'" :class="[statusIcon[r.approvalStatus], `text-${statusVariant[r.approvalStatus]}`]"></i>
            </span>
        </div>
        <div class="col-2 col-md-1 text-center"><StatusBadge :status="item.status" compact /></div>
        
        <div class="col-auto">
            <!-- readonly comes from List.vue; it is the hook for permission-gating (entities.patterns.md -> Permission-gated UI) -->
            <ConfirmButton
                v-if="!readonly"
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
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import { useEntityStore as useRoomStore, type Entity as Room } from "@/entities/rooms"
import StatusBadge from "@/components/planning/StatusBadge.vue"
import { formatDay, formatTime, statusIcon, statusVariant } from "@/utilities/planning"
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
const { fromPool: poolRoom } = useRoomStore()
const getRoom = (x?: Partial<Room>) => (x ? poolRoom(x as Room) : undefined)
</script>
