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
            <input v-model.lazy.trim="searchObject.department" list="ah-departments" class="form-control" :placeholder="$t('department')" @change="handleUpdate" />
            <datalist id="ah-departments">
                <option v-for="d in departments" :key="d" :value="d" />
            </datalist>
        </div>
        <div class="d-flex flex-wrap gap-3 mb-2">
            <NullableCheckBox v-model="searchObject.isActive" id="empIsActive" :label="$t('active')" @update:modelValue="handleUpdate" />
            <NullableCheckBox v-model="searchObject.hasAssets" id="empHasAssets" :label="$t('holdsAssets')" @update:modelValue="handleUpdate" />
        </div>
        <select v-model="searchObject.sortBy" class="form-select mb-2" @change="handleUpdate">
            <option :value="undefined">{{ $t("sortBy") }}: {{ $t("name") }}</option>
            <option value="Code">{{ $t("sortBy") }}: {{ $t("employeeNumber") }}</option>
            <option value="Department">{{ $t("sortBy") }}: {{ $t("department") }}</option>
            <option value="HireDateDesc">{{ $t("sortBy") }}: {{ $t("hireDate") }}</option>
        </select>
        <div class="mb-2">
            <LocationInputSelector
                v-model="filterLocation"
                v-model:idValue="searchObject.locationId as number"
                :canEdit="false"
                :placeholder="$t('location')"
                @select="handleUpdate"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { NullableCheckBox } from "@regira/modules/vue/ui"
import { InputSelector as LocationInputSelector } from "@/entities/locations"
import type { Entity as Location } from "@/entities/locations"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterLocation = ref<Location>()
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too — resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterLocation.value = undefined
}
const departments = ["Engineering", "Sales", "Marketing", "Finance", "HR", "IT", "Operations", "Support", "Legal", "Facilities"]
</script>
