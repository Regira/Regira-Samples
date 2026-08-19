<!-- Owned-collection editor for Asset.MaintenanceRecords -- an editable table of scalar rows.
     Bind it in the parent Form.vue: <AssetMaintenanceRecordOverview v-model="item.maintenanceRecords" /> -->
<script setup lang="ts">
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { DateInput } from "@regira/modules/vue/ui"
import AssetMaintenanceRecord from "./Entity"

const props = defineProps<{ modelValue?: Array<AssetMaintenanceRecord>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<AssetMaintenanceRecord>] }>()
const { items, newItem, handleSave } = useOwnedCollection<AssetMaintenanceRecord>({
    props,
    emit,
    createRow: () => Object.assign(new AssetMaintenanceRecord(), { maintenanceDate: new Date() }),
})
</script>

<template>
    <div class="maintenance-editor">
        <div v-if="items.length" class="row g-2 mb-1 fw-bold small text-muted">
            <div class="col-2">{{ $t("assetMaintenanceRecord.maintenanceDate") }}</div>
            <div class="col-2">{{ $t("assetMaintenanceRecord.performedBy") }}</div>
            <div class="col">{{ $t("assetMaintenanceRecord.description") }}</div>
            <div class="col-1">{{ $t("assetMaintenanceRecord.cost") }}</div>
            <div class="col-2">{{ $t("assetMaintenanceRecord.nextDueDate") }}</div>
        </div>
        <div v-for="row in items" :key="row.id" class="row g-2 mb-1 align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col-2">
                <DateInput v-model="row.maintenanceDate" :readonly="readonly || row._deleted" />
            </div>
            <div class="col-2">
                <input v-model="row.performedBy" :readonly="readonly || row._deleted" class="form-control" :placeholder="$t('assetMaintenanceRecord.performedBy')" />
            </div>
            <div class="col">
                <input v-model="row.description" :readonly="readonly || row._deleted" class="form-control" :placeholder="$t('assetMaintenanceRecord.description')" />
            </div>
            <div class="col-1">
                <input type="number" step="0.01" v-model.number="row.cost" :readonly="readonly || row._deleted" class="form-control" />
            </div>
            <div class="col-2">
                <DateInput v-model="row.nextDueDate" :readonly="readonly || row._deleted" />
            </div>
            <div v-if="!readonly" class="col-auto">
                <button type="button" class="btn btn-outline-danger" :title="row._deleted ? 'Restore' : 'Remove'" @click="row._deleted = !row._deleted">
                    {{ row._deleted ? "↺" : "×" }}
                </button>
            </div>
        </div>
        <div v-if="newItem && !readonly" class="row g-2 mb-1 align-items-center">
            <div class="col-2"><DateInput v-model="newItem.maintenanceDate" /></div>
            <div class="col-2"><input v-model="newItem.performedBy" class="form-control" :placeholder="$t('assetMaintenanceRecord.performedBy')" /></div>
            <div class="col">
                <input
                    v-model="newItem.description"
                    class="form-control"
                    :placeholder="$t('assetMaintenanceRecord.description')"
                    @keyup.enter="handleSave({ saved: newItem, isNew: true })"
                />
            </div>
            <div class="col-1"><input type="number" step="0.01" v-model.number="newItem.cost" class="form-control" /></div>
            <div class="col-2"><DateInput v-model="newItem.nextDueDate" /></div>
            <div class="col-auto"><button type="button" class="btn btn-success" @click="handleSave({ saved: newItem, isNew: true })">+</button></div>
        </div>
    </div>
</template>
