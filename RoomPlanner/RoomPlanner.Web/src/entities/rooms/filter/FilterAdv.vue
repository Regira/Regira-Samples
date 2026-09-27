<template>
    <div class="adv-filter">
        <!-- top row: result count (left) + clear (right) -->
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
        <div class="row g-2">
            <div class="col-md-6 mb-2">
                <BuildingInputSelector
                    v-model="filterBuilding"
                    v-model:idValue="searchObject.buildingId as number"
                    :canEdit="false"
                    :placeholder="$t('building')"
                    @select="handleUpdate"
                />
            </div>
            <div class="col-md-6 mb-2">
                <FloorInputSelector
                    v-model="filterFloor"
                    v-model:idValue="searchObject.floorId as number"
                    :canEdit="false"
                    :filter-defaults="searchObject.buildingId ? { buildingId: searchObject.buildingId } : {}"
                    :placeholder="$t('floor')"
                    @select="handleUpdate"
                />
            </div>
        </div>
        <div class="row g-2 align-items-center">
            <div class="col-sm-6 mb-2">
                <div class="input-group">
                    <span class="input-group-text"><Icon name="people" /></span>
                    <input type="number" min="1" v-model.lazy.number="searchObject.minCapacity" class="form-control" :placeholder="$t('minCapacity')" @change="handleUpdate" />
                </div>
            </div>
            <div class="col-sm-6 mb-2 d-flex gap-3">
                <NullableCheckBox v-model="searchObject.requiresApproval" id="roomRequiresApproval" :label="$t('requiresApproval')" @update:modelValue="handleUpdate" />
                <NullableCheckBox v-model="searchObject.isActive" id="roomIsActive" :label="$t('isActive')" @update:modelValue="handleUpdate" />
            </div>
        </div>
        <div class="mb-2">
            <FormLabel :label="$t('equipment')" />
            <EquipmentToggles v-model="searchObject.equipmentId" size="sm" @change="handleUpdate" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, Icon, NullableCheckBox, FormLabel } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as FloorInputSelector } from "@/entities/floors"
import type { Entity as Floor } from "@/entities/floors"
import { InputSelector as BuildingInputSelector } from "@/entities/buildings"
import type { Entity as Building } from "@/entities/buildings"
import EquipmentToggles from "@/components/planning/EquipmentToggles.vue"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterFloor = ref<Floor>()
const filterBuilding = ref<Building>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterFloor.value = undefined
    filterBuilding.value = undefined
}
</script>
