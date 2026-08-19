<!-- Owned-collection editor for Asset.Warranties -- an editable table of scalar rows.
     Bind it in the parent Form.vue: <AssetWarrantyOverview v-model="item.warranties" /> -->
<script setup lang="ts">
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { DateInput } from "@regira/modules/vue/ui"
import AssetWarranty from "./Entity"

const props = defineProps<{ modelValue?: Array<AssetWarranty>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<AssetWarranty>] }>()
const { items, newItem, handleSave } = useOwnedCollection<AssetWarranty>({ props, emit, createRow: () => new AssetWarranty() })
</script>

<template>
    <div class="warranties-editor">
        <div v-if="items.length" class="row g-2 mb-1 fw-bold small text-muted">
            <div class="col">{{ $t("assetWarranty.provider") }}</div>
            <div class="col-2">{{ $t("assetWarranty.warrantyNumber") }}</div>
            <div class="col-2">{{ $t("assetWarranty.startDate") }}</div>
            <div class="col-2">{{ $t("assetWarranty.endDate") }}</div>
            <div class="col-1">{{ $t("assetWarranty.cost") }}</div>
        </div>
        <div v-for="row in items" :key="row.id" class="row g-2 mb-1 align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col">
                <input v-model="row.provider" :readonly="readonly || row._deleted" class="form-control" :placeholder="$t('assetWarranty.provider')" />
            </div>
            <div class="col-2">
                <input v-model="row.warrantyNumber" :readonly="readonly || row._deleted" class="form-control" />
            </div>
            <div class="col-2">
                <DateInput v-model="row.startDate" :readonly="readonly || row._deleted" />
            </div>
            <div class="col-2">
                <DateInput v-model="row.endDate" :readonly="readonly || row._deleted" />
            </div>
            <div class="col-1">
                <input type="number" step="0.01" v-model.number="row.cost" :readonly="readonly || row._deleted" class="form-control" />
            </div>
            <div v-if="!readonly" class="col-auto">
                <button type="button" class="btn btn-outline-danger" :title="row._deleted ? 'Restore' : 'Remove'" @click="row._deleted = !row._deleted">
                    {{ row._deleted ? "↺" : "×" }}
                </button>
            </div>
        </div>
        <div v-if="newItem && !readonly" class="row g-2 mb-1 align-items-center">
            <div class="col">
                <input
                    v-model="newItem.provider"
                    class="form-control"
                    :placeholder="$t('assetWarranty.provider')"
                    @keyup.enter="handleSave({ saved: newItem, isNew: true })"
                />
            </div>
            <div class="col-2"><input v-model="newItem.warrantyNumber" class="form-control" /></div>
            <div class="col-2"><DateInput v-model="newItem.startDate" /></div>
            <div class="col-2"><DateInput v-model="newItem.endDate" /></div>
            <div class="col-1"><input type="number" step="0.01" v-model.number="newItem.cost" class="form-control" /></div>
            <div class="col-auto"><button type="button" class="btn btn-success" @click="handleSave({ saved: newItem, isNew: true })">+</button></div>
        </div>
    </div>
</template>
