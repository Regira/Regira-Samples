<template>
    <div class="adv-filter">
        <!-- top row: result count (left) + clear (right) — the overview-filter convention; keep it -->
        <div class="row">
            <div class="col mb-2" v-if="resultCount != null">
                <span class="text-info">{{ resultCount }} {{ $t("results") }}</span>
                <small v-if="filterIsActive" class="ms-2 italic-muted">({{ $t("filtersAreApplied") }})</small>
            </div>
            <div class="col mb-2 text-end">
                <IconButton icon="clear" :showText="true" @click="handleReset" />
            </div>
        </div>

        <!-- keywords (free-text q) -->
        <input v-model.lazy.trim="searchObject.q" class="form-control mb-2" :placeholder="$t('keywords')" @change="handleUpdate" />

        <div class="mb-2">
            <FloorInputSelector
                v-model="filterFloor"
                v-model:idValue="searchObject.floorId as number"
                :canEdit="false"
                :placeholder="$t('floor')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <input type="number" min="1" v-model.number="searchObject.minCapacity" class="form-control" :placeholder="$t('minCapacity')" @change="handleUpdate" />
        </div>
        <div class="mb-2">
            <div class="form-check form-check-inline" v-for="opt in EquipmentOptions" :key="opt">
                <input
                    type="checkbox"
                    class="form-check-input"
                    :id="`filter-eq-${opt}`"
                    :checked="hasEquipment(opt)"
                    @change="toggleEquipment(opt)"
                />
                <label class="form-check-label" :for="`filter-eq-${opt}`">{{ opt }}</label>
            </div>
        </div>
        <NullableCheckBox v-model="searchObject.isActive" id="mr-isActive" :label="$t('isActive')" class="mb-2" @update:modelValue="handleUpdate" />
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as FloorInputSelector } from "@/entities/floors"
import type { Entity as Floor } from "@/entities/floors"
import { EquipmentOptions, type EquipmentOption } from "../data/Entity"

function hasEquipment(opt: EquipmentOption): boolean {
    return (searchObject.value.equipment ?? "").split(",").map((s) => s.trim()).includes(opt)
}
function toggleEquipment(opt: EquipmentOption) {
    const current = (searchObject.value.equipment ?? "").split(",").map((s) => s.trim()).filter(Boolean)
    const next = current.includes(opt) ? current.filter((x) => x !== opt) : [...current, opt]
    searchObject.value.equipment = next.join(", ") || undefined
    handleUpdate()
}

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterFloor = ref<Floor>()
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too — resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterFloor.value = undefined
}
</script>
