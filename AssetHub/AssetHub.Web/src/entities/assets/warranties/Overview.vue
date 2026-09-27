<!-- Owned-collection editor for Asset.Warranties - an editable table of scalar rows.
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged
     rows so `Related()` deletes them by omission. New rows mint negative temp ids and insert with save(). -->
<script setup lang="ts">
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { Icon } from "@regira/modules/vue/ui"
import { fmtMoney, daysUntil } from "@/utilities/format"
import Warranty, { warrantyTypes } from "./Entity"

const props = defineProps<{ modelValue?: Array<Warranty>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<Warranty>] }>()
const { items, newItem, handleSave } = useOwnedCollection<Warranty>({ props, emit, createRow: () => new Warranty() })

function coverageState(row: Warranty): "active" | "expiring" | "expired" | "future" | undefined {
    const toEnd = daysUntil(row.endDate)
    const toStart = daysUntil(row.startDate)
    if (toEnd == null || toStart == null) return undefined
    if (toStart > 0) return "future"
    if (toEnd < 0) return "expired"
    return toEnd <= 60 ? "expiring" : "active"
}
const stateClass: Record<string, string> = {
    active: "text-bg-success",
    expiring: "text-bg-warning",
    expired: "text-bg-secondary",
    future: "text-bg-info",
}
function add() {
    if (!newItem.value?.provider || !newItem.value.startDate || !newItem.value.endDate) return
    handleSave({ saved: newItem.value, isNew: true })
}
</script>

<template>
    <div class="ah-owned">
        <div class="row g-2 fw-bold small text-muted border-bottom pb-1 d-none d-lg-flex">
            <div class="col-lg-2">{{ $t("type") }}</div>
            <div class="col-lg-2">{{ $t("provider") }}</div>
            <div class="col-lg-1">{{ $t("reference") }}</div>
            <div class="col-lg-3">{{ $t("period") }}</div>
            <div class="col-lg-1">{{ $t("cost") }}</div>
            <div class="col-lg">{{ $t("coverage") }}</div>
            <div class="col-lg-auto" style="width: 4.5rem"></div>
        </div>
        <div v-for="row in items" :key="row.id" class="row g-2 py-2 border-bottom align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col-6 col-lg-2">
                <select v-model="row.type" :disabled="readonly || row._deleted" class="form-select form-select-sm">
                    <option v-for="t in warrantyTypes" :key="t" :value="t">{{ $t("warranty" + t) }}</option>
                </select>
            </div>
            <div class="col-6 col-lg-2"><input v-model="row.provider" required maxlength="96" :readonly="readonly || row._deleted" class="form-control form-control-sm" :placeholder="$t('provider')" /></div>
            <div class="col-6 col-lg-1"><input v-model="row.reference" maxlength="64" :readonly="readonly || row._deleted" class="form-control form-control-sm" :placeholder="$t('reference')" /></div>
            <div class="col-12 col-lg-3">
                <div class="input-group input-group-sm">
                    <input v-model="row.startDate" type="date" required :readonly="readonly || row._deleted" class="form-control" />
                    <input v-model="row.endDate" type="date" required :readonly="readonly || row._deleted" class="form-control" />
                </div>
            </div>
            <div class="col-4 col-lg-1"><input v-model.number="row.cost" type="number" min="0" step="0.01" :readonly="readonly || row._deleted" class="form-control form-control-sm" :placeholder="$t('cost')" /></div>
            <div class="col col-lg">
                <div class="d-flex align-items-center gap-2">
                    <input v-model="row.coverage" maxlength="512" :readonly="readonly || row._deleted" class="form-control form-control-sm" :placeholder="$t('coverage')" />
                    <span v-if="coverageState(row)" class="badge" :class="stateClass[coverageState(row)!]">{{ $t("coverage_" + coverageState(row)) }}</span>
                </div>
            </div>
            <div v-if="!readonly" class="col-auto text-end" style="width: 4.5rem">
                <button type="button" class="btn btn-sm btn-outline-danger" :title="row._deleted ? $t('restore') : $t('delete')" @click="row._deleted = !row._deleted">
                    <Icon :name="row._deleted ? 'restore' : 'delete'" />
                </button>
            </div>
        </div>
        <p v-if="!items.length" class="italic-muted my-2">{{ $t("noWarranties") }}</p>

        <div v-if="newItem && !readonly" class="row g-2 py-2 align-items-center ah-owned__add">
            <div class="col-6 col-lg-2">
                <select v-model="newItem.type" class="form-select form-select-sm">
                    <option v-for="t in warrantyTypes" :key="t" :value="t">{{ $t("warranty" + t) }}</option>
                </select>
            </div>
            <div class="col-6 col-lg-2"><input v-model="newItem.provider" maxlength="96" class="form-control form-control-sm" :placeholder="$t('provider')" /></div>
            <div class="col-6 col-lg-1"><input v-model="newItem.reference" maxlength="64" class="form-control form-control-sm" :placeholder="$t('reference')" /></div>
            <div class="col-12 col-lg-3">
                <div class="input-group input-group-sm">
                    <input v-model="newItem.startDate" type="date" class="form-control" />
                    <input v-model="newItem.endDate" type="date" class="form-control" />
                </div>
            </div>
            <div class="col-4 col-lg-1"><input v-model.number="newItem.cost" type="number" min="0" step="0.01" class="form-control form-control-sm" :placeholder="$t('cost')" /></div>
            <div class="col col-lg"><input v-model="newItem.coverage" maxlength="512" class="form-control form-control-sm" :placeholder="$t('coverage')" @keyup.enter="add" /></div>
            <div class="col-auto text-end" style="width: 4.5rem">
                <button type="button" class="btn btn-sm btn-success" :title="$t('add')" :disabled="!newItem.provider || !newItem.startDate || !newItem.endDate" @click="add">
                    <Icon name="new" />
                </button>
            </div>
        </div>
        <div v-if="items.length" class="small text-muted mt-1">
            {{ $t("totalCost") }}: {{ fmtMoney(items.filter((x) => !x._deleted).reduce((s, x) => s + (x.cost || 0), 0)) }}
        </div>
    </div>
</template>
