<!-- Card presentation of the same search result (same props / emits as List.vue) -->
<template>
    <div class="row g-3 ah-cards">
        <div v-for="item in items" :key="item.$id" class="col-12 col-sm-6 col-lg-4 col-xxl-3">
            <div class="card h-100 ah-card" :style="{ '--ah-status-color': getAssetStatus(item.status)?.color || '#6c757d' }">
                <div class="card-body pb-2">
                    <div class="d-flex align-items-start gap-2 mb-2">
                        <div class="ah-card__icon"><i :class="getCategory(item.category)?.$iconClass || 'bi bi-box'"></i></div>
                        <div class="flex-grow-1 min-w-0">
                            <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="stretched-link text-decoration-none text-body">
                                <div class="fw-semibold text-truncate" :title="item.title">{{ item.title }}</div>
                            </RouterLink>
                            <div class="small text-muted text-truncate">
                                <span class="ah-code">{{ item.code }}</span>
                                <span v-if="item.serialNumber"> &middot; SN {{ item.serialNumber }}</span>
                            </div>
                        </div>
                    </div>
                    <StatusBadge :status="getAssetStatus(item.status)" small />
                    <dl class="ah-card__facts small mt-2 mb-0">
                        <dt><Icon name="user" /></dt>
                        <dd class="text-truncate">{{ getEmployee(item.currentEmployee)?.$title || "-" }}</dd>
                        <dt><Icon name="map" /></dt>
                        <dd class="text-truncate">{{ getLocation(item.location)?.$title || "-" }}</dd>
                        <dt><Icon name="calendar" /></dt>
                        <dd class="text-truncate">{{ fmtDate(item.purchaseDate) || "-" }} <span class="text-muted">{{ fmtMoney(item.purchasePrice) }}</span></dd>
                    </dl>
                </div>
                <div class="card-footer d-flex justify-content-between align-items-center py-1 small text-muted">
                    <span class="text-truncate">{{ getCategory(item.category)?.$title }}</span>
                    <ConfirmButton
                        v-if="!readonly"
                        icon="delete"
                        class="position-relative ah-card__action"
                        :modal-type="ModalType.danger"
                        :modal-title="$t('delete')"
                        :modal-labels="{ cancel: $t('cancel'), submit: $t('delete') }"
                        @confirm="$emit('request-remove', item)"
                    >
                        {{ $t("deleteItem", { title: item?.$title }) }}
                    </ConfirmButton>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { OverviewEmits } from "@regira/modules/vue/entities"
import StatusBadge from "@/components/StatusBadge.vue"
import { fmtDate, fmtMoney } from "@/utilities/format"
import config from "../config/config"
import type Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { useEntityStore as useCategoryStore } from "@/entities/categories"
import { useEntityStore as useAssetStatusStore } from "@/entities/asset-statuses"
import { useEntityStore as useLocationStore } from "@/entities/locations"
import { useEntityStore as useEmployeeStore } from "@/entities/employees"

interface Emits extends /* @vue-ignore */ OverviewEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = defineProps<{ modelValue?: Array<Entity>; readonly?: boolean }>()

const { fromPool } = useEntityStore()
const items = computed<Array<Entity>>({
    get: () => fromPool(props.modelValue || []),
    set: (value) => emit("update:modelValue", value),
})
const { fromPool: getCategory } = useCategoryStore()
const { fromPool: getAssetStatus } = useAssetStatusStore()
const { fromPool: getLocation } = useLocationStore()
const { fromPool: getEmployee } = useEmployeeStore()
</script>
