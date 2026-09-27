<!-- Owned-collection editor for Room.Equipment — an editable table (equipment picker + quantity).
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged
     rows so `Related()` deletes them by omission. New rows mint negative temp ids and insert with save(). -->
<script setup lang="ts">
import { computed } from "vue"
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { InputSelector as EquipmentSelector, FormModalButton as EquipmentButton, useEntityStore as useEquipmentStore } from "@/entities/equipment"
import type { Entity as Equipment } from "@/entities/equipment"
import RoomEquipment from "./Entity"

const props = defineProps<{ modelValue?: Array<RoomEquipment>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<RoomEquipment>] }>()
const { items, newItem, handleSave } = useOwnedCollection<RoomEquipment>({ props, emit, createRow: () => new RoomEquipment() })

const { fromPool } = useEquipmentStore()
const hydrate = (x?: Partial<Equipment>) => (x ? fromPool(x as Equipment) : undefined)
// already-listed equipment leaves the add-row picker
const usedIds = computed(() => items.value.filter((x) => !x._deleted && x.equipmentId).map((x) => x.equipmentId!))

function add() {
    if (!newItem.value?.equipmentId) return
    handleSave({ saved: newItem.value, isNew: true })
}
</script>

<template>
    <div class="equipment-editor">
        <p v-if="!items.length" class="italic-muted mb-2">{{ $t("noEquipment") }}</p>
        <div v-for="row in items" :key="row.id" class="row g-2 mb-2 align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col d-flex align-items-center gap-2 text-truncate">
                <EquipmentButton v-if="row.equipment" :model-value="hydrate(row.equipment)!" />
                <i :class="hydrate(row.equipment)?.$iconClass" class="text-primary"></i>
                <span class="text-truncate">{{ hydrate(row.equipment)?.$title }}</span>
            </div>
            <div class="col-3 col-md-2">
                <input type="number" min="1" max="100" v-model.number="row.quantity" :readonly="readonly || row._deleted" class="form-control" :title="$t('quantity')" />
            </div>
            <div v-if="!readonly" class="col-auto">
                <button type="button" class="btn" :class="row._deleted ? 'btn-outline-secondary' : 'btn-outline-danger'" :title="row._deleted ? $t('restore') : $t('delete')" @click="row._deleted = !row._deleted">
                    <i :class="row._deleted ? 'bi bi-arrow-counterclockwise' : 'bi bi-x-lg'"></i>
                </button>
            </div>
        </div>
        <div v-if="newItem && !readonly" class="row g-2 mt-1 align-items-center border-top pt-2">
            <div class="col">
                <EquipmentSelector v-model="newItem.equipment" v-model:idValue="newItem.equipmentId" :filter-defaults="{ exclude: usedIds }" :placeholder="$t('addEquipment')" />
            </div>
            <div class="col-3 col-md-2">
                <input type="number" min="1" max="100" v-model.number="newItem.quantity" class="form-control" :title="$t('quantity')" />
            </div>
            <div class="col-auto">
                <button type="button" class="btn btn-success" :disabled="!newItem.equipmentId" @click="add"><i class="bi bi-plus-lg"></i></button>
            </div>
        </div>
    </div>
</template>
