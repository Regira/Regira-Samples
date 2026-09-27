<!-- Owned-collection editor for Intervention.Lines: which intervention types are performed, at what cost.
     A row carries a relation (InterventionType) AND scalars (cost, remarks) -> a table with an InputSelector
     in the relation column. Removal marks `_deleted` (undoable until save); the parent's
     EntityService.prepareItem drops flagged rows so Related() deletes them by omission. -->
<script setup lang="ts">
import { computed } from "vue"
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { InputSelector as InterventionTypeSelector, type Entity as InterventionType } from "@/entities/intervention-types"
import { fmtMoney } from "@/infrastructure/domain"
import InterventionLine from "./Entity"

const props = defineProps<{ modelValue?: Array<InterventionLine>; readonly?: boolean; supplierTypeIds?: Array<number> }>()
const emit = defineEmits<{ "update:modelValue": [Array<InterventionLine>] }>()
const { items, newItem, handleSave } = useOwnedCollection<InterventionLine>({ props, emit, createRow: () => new InterventionLine() })

const usedTypeIds = computed(() => items.value.filter((x) => !x._deleted).map((x) => x.interventionTypeId!))
const total = computed(() => items.value.filter((x) => !x._deleted).reduce((sum, x) => sum + (Number(x.cost) || 0), 0))
const warn = (row: InterventionLine) => !!props.supplierTypeIds && !!row.interventionTypeId && !props.supplierTypeIds.includes(row.interventionTypeId)

function handleTypeSelect(row: InterventionLine, type?: InterventionType) {
    // pre-fill the cost with the type's indicative price
    if (type && !row.cost) row.cost = type.standardCost
}
function addRow() {
    if (!newItem.value?.interventionTypeId) return
    handleSave({ saved: newItem.value, isNew: true })
}
</script>

<template>
    <div class="lines-editor">
        <div class="row g-2 small text-secondary fw-semibold mb-1 d-none d-md-flex">
            <div class="col-md-6">{{ $t("interventionType") }}</div>
            <div class="col-md-2">{{ $t("cost") }}</div>
            <div class="col">{{ $t("remarks") }}</div>
            <div class="col-auto" style="width: 3rem"></div>
        </div>
        <div v-for="row in items" :key="row.id" class="row g-2 mb-2 align-items-center fleet-line" :class="{ 'is-deleted': row._deleted }">
            <div class="col-12 col-md-6">
                <InterventionTypeSelector
                    v-model="row.interventionType"
                    v-model:idValue="row.interventionTypeId"
                    :readonly="readonly || row._deleted"
                    :can-edit="false"
                    :filter-defaults="{ isActive: true, exclude: usedTypeIds.filter((id) => id !== row.interventionTypeId) }"
                    @select="(t?: InterventionType) => handleTypeSelect(row, t)"
                />
                <small v-if="warn(row)" class="text-danger"><i class="bi bi-exclamation-triangle me-1"></i>{{ $t("supplierCannotPerform") }}</small>
            </div>
            <div class="col-5 col-md-2">
                <div class="input-group">
                    <span class="input-group-text"><i class="bi bi-currency-euro"></i></span>
                    <input v-model.number="row.cost" type="number" min="0" step="0.01" :readonly="readonly || row._deleted" class="form-control" />
                </div>
            </div>
            <div class="col">
                <input v-model="row.remarks" :readonly="readonly || row._deleted" class="form-control" maxlength="256" :placeholder="$t('remarks')" />
            </div>
            <div v-if="!readonly" class="col-auto" style="width: 3rem">
                <button
                    type="button"
                    class="btn btn-sm"
                    :class="row._deleted ? 'btn-outline-secondary' : 'btn-outline-danger'"
                    :title="row._deleted ? $t('restore') : $t('delete')"
                    @click="row._deleted = !row._deleted"
                >
                    <i :class="row._deleted ? 'bi bi-arrow-counterclockwise' : 'bi bi-x-lg'"></i>
                </button>
            </div>
        </div>
        <!-- add-row -->
        <div v-if="newItem && !readonly" class="row g-2 mb-2 align-items-center fleet-line fleet-line--new">
            <div class="col-12 col-md-6">
                <InterventionTypeSelector
                    v-model="newItem.interventionType"
                    v-model:idValue="newItem.interventionTypeId"
                    :can-edit="false"
                    :placeholder="$t('addInterventionType')"
                    :filter-defaults="{ isActive: true, exclude: usedTypeIds }"
                    @select="(t?: InterventionType) => handleTypeSelect(newItem!, t)"
                />
            </div>
            <div class="col-5 col-md-2">
                <div class="input-group">
                    <span class="input-group-text"><i class="bi bi-currency-euro"></i></span>
                    <input v-model.number="newItem.cost" type="number" min="0" step="0.01" class="form-control" />
                </div>
            </div>
            <div class="col">
                <input v-model="newItem.remarks" class="form-control" maxlength="256" :placeholder="$t('remarks')" @keyup.enter="addRow" />
            </div>
            <div class="col-auto" style="width: 3rem">
                <button type="button" class="btn btn-sm btn-success" :disabled="!newItem.interventionTypeId" :title="$t('add')" @click="addRow">
                    <i class="bi bi-plus-lg"></i>
                </button>
            </div>
        </div>
        <div class="d-flex justify-content-end border-top pt-2 mt-2">
            <span class="text-secondary me-2">{{ $t("total") }}</span>
            <strong class="fleet-num">{{ fmtMoney(total) }}</strong>
        </div>
    </div>
</template>
