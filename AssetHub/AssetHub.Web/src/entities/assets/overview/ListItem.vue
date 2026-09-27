<template>
    <div class="row border-bottom ah-table__row align-items-center">
        <div class="col-auto">
            <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1" :title="$t('open')">
                <Icon :name="readonly ? 'details' : 'edit'" />
            </RouterLink>
        </div>
        <div class="col-3 col-md-2 col-xl-1 text-truncate"><span class="ah-code">{{ item.code }}</span></div>
        <div class="col text-truncate">
            <i :class="getCategory(item.category)?.$iconClass || 'bi bi-box'" class="me-1 text-secondary"></i>
            <span class="fw-semibold">{{ item.title }}</span>
            <span v-if="item.serialNumber" class="small text-muted ms-2 d-none d-sm-inline">SN {{ item.serialNumber }}</span>
        </div>
        <div class="col d-none d-md-block text-truncate">
            <CategoryButton v-if="item.category" :model-value="getCategory(item.category)" class="p-0 me-1" />{{ getCategory(item.category)?.$title }}
        </div>
        <div class="col-3 col-lg-2 text-truncate">
            <StatusBadge :status="getAssetStatus(item.status)" small />
        </div>
        <div class="col d-none d-lg-block text-truncate">
            <template v-if="item.currentEmployee">
                <EmployeeButton :model-value="getEmployee(item.currentEmployee)" class="p-0 me-1" />{{ getEmployee(item.currentEmployee)?.$title }}
            </template>
            <span v-else class="text-muted">-</span>
        </div>
        <div class="col d-none d-xl-block text-truncate">
            <template v-if="item.location">
                <LocationButton :model-value="getLocation(item.location)" class="p-0 me-1" />{{ getLocation(item.location)?.$title }}
            </template>
        </div>
        <div class="col-1 d-none d-xxl-block text-truncate text-end">{{ fmtMoney(item.purchasePrice) }}</div>
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
import StatusBadge from "@/components/StatusBadge.vue"
import { fmtMoney } from "@/utilities/format"
import config from "../config/config"
import Entity from "../data/Entity"
import { FormModalButton as CategoryButton, useEntityStore as useCategoryStore } from "@/entities/categories"
import { useEntityStore as useAssetStatusStore } from "@/entities/asset-statuses"
import { FormModalButton as LocationButton, useEntityStore as useLocationStore } from "@/entities/locations"
import { FormModalButton as EmployeeButton, useEntityStore as useEmployeeStore } from "@/entities/employees"

defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getCategory } = useCategoryStore()
const { fromPool: getAssetStatus } = useAssetStatusStore()
const { fromPool: getLocation } = useLocationStore()
const { fromPool: getEmployee } = useEmployeeStore()
</script>
