<template>
    <div class="adv-filter">
        <div class="row">
            <div class="col mb-2" v-if="resultCount != null">
                <span class="text-info">{{ resultCount }} {{ $t("results") }}</span>
                <small v-if="filterIsActive" class="ms-2 italic-muted">({{ $t("filtersAreApplied") }})</small>
            </div>
            <div class="col mb-2 text-end">
                <IconButton icon="clear" :showText="true" @click="handleReset" />
            </div>
        </div>

        <input v-model.lazy.trim="searchObject.q" class="form-control mb-2" :placeholder="$t('assetKeywords')" @change="handleUpdate" />

        <div class="row g-2">
            <div class="col-md-6 mb-2">
                <CategoryInputSelector v-model="filterCategory" v-model:idValue="searchObject.categoryId as number" :canEdit="false" :placeholder="$t('category')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <AssetStatusInputSelector v-model="filterStatus" v-model:idValue="searchObject.statusId as number" :canEdit="false" :placeholder="$t('status')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <LocationInputSelector v-model="filterLocation" v-model:idValue="searchObject.locationId as number" :canEdit="false" :placeholder="$t('location')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <SupplierInputSelector v-model="filterSupplier" v-model:idValue="searchObject.supplierId as number" :canEdit="false" :placeholder="$t('supplier')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <EmployeeInputSelector v-model="filterEmployee" v-model:idValue="searchObject.employeeId as number" :canEdit="false" :placeholder="$t('assignedTo')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <select v-model="searchObject.statusKind" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("anyStatusKind") }}</option>
                    <option v-for="k in statusKinds" :key="k" :value="k">{{ $t("kind" + k) }}</option>
                </select>
            </div>
        </div>

        <div class="d-flex flex-wrap gap-3 mb-2">
            <NullableCheckBox v-model="searchObject.isAssigned" id="assetIsAssigned" :label="$t('assigned')" @update:modelValue="handleUpdate" />
            <NullableCheckBox v-model="searchObject.underWarranty" id="assetUnderWarranty" :label="$t('underWarranty')" @update:modelValue="handleUpdate" />
            <NullableCheckBox v-model="searchObject.hasAttachment" id="assetHasAttachment" :label="$t('hasFiles')" @update:modelValue="handleUpdate" />
        </div>

        <div class="row g-2">
            <div class="col-md-6 mb-2">
                <input v-model="searchObject.warrantyExpiresBefore" type="date" class="form-control" @change="handleUpdate" />
                <FormLabel :label="$t('warrantyExpiresBefore')" />
            </div>
            <div class="col-md-6 mb-2">
                <input v-model="searchObject.maintenanceDueBefore" type="date" class="form-control" @change="handleUpdate" />
                <FormLabel :label="$t('maintenanceDueBefore')" />
            </div>
        </div>

        <select v-model="searchObject.sortBy" class="form-select mb-2" @change="handleUpdate">
            <option :value="undefined">{{ $t("sortBy") }}: {{ $t("newestFirst") }}</option>
            <option v-for="s in sortOptions" :key="s.value" :value="s.value">{{ $t("sortBy") }}: {{ $t(s.label) }}</option>
        </select>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox, FormLabel } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { statusKinds } from "@/entities/asset-statuses/data/Entity"
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import type { Entity as Category } from "@/entities/categories"
import { InputSelector as AssetStatusInputSelector } from "@/entities/asset-statuses"
import type { Entity as AssetStatus } from "@/entities/asset-statuses"
import { InputSelector as LocationInputSelector } from "@/entities/locations"
import type { Entity as Location } from "@/entities/locations"
import { InputSelector as SupplierInputSelector } from "@/entities/suppliers"
import type { Entity as Supplier } from "@/entities/suppliers"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import type { Entity as Employee } from "@/entities/employees"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const sortOptions = [
    { value: "Code", label: "assetTag" },
    { value: "Title", label: "name" },
    { value: "PurchaseDateDesc", label: "purchaseDateNewest" },
    { value: "PurchaseDate", label: "purchaseDateOldest" },
    { value: "PurchasePriceDesc", label: "priceHighest" },
    { value: "LastModifiedDesc", label: "recentlyModified" },
]

const searchObject = defineModel<SearchObject>({ required: true })
const filterCategory = ref<Category>()
const filterStatus = ref<AssetStatus>()
const filterLocation = ref<Location>()
const filterSupplier = ref<Supplier>()
const filterEmployee = ref<Employee>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterCategory.value = undefined
    filterStatus.value = undefined
    filterLocation.value = undefined
    filterSupplier.value = undefined
    filterEmployee.value = undefined
}
</script>
