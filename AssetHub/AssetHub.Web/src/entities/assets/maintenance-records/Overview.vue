<!-- Owned-collection editor for Asset.MaintenanceRecords - an editable table of scalar rows.
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged rows. -->
<script setup lang="ts">
import { computed } from "vue"
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { Icon } from "@regira/modules/vue/ui"
import { fmtMoney, daysUntil, isoDay } from "@/utilities/format"
import MaintenanceRecord, { maintenanceTypes } from "./Entity"

const props = defineProps<{ modelValue?: Array<MaintenanceRecord>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<MaintenanceRecord>] }>()
const { items, newItem, handleSave } = useOwnedCollection<MaintenanceRecord>({
    props,
    emit,
    createRow: () => MaintenanceRecord.create({ date: isoDay() }),
})

// newest first for display; the underlying array keeps its order
const sorted = computed(() => [...items.value].sort((a, b) => (b.date || "").localeCompare(a.date || "")))
function dueClass(row: MaintenanceRecord) {
    const d = daysUntil(row.nextDueDate)
    if (d == null) return ""
    return d < 0 ? "text-danger fw-semibold" : d <= 30 ? "text-warning-emphasis fw-semibold" : ""
}
function add() {
    if (!newItem.value?.title || !newItem.value.date) return
    handleSave({ saved: newItem.value, isNew: true })
}
</script>

<template>
    <div class="ah-owned">
        <div class="row g-2 fw-bold small text-muted border-bottom pb-1 d-none d-lg-flex">
            <div class="col-lg-2">{{ $t("date") }}</div>
            <div class="col-lg-2">{{ $t("type") }}</div>
            <div class="col-lg-3">{{ $t("title") }}</div>
            <div class="col-lg-2">{{ $t("performedBy") }}</div>
            <div class="col-lg-1">{{ $t("cost") }}</div>
            <div class="col-lg">{{ $t("nextDue") }}</div>
            <div class="col-lg-auto" style="width: 4.5rem"></div>
        </div>
        <div v-for="row in sorted" :key="row.id" class="py-2 border-bottom" :class="{ 'is-deleted': row._deleted }">
            <div class="row g-2 align-items-center">
                <div class="col-6 col-lg-2"><input v-model="row.date" type="date" required :readonly="readonly || row._deleted" class="form-control form-control-sm" /></div>
                <div class="col-6 col-lg-2">
                    <select v-model="row.type" :disabled="readonly || row._deleted" class="form-select form-select-sm">
                        <option v-for="t in maintenanceTypes" :key="t" :value="t">{{ $t("maintenance" + t) }}</option>
                    </select>
                </div>
                <div class="col-12 col-lg-3"><input v-model="row.title" required maxlength="128" :readonly="readonly || row._deleted" class="form-control form-control-sm" :placeholder="$t('title')" /></div>
                <div class="col-6 col-lg-2"><input v-model="row.performedBy" maxlength="96" :readonly="readonly || row._deleted" class="form-control form-control-sm" :placeholder="$t('performedBy')" /></div>
                <div class="col-6 col-lg-1"><input v-model.number="row.cost" type="number" min="0" step="0.01" :readonly="readonly || row._deleted" class="form-control form-control-sm" :placeholder="$t('cost')" /></div>
                <div class="col col-lg"><input v-model="row.nextDueDate" type="date" :readonly="readonly || row._deleted" class="form-control form-control-sm" :class="dueClass(row)" /></div>
                <div v-if="!readonly" class="col-auto text-end" style="width: 4.5rem">
                    <button type="button" class="btn btn-sm btn-outline-danger" :title="row._deleted ? $t('restore') : $t('delete')" @click="row._deleted = !row._deleted">
                        <Icon :name="row._deleted ? 'restore' : 'delete'" />
                    </button>
                </div>
            </div>
            <input v-model="row.description" maxlength="1024" :readonly="readonly || row._deleted" class="form-control form-control-sm mt-1 ah-owned__note" :placeholder="$t('description')" />
        </div>
        <p v-if="!items.length" class="italic-muted my-2">{{ $t("noMaintenance") }}</p>

        <div v-if="newItem && !readonly" class="row g-2 py-2 align-items-center ah-owned__add">
            <div class="col-6 col-lg-2"><input v-model="newItem.date" type="date" class="form-control form-control-sm" /></div>
            <div class="col-6 col-lg-2">
                <select v-model="newItem.type" class="form-select form-select-sm">
                    <option v-for="t in maintenanceTypes" :key="t" :value="t">{{ $t("maintenance" + t) }}</option>
                </select>
            </div>
            <div class="col-12 col-lg-3"><input v-model="newItem.title" maxlength="128" class="form-control form-control-sm" :placeholder="$t('title')" @keyup.enter="add" /></div>
            <div class="col-6 col-lg-2"><input v-model="newItem.performedBy" maxlength="96" class="form-control form-control-sm" :placeholder="$t('performedBy')" /></div>
            <div class="col-6 col-lg-1"><input v-model.number="newItem.cost" type="number" min="0" step="0.01" class="form-control form-control-sm" :placeholder="$t('cost')" /></div>
            <div class="col col-lg"><input v-model="newItem.nextDueDate" type="date" class="form-control form-control-sm" :title="$t('nextDue')" /></div>
            <div class="col-auto text-end" style="width: 4.5rem">
                <button type="button" class="btn btn-sm btn-success" :title="$t('add')" :disabled="!newItem.title || !newItem.date" @click="add">
                    <Icon name="new" />
                </button>
            </div>
        </div>
        <div v-if="items.length" class="small text-muted mt-1">
            {{ $t("totalCost") }}: {{ fmtMoney(items.filter((x) => !x._deleted).reduce((s, x) => s + (x.cost || 0), 0)) }}
        </div>
    </div>
</template>
