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
            <BrandInputSelector
                v-model="filterBrand"
                v-model:idValue="searchObject.brandId as number"
                :canEdit="false"
                :placeholder="$t('brand')"
                @select="handleUpdate"
            />
        </div>
        <div class="row g-2 mb-2">
            <div class="col">
                <input type="number" min="0" v-model.lazy.number="searchObject.minPrice" class="form-control" :placeholder="$t('minPrice')" @change="handleUpdate" />
            </div>
            <div class="col">
                <input type="number" min="0" v-model.lazy.number="searchObject.maxPrice" class="form-control" :placeholder="$t('maxPrice')" @change="handleUpdate" />
            </div>
        </div>
        <div class="d-flex flex-wrap gap-3 mb-2">
            <NullableCheckBox v-model="searchObject.isActive" id="f-active" :label="$t('published')" @update:modelValue="handleUpdate" />
            <NullableCheckBox v-model="searchObject.onSale" id="f-sale" :label="$t('onSale')" @update:modelValue="handleUpdate" />
            <NullableCheckBox v-model="searchObject.inStock" id="f-stock" :label="$t('inStock')" @update:modelValue="handleUpdate" />
            <NullableCheckBox v-model="searchObject.isFeatured" id="f-feat" :label="$t('featured')" @update:modelValue="handleUpdate" />
        </div>
        <select v-model="searchObject.sortBy" class="form-select mb-2" @change="handleUpdate">
            <option :value="undefined">{{ $t("sortDefault") }}</option>
            <option v-for="s in sortOptions" :key="s" :value="s">{{ $t("sort" + s) }}</option>
        </select>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { NullableCheckBox } from "@regira/modules/vue/ui"
const sortOptions = ["Newest", "PriceAsc", "PriceDesc", "Rating", "Popularity", "Title", "Stock"]
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import type { Entity as Category } from "@/entities/categories"
import { InputSelector as BrandInputSelector } from "@/entities/brands"
import type { Entity as Brand } from "@/entities/brands"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterCategory = ref<Category>()
const filterBrand = ref<Brand>()
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too — resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterCategory.value = undefined
    filterBrand.value = undefined
}
</script>
