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
        <input v-model.lazy.trim="searchObject.code" class="form-control mb-2" :placeholder="$t('code')" @change="handleUpdate" />

        <div class="mb-2">
            <SupplierInputSelector v-model="filterSupplier" v-model:idValue="searchObject.supplierId as number" :canEdit="false" :placeholder="$t('supplier')" @select="handleUpdate" />
        </div>

        <FormLabel :label="$t('status')" class="mb-1" />
        <div class="d-flex flex-wrap gap-2 mb-2">
            <div class="form-check form-check-inline" v-for="s in statusOptions" :key="s">
                <input type="checkbox" class="form-check-input" :id="`inv-status-${s}`" :checked="searchObject.status?.includes(s)" @change="toggleStatus(s)" />
                <label class="form-check-label" :for="`inv-status-${s}`">{{ $t(`invoiceStatus.${s}`) }}</label>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { FormLabel, IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InvoiceStatus } from "../data/Entity"
import { InputSelector as SupplierInputSelector, type Entity as Supplier } from "@/entities/suppliers"

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

const statusOptions = Object.values(InvoiceStatus)
function toggleStatus(s: InvoiceStatus) {
    const current = searchObject.value.status ?? []
    searchObject.value.status = current.includes(s) ? current.filter((x) => x !== s) : [...current, s]
    handleUpdate()
}
</script>
