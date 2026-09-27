<template>
    <div class="adv-filter">
        <div class="row">
            <div class="col mb-2" v-if="resultCount != null">
                <span class="text-info">{{ resultCount }} {{ $t("results") }}</span>
                <small v-if="filterIsActive" class="ms-2 italic-muted">({{ $t("filtersAreApplied") }})</small>
            </div>
            <div class="col mb-2 text-end">
                <IconButton icon="clear" :showText="true" @click="handleReset" />
            </div>
        </div>

        <input v-model.lazy.trim="searchObject.q" class="form-control mb-2" :placeholder="$t('interventionKeywords')" @change="handleUpdate" />
        <div class="row g-2">
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('vehicle')" />
                <VehicleInputSelector
                    v-model="filterVehicle"
                    v-model:idValue="searchObject.vehicleId as number"
                    :canEdit="false"
                    :placeholder="$t('vehicle')"
                    @select="handleUpdate"
                />
            </div>
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('supplier')" />
                <SupplierInputSelector
                    v-model="filterSupplier"
                    v-model:idValue="searchObject.supplierId as number"
                    :canEdit="false"
                    :placeholder="$t('supplier')"
                    @select="handleUpdate"
                />
            </div>
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('interventionType')" />
                <InterventionTypeInputSelector
                    v-model="filterType"
                    v-model:idValue="searchObject.interventionTypeId as number"
                    :canEdit="false"
                    :placeholder="$t('interventionType')"
                    @select="handleUpdate"
                />
            </div>
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('invoice')" />
                <InvoiceInputSelector
                    v-model="filterInvoice"
                    v-model:idValue="searchObject.invoiceId as number"
                    :canEdit="false"
                    :placeholder="$t('invoice')"
                    @select="handleUpdate"
                />
            </div>
            <div class="col-6 col-md-3 mb-2">
                <FormLabel :label="$t('status')" />
                <select v-model="searchObject.status" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("all") }}</option>
                    <option v-for="s in InterventionStatuses" :key="s" :value="s">{{ interventionStatusBadge[s]!.label }}</option>
                </select>
            </div>
            <div class="col-6 col-md-3 mb-2">
                <FormLabel :label="$t('priority')" />
                <select v-model="searchObject.priority" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("all") }}</option>
                    <option v-for="p in Priorities" :key="p" :value="p">{{ p }}</option>
                </select>
            </div>
            <div class="col-6 col-md-3 mb-2">
                <FormLabel :label="$t('dateFrom')" />
                <input v-model="searchObject.minDate" type="date" class="form-control" @change="handleUpdate" />
            </div>
            <div class="col-6 col-md-3 mb-2">
                <FormLabel :label="$t('dateTo')" />
                <input v-model="searchObject.maxDate" type="date" class="form-control" @change="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('sortBy')" />
                <select v-model="searchObject.sortBy" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("date") }} (newest)</option>
                    <option value="ScheduledDate">{{ $t("date") }} (oldest)</option>
                    <option value="TotalCostDesc">{{ $t("cost") }} (highest)</option>
                    <option value="PriorityDesc">{{ $t("priority") }} (urgent first)</option>
                    <option value="Vehicle">{{ $t("licensePlate") }}</option>
                    <option value="Supplier">{{ $t("supplier") }}</option>
                </select>
            </div>
            <div class="col-md-6 mb-2 d-flex align-items-end">
                <NullableCheckBox v-model="searchObject.isInvoiced" id="interventionIsInvoiced" :label="$t('invoiced')" @update:modelValue="handleUpdate" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { FormLabel, IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import { InterventionStatuses, Priorities, interventionStatusBadge } from "@/infrastructure/domain"
import SearchObject from "./SearchObject"
import { InputSelector as VehicleInputSelector } from "@/entities/vehicles"
import type { Entity as Vehicle } from "@/entities/vehicles"
import { InputSelector as SupplierInputSelector } from "@/entities/suppliers"
import type { Entity as Supplier } from "@/entities/suppliers"
import { InputSelector as InvoiceInputSelector } from "@/entities/invoices"
import type { Entity as Invoice } from "@/entities/invoices"
import { InputSelector as InterventionTypeInputSelector } from "@/entities/intervention-types"
import type { Entity as InterventionType } from "@/entities/intervention-types"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterVehicle = ref<Vehicle>()
const filterSupplier = ref<Supplier>()
const filterInvoice = ref<Invoice>()
const filterType = ref<InterventionType>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterVehicle.value = undefined
    filterSupplier.value = undefined
    filterInvoice.value = undefined
    filterType.value = undefined
}
</script>
