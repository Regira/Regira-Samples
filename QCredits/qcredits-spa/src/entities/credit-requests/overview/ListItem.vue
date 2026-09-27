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

        <div class="col text-truncate">
            <span class="fw-semibold">{{ item.$title }}</span>
            <div class="small text-muted text-truncate">{{ item.year }}<span v-if="item.employee?.department"> &middot; {{ item.employee.department.title }}</span></div>
        </div>
        <div class="col d-none d-md-block text-truncate">
            <EmployeeButton v-if="item.employee" :model-value="getEmployee(item.employee)" /> {{ getEmployee(item.employee)?.$title }}
        </div>
        <div class="col-2 col-lg-1 text-end fw-semibold">{{ formatCredits(item.totalCredits) }}</div>
        <div class="col-2 d-none d-sm-block"><StatusBadge :status="item.status" /></div>
        <div class="col-2 d-none d-lg-block small text-muted">{{ item.submittedAt ? formatDate(new Date(item.submittedAt), "en-GB") : "" }}</div>

        <div class="col-auto">
            <!-- readonly comes from List.vue; it is the hook for permission-gating (entities.patterns.md -> Permission-gated UI) -->
            <ConfirmButton
                v-if="!readonly && item.isDraft"
                icon="delete"
                :modal-type="ModalType.danger"
                :modal-title="$t('delete')"
                :modal-labels="{ cancel: $t('cancel'), submit: $t('delete') }"
                @confirm="$emit('request-remove', item)"
            >
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
            <span v-else-if="!readonly" class="btn disabled invisible"><Icon name="delete" /></span>
        </div>
    </div>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import { formatDate } from "@regira/modules/vue/formatters"
import { StatusBadge } from "@/components/credits"
import { formatCredits } from "@/domain/enums"
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
</script>
