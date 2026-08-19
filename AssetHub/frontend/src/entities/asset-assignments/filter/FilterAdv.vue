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
            <AssetInputSelector
                v-model="filterAsset"
                v-model:idValue="searchObject.assetId as number"
                :canEdit="false"
                :placeholder="$t('asset')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <EmployeeInputSelector
                v-model="filterEmployee"
                v-model:idValue="searchObject.employeeId as number"
                :canEdit="false"
                :placeholder="$t('employee')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <NullableCheckBox v-model="searchObject.isActive" id="isActiveAssignmentFilter" :label="$t('assetAssignment.active')" @update:modelValue="handleUpdate" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as AssetInputSelector } from "@/entities/assets"
import type { Entity as Asset } from "@/entities/assets"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import type { Entity as Employee } from "@/entities/employees"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterAsset = ref<Asset>()
const filterEmployee = ref<Employee>()
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too — resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterAsset.value = undefined
    filterEmployee.value = undefined
}
</script>
