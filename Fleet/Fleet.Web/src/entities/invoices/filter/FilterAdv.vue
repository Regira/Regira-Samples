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

        <input v-model.lazy.trim="searchObject.q" class="form-control mb-2" :placeholder="$t('invoiceNumber')" @change="handleUpdate" />
        <div class="mb-2">
            <FormLabel :label="$t('supplier')" />
            <SupplierInputSelector
                v-model="filterSupplier"
                v-model:idValue="searchObject.supplierId as number"
                :canEdit="false"
                :placeholder="$t('supplier')"
                @select="handleUpdate"
            />
        </div>
        <div class="row g-2">
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('status')" />
                <select v-model="searchObject.status" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("all") }}</option>
                    <option v-for="s in InvoiceStatuses" :key="s" :value="s">{{ invoiceStatusBadge[s]!.label }}</option>
                </select>
            </div>
            <div class="col-md-6 mb-2 d-flex align-items-end">
                <NullableCheckBox v-model="searchObject.isOverdue" id="invoiceOverdue" :label="$t('overdue')" @update:modelValue="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('invoiceDateFrom')" />
                <input v-model="searchObject.minDate" type="date" class="form-control" @change="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('invoiceDateTo')" />
                <input v-model="searchObject.maxDate" type="date" class="form-control" @change="handleUpdate" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { FormLabel, IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import { InputSelector as SupplierInputSelector } from "@/entities/suppliers"
import type { Entity as Supplier } from "@/entities/suppliers"
import { InvoiceStatuses, invoiceStatusBadge } from "@/infrastructure/domain"
import SearchObject from "./SearchObject"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterSupplier = ref<Supplier>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterSupplier.value = undefined
}
</script>
