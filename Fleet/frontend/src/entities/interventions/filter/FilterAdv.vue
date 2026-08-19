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

        <input v-model.lazy.trim="searchObject.q" class="form-control mb-2" :placeholder="$t('keywords')" @change="handleUpdate" />

        <div class="mb-2">
            <VehicleInputSelector v-model="filterVehicle" v-model:idValue="searchObject.vehicleId as number" :canEdit="false" :placeholder="$t('vehicle')" @select="handleUpdate" />
        </div>
        <div class="mb-2">
            <SupplierInputSelector v-model="filterSupplier" v-model:idValue="searchObject.supplierId as number" :canEdit="false" :placeholder="$t('supplier')" @select="handleUpdate" />
        </div>
        <div class="mb-2">
            <InvoiceInputSelector v-model="filterInvoice" v-model:idValue="searchObject.invoiceId as number" :canEdit="false" :placeholder="$t('invoice')" @select="handleUpdate" />
        </div>

        <FormLabel :label="$t('status')" class="mb-1" />
        <div class="d-flex flex-wrap gap-2 mb-2">
            <div class="form-check form-check-inline" v-for="s in statusOptions" :key="s">
                <input type="checkbox" class="form-check-input" :id="`iv-status-${s}`" :checked="searchObject.status?.includes(s)" @change="toggleStatus(s)" />
                <label class="form-check-label" :for="`iv-status-${s}`">{{ $t(`interventionStatus.${s}`) }}</label>
            </div>
        </div>

        <NullableCheckBox v-model="searchObject.hasInvoice" id="iv-has-invoice" :label="$t('hasInvoice')" @update:modelValue="handleUpdate" class="mb-2" />

        <div class="row">
            <div class="col-6 mb-2">
                <FormLabel :label="$t('minScheduledDate')" class="small" />
                <DateInput v-model="searchObject.minScheduledDate" @update:modelValue="handleUpdate" />
            </div>
            <div class="col-6 mb-2">
                <FormLabel :label="$t('maxScheduledDate')" class="small" />
                <DateInput v-model="searchObject.maxScheduledDate" @update:modelValue="handleUpdate" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { FormLabel, IconButton, NullableCheckBox, DateInput } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InterventionStatus } from "../data/Entity"
import { InputSelector as VehicleInputSelector, type Entity as Vehicle } from "@/entities/vehicles"
import { InputSelector as SupplierInputSelector, type Entity as Supplier } from "@/entities/suppliers"
import { InputSelector as InvoiceInputSelector, type Entity as Invoice } from "@/entities/invoices"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterVehicle = ref<Vehicle>()
const filterSupplier = ref<Supplier>()
const filterInvoice = ref<Invoice>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterVehicle.value = undefined
    filterSupplier.value = undefined
    filterInvoice.value = undefined
}

const statusOptions = Object.values(InterventionStatus)
function toggleStatus(s: InterventionStatus) {
    const current = searchObject.value.status ?? []
    searchObject.value.status = current.includes(s) ? current.filter((x) => x !== s) : [...current, s]
    handleUpdate()
}
</script>
