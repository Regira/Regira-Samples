<template>
    <div class="row fleet-table__row align-items-center">
        <div class="col-auto">
            <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
        </div>
        <div class="col-3 col-md-2 text-truncate fw-semibold">{{ item.invoiceNumber }}</div>
        <div class="col text-truncate">
            <SupplierButton v-if="item.supplier" :model-value="getSupplier(item.supplier)" /> {{ getSupplier(item.supplier)?.$title }}
        </div>
        <div class="col-2 d-none d-lg-block">{{ fmtDay(item.invoiceDate) }}</div>
        <div class="col-2 d-none d-md-block text-truncate"><DueDate :value="item.dueDate" :soon-days="7" :inactive="item.status === 'Paid'" /></div>
        <div class="col-3 col-md-2 text-end fleet-num fw-semibold">{{ fmtMoney(item.totalAmount) }}</div>
        <div class="col-auto fleet-col-status"><StatusBadge :value="item.status" :map="invoiceStatusBadge" :compact="true" /></div>
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
                <p class="small text-secondary mt-2 mb-0">{{ $t("deleteInvoiceHint") }}</p>
            </ConfirmButton>
        </div>
    </div>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import { fmtDay, fmtMoney, invoiceStatusBadge } from "@/infrastructure/domain"
import StatusBadge from "@/components/StatusBadge.vue"
import DueDate from "@/components/DueDate.vue"
import config from "../config/config"
import Entity from "../data/Entity"
import { FormModalButton as SupplierButton, useEntityStore as useSupplierStore } from "@/entities/suppliers"

defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getSupplier } = useSupplierStore()
</script>
