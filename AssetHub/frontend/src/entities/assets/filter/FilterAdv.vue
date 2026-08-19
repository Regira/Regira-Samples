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
            <CategoryInputSelector
                v-model="filterCategory"
                v-model:idValue="searchObject.categoryId as number"
                :canEdit="false"
                :placeholder="$t('category')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <AssetStatusInputSelector
                v-model="filterStatus"
                v-model:idValue="searchObject.statusId as number"
                :canEdit="false"
                :placeholder="$t('status')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <LocationItemInputSelector
                v-model="filterLocation"
                v-model:idValue="searchObject.locationId as number"
                :canEdit="false"
                :placeholder="$t('location')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <SupplierInputSelector
                v-model="filterSupplier"
                v-model:idValue="searchObject.supplierId as number"
                :canEdit="false"
                :placeholder="$t('supplier')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <NullableCheckBox v-model="searchObject.isAssigned" id="isAssignedFilter" :label="$t('asset.isAssigned')" @update:modelValue="handleUpdate" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import type { Entity as Category } from "@/entities/categories"
import { InputSelector as AssetStatusInputSelector } from "@/entities/asset-statuses"
import type { Entity as AssetStatus } from "@/entities/asset-statuses"
import { InputSelector as LocationItemInputSelector } from "@/entities/locations"
import type { Entity as LocationItem } from "@/entities/locations"
import { InputSelector as SupplierInputSelector } from "@/entities/suppliers"
import type { Entity as Supplier } from "@/entities/suppliers"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterCategory = ref<Category>()
const filterStatus = ref<AssetStatus>()
const filterLocation = ref<LocationItem>()
const filterSupplier = ref<Supplier>()
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too — resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterCategory.value = undefined
    filterStatus.value = undefined
    filterLocation.value = undefined
    filterSupplier.value = undefined
}
</script>
