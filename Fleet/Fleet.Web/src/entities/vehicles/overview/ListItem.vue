<template>
    <div class="row fleet-table__row align-items-center" :class="{ 'fleet-row--inactive': item.status === 'Retired' }">
        <div class="col-auto">
            <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
        </div>
        <div class="col-3 col-md-2 text-truncate"><span class="fleet-plate">{{ item.licensePlate }}</span></div>
        <div class="col text-truncate">
            <i :class="vehicleTypeIcon[item.vehicleType]" class="text-secondary me-1" :title="item.vehicleType"></i>
            <span class="fw-semibold">{{ item.make }} {{ item.model }}</span>
            <small class="text-secondary ms-1 d-none d-md-inline">{{ item.year }} &middot; {{ fuelLabel[item.fuelType] ?? item.fuelType }}</small>
        </div>
        <div class="col-2 d-none d-lg-block text-truncate">
            {{ item.department }}
            <small v-if="item.assignedDriver" class="d-block text-secondary text-truncate">{{ item.assignedDriver }}</small>
        </div>
        <div class="col-2 col-lg-1 d-none d-md-block text-end fleet-num">{{ fmtKm(item.mileage) }}</div>
        <div class="col-2 d-none d-sm-block text-truncate"><DueDate :value="item.nextServiceDate" :inactive="item.status === 'Retired'" /></div>
        <div class="col-auto fleet-col-status"><StatusBadge :value="item.status" :map="vehicleStatusBadge" :compact="true" /></div>
        <div class="col-auto">
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
import { fmtKm, fuelLabel, vehicleStatusBadge, vehicleTypeIcon } from "@/infrastructure/domain"
import StatusBadge from "@/components/StatusBadge.vue"
import DueDate from "@/components/DueDate.vue"
import config from "../config/config"
import Entity from "../data/Entity"

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
