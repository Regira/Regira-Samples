<template>
    <div class="row border-bottom py-2 align-items-center">
        <div class="col-auto">
            <!-- Row-edit affordance follows config.isComplex: a real entity (page) links to its Details route;
                 a very basic entity (modal) opens FormModalButton. Forward @remove either way so a delete from
                 inside the modal refreshes the pooled overview — without it the deleted row lingers until reload. -->
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" :readonly="readonly" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col text-truncate">
            <EventItemButton v-if="item.event" :model-value="getEventItem(item.event)" />
            <span class="small text-muted me-1">{{ getEventItem(item.event)?.startDate }}</span>{{ getEventItem(item.event)?.$title }}
        </div>
        <div class="col d-none d-md-block text-truncate">
            <EmployeeButton v-if="item.user" :model-value="getEmployee(item.user)" /> {{ getEmployee(item.user)?.$title }}
        </div>
        <div class="col-auto col-md-2 text-truncate"><span class="badge ep-status" :class="`ep-status-${item.status}`">{{ $t(item.status) }}</span></div>
        <div class="col-2 d-none d-lg-block text-truncate small"><i class="bi bi-collection me-1 text-muted"></i>{{ item.sessions?.length ?? 0 }}</div>

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
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import FormModalButton from "../details/FormModalButton.vue"
import { FormModalButton as EventItemButton, useEntityStore as useEventItemStore } from "@/entities/events"
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
const { fromPool: getEventItem } = useEventItemStore()
const { fromPool: getEmployee } = useEmployeeStore()
</script>
