<template>
    <form @submit.prevent="handleSubmit" class="entity-form">
        <div class="row form-toolbar align-items-center mb-3">
            <div class="col col-md-auto order-1">
                <FormButtonsRow
                    :item="item"
                    :readonly="readonly"
                    :feedback="feedback"
                    :show-delete="item?.id > 0"
                    :labels="{ save: $t('save'), cancel: $t('cancel'), delete: $t('delete'), restore: $t('restore') }"
                    :modal-title="$t('delete')"
                    @cancel="handleCancel"
                    @remove="handleRemove"
                    @restore="handleRestore"
                >
                    <template #delete>{{ $t("deleteItem", { title: item?.$title }) }}</template>
                </FormButtonsRow>
            </div>
            <div class="col-auto order-2 order-md-3">
                <RouterLink
                    v-if="isPopup"
                    :to="{ name: `${config.key}Details`, params: { id: item.$id } }"
                    target="_blank"
                    class="btn btn-outline-secondary"
                    :title="$t('popOut')"
                >
                    <Icon name="popOut" />
                </RouterLink>
                <RouterLink v-else-if="overviewUrl" :to="overviewUrl" class="btn btn-outline-info">
                    <Icon name="list" /> <span class="d-none d-md-inline ms-1">{{ $t("overview") }}</span>
                </RouterLink>
            </div>
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <div v-if="item.id" class="fleet-entity-header mb-3">
            <div>
                <div class="fs-5 fw-semibold">{{ $t("intervention") }} #{{ item.id }}</div>
                <div class="text-secondary small">{{ item.vehicle?.licensePlate }} &middot; {{ item.supplier?.title }}</div>
            </div>
            <div class="ms-auto d-flex gap-2 align-items-center">
                <StatusBadge :value="item.priority" :map="priorityBadge" />
                <StatusBadge :value="item.status" :map="interventionStatusBadge" />
            </div>
        </div>

        <div class="row g-3">
            <div class="col-xl-6">
                <FormSection :title="$t('workOrder')" :readonly="readonly" class="mb-3">
                    <div class="row g-3">
                        <div class="col-12">
                            <FormLabel :label="$t('vehicle')" />
                            <VehicleInputSelector v-model="item.vehicle" v-model:idValue="item.vehicleId" :readonly="readonly" :filter-defaults="{ status: ['Active', 'InMaintenance', 'OutOfService'] }" />
                        </div>
                        <div class="col-12">
                            <FormLabel :label="$t('supplier')" />
                            <SupplierInputSelector v-model="item.supplier" v-model:idValue="item.supplierId" :readonly="readonly || !!item.invoiceId" :filter-defaults="{ isActive: true }" />
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('status')" />
                            <select v-model="item.status" :disabled="readonly" class="form-select">
                                <option v-for="s in InterventionStatuses" :key="s" :value="s">{{ interventionStatusBadge[s]!.label }}</option>
                            </select>
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('priority')" />
                            <select v-model="item.priority" :disabled="readonly" class="form-select">
                                <option v-for="p in Priorities" :key="p" :value="p">{{ p }}</option>
                            </select>
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('scheduledDate')" />
                            <input v-model="item.scheduledDate" type="date" :readonly="readonly" class="form-control" required />
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('completedDate')" />
                            <input v-model="item.completedDate" type="date" :readonly="readonly || item.status !== 'Completed'" class="form-control" />
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('mileage')" />
                            <div class="input-group">
                                <input v-model.number="item.mileage" type="number" min="0" :readonly="readonly" class="form-control" />
                                <span class="input-group-text">km</span>
                            </div>
                            <small v-if="vehicleMileage" class="text-secondary">{{ $t("currentOdometer") }}: {{ fmtKm(vehicleMileage) }}</small>
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('invoice')" />
                            <InvoiceInputSelector
                                v-model="item.invoice"
                                v-model:idValue="item.invoiceId"
                                :readonly="readonly || item.status !== 'Completed' || !item.supplierId"
                                :can-edit="false"
                                :filter-defaults="{ supplierId: item.supplierId }"
                                :placeholder="item.status === 'Completed' ? $t('notInvoiced') : $t('onlyCompletedInvoiced')"
                            />
                        </div>
                        <div class="col-12">
                            <FormLabel :label="$t('description')" />
                            <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="3" maxlength="512"></textarea>
                        </div>
                    </div>
                </FormSection>
            </div>
            <div class="col-xl-6">
                <FormSection :title="$t('interventionTypesAndCosts')" :readonly="readonly" class="mb-3">
                    <InterventionLineOverview v-model="item.lines" :readonly="readonly" :supplier-type-ids="supplierTypeIds" />
                </FormSection>
            </div>
        </div>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed, onMounted } from "vue"
import { RouterLink, useRoute, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { InputSelector as VehicleInputSelector, useEntityStore as useVehicleStore } from "@/entities/vehicles"
import { InputSelector as SupplierInputSelector, useEntityStore as useSupplierStore } from "@/entities/suppliers"
import { InputSelector as InvoiceInputSelector } from "@/entities/invoices"
import { InterventionStatuses, Priorities, fmtKm, interventionStatusBadge, priorityBadge } from "@/infrastructure/domain"
import StatusBadge from "@/components/StatusBadge.vue"
import { InterventionLineOverview } from "../intervention-lines"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const { fromPool: getVehicle } = useVehicleStore()
const { fromPool: getSupplier } = useSupplierStore()
const vehicleMileage = computed(() => getVehicle(item.value.vehicle)?.mileage)
// capabilities are known when the pooled supplier carries them (supplier lists always include them)
const supplierTypeIds = computed(() => getSupplier(item.value.supplier)?.interventionTypes?.map((x) => x.interventionTypeId))

// "Plan intervention" from a vehicle page pre-selects that vehicle
const route = useRoute()
onMounted(() => {
    if (!item.value.id && route.query.vehicleId && !item.value.vehicleId) item.value.vehicleId = Number(route.query.vehicleId)
})
</script>
