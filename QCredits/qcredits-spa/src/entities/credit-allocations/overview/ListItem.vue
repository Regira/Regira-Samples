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

        <div class="col-1 fw-semibold">{{ item.year }}</div>
        <div class="col text-truncate">
            <EmployeeButton v-if="item.employee" :model-value="getEmployee(item.employee)" /> {{ getEmployee(item.employee)?.$title }}
        </div>
        <div class="col-4 d-none d-md-block">
            <CreditBar :free="item.freeCredits" :used="item.usedCredits" :pending="item.pendingCredits" :min-balance="item.minBalance" compact />
            <div class="small text-muted">{{ formatCredits(item.usedCredits) }} / {{ formatCredits(item.freeCredits) }}<span v-if="item.pendingCredits"> &middot; {{ formatCredits(item.pendingCredits) }} {{ $t("pending") }}</span></div>
        </div>
        <div class="col-2 col-md-1 text-end fw-semibold" :class="item.remainingCredits < 0 ? 'text-danger' : item.remainingCredits < 3 ? 'text-warning' : 'text-success'">{{ formatCredits(item.remainingCredits) }}</div>
        <div class="col-1 d-none d-lg-block text-end text-muted">{{ item.carriedOver ? formatCredits(item.carriedOver) : "" }}</div>

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
import { CreditBar } from "@/components/credits"
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
