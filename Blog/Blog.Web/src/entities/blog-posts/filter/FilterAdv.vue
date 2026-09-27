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

        <!-- keywords (free-text q over title, summary and author) -->
        <input v-model.lazy.trim="searchObject.q" class="form-control mb-3" :placeholder="$t('keywords')" @change="handleUpdate" />

        <div class="row g-2 mb-2">
            <div class="col-md-6">
                <CategoryInputSelector
                    v-model="filterCategory"
                    v-model:idValue="searchObject.categoryId as number"
                    :canEdit="false"
                    :placeholder="$t('category')"
                    @select="handleUpdate"
                />
            </div>
            <div class="col-md-6">
                <TagInputSelector
                    v-model="filterTag"
                    v-model:idValue="searchObject.tagId as number"
                    :canEdit="false"
                    :placeholder="$t('tag')"
                    @select="handleUpdate"
                />
            </div>
        </div>

        <div class="row g-2 mb-2">
            <div class="col-sm-6">
                <select v-model="searchObject.status" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("anyStatus") }}</option>
                    <option v-for="s in statuses" :key="s" :value="s">{{ $t("status" + s) }}</option>
                </select>
                <FormLabel :label="$t('status')" />
            </div>
            <div class="col-sm-6">
                <select v-model="searchObject.year" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("anyYear") }}</option>
                    <option v-for="y in years" :key="y" :value="String(y)">{{ y }}</option>
                </select>
                <FormLabel :label="$t('publishedIn')" />
            </div>
        </div>

        <div class="row g-2 mb-2">
            <div class="col-sm-6">
                <input v-model.lazy.trim="searchObject.author" class="form-control" :placeholder="$t('author')" @change="handleUpdate" />
                <FormLabel :label="$t('authorExact')" />
            </div>
            <div class="col-sm-6">
                <select v-model="searchObject.sortBy" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("sortNewest") }}</option>
                    <option v-for="s in sortOptions" :key="s.value" :value="s.value">{{ $t(s.label) }}</option>
                </select>
                <FormLabel :label="$t('sortBy')" />
            </div>
        </div>

        <div class="mb-2">
            <NullableCheckBox v-model="searchObject.isFeatured" id="isFeaturedFilter" :label="$t('featured')" @update:modelValue="handleUpdate" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox, FormLabel } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { PostStatus } from "../data/Entity"
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import type { Entity as Category } from "@/entities/categories"
import { InputSelector as TagInputSelector } from "@/entities/tags"
import type { Entity as Tag } from "@/entities/tags"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const statuses = Object.values(PostStatus)
const thisYear = new Date().getFullYear()
const years = [thisYear, thisYear - 1, thisYear - 2, thisYear - 3]
const sortOptions = [
    { value: "Oldest", label: "sortOldest" },
    { value: "Title", label: "sortTitle" },
    { value: "ReadingTimeDesc", label: "sortLongest" },
    { value: "LastModified", label: "sortLastModified" },
    { value: "Created", label: "sortCreated" },
]

const searchObject = defineModel<SearchObject>({ required: true })
const filterCategory = ref<Category>()
const filterTag = ref<Tag>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too - resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterCategory.value = undefined
    filterTag.value = undefined
}
</script>
