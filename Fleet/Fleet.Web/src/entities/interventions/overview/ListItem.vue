<template>
    <div class="row fleet-table__row align-items-center" :class="{ 'fleet-row--inactive': item.status === 'Cancelled' }">
        <div class="col-auto">
            <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
        </div>
        <div class="col-3 col-md-2 col-xl-1 text-truncate">
            <span :class="{ 'text-danger fw-semibold': overdue }" :title="overdue ? 'planned date has passed' : ''">{{ fmtDay(item.scheduledDate) }}</span>
            <small v-if="item.priority === 'Urgent' || item.priority === 'High'" class="d-block">
                <StatusBadge :value="item.priority" :map="priorityBadge" :compact="true" />
            </small>
        </div>
        <div class="col-4 col-md-3 col-xl-2 text-truncate">
            <VehicleButton v-if="item.vehicle" :model-value="getVehicle(item.vehicle)" />
            <span class="fleet-plate">{{ getVehicle(item.vehicle)?.licensePlate }}</span>
            <small class="d-block text-secondary text-truncate">{{ getVehicle(item.vehicle)?.make }} {{ getVehicle(item.vehicle)?.model }}</small>
        </div>
        <div class="col d-none d-md-block text-truncate">
            <span v-for="l in item.lines ?? []" :key="l.id" class="fleet-chip" :title="l.interventionType?.title">{{ l.interventionType?.title }}</span>
            <small v-if="item.description" class="d-block text-secondary text-truncate">{{ item.description }}</small>
        </div>
        <div class="col-2 d-none d-lg-block text-truncate">
            <SupplierButton v-if="item.supplier" :model-value="getSupplier(item.supplier)" /> {{ getSupplier(item.supplier)?.$title }}
        </div>
        <div class="col-2 col-lg-1 d-none d-sm-block text-end fleet-num">{{ fmtMoney(item.totalCost) }}</div>
        <div class="col-1 d-none d-xl-block text-truncate">
            <template v-if="item.invoice">
                <InvoiceButton :model-value="getInvoice(item.invoice)" /><small>{{ getInvoice(item.invoice)?.$title }}</small>
            </template>
            <small v-else-if="item.status === 'Completed'" class="text-warning-emphasis"><i class="bi bi-hourglass-split"></i> {{ $t("toInvoice") }}</small>
        </div>
        <div class="col-auto fleet-col-status"><StatusBadge :value="item.status" :map="interventionStatusBadge" :compact="true" /></div>
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
import { computed } from "vue"
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import { daysFromToday, fmtDay, fmtMoney, interventionStatusBadge, priorityBadge } from "@/infrastructure/domain"
import StatusBadge from "@/components/StatusBadge.vue"
import config from "../config/config"
import Entity from "../data/Entity"
import { FormModalButton as VehicleButton, useEntityStore as useVehicleStore } from "@/entities/vehicles"
import { FormModalButton as SupplierButton, useEntityStore as useSupplierStore } from "@/entities/suppliers"
import { FormModalButton as InvoiceButton, useEntityStore as useInvoiceStore } from "@/entities/invoices"

defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getVehicle } = useVehicleStore()
const { fromPool: getSupplier } = useSupplierStore()
const { fromPool: getInvoice } = useInvoiceStore()
const overdue = computed(() => item.value.status === "Planned" && (daysFromToday(item.value.scheduledDate) ?? 0) < 0)
</script>
