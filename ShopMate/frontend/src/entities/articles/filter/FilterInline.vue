<template>
    <div class="sm-inline-filter">
        <div class="input-group">
            <IconButton icon="clear" class="btn-outline-secondary" @click="handleReset" :aria-label="$t('clear')" />
            <input v-model.lazy.trim="searchObject.q" type="search" class="form-control" :placeholder="$t('searchArticles')" enterkeyhint="search" @change="handleUpdate" />
            <IconButton icon="search" class="btn-outline-primary d-none d-sm-block" @click="handleUpdate" />
            <IconButton v-if="showToggleAdv" icon="filter" :class="filterIsActive ? 'btn-info' : 'btn-outline-info'" @click="handleToggle" />
        </div>
        <!-- quick filters: status segments + category chips (the pick expands to its sub-categories) -->
        <div class="sm-segments mt-2" role="group" :aria-label="$t('status')">
            <button type="button" :class="{ active: status === undefined }" @click="setStatus(undefined)">{{ $t("all") }}</button>
            <button type="button" :class="{ active: status === true }" @click="setStatus(true)">{{ $t("toBuy") }}</button>
            <button type="button" :class="{ active: status === false }" @click="setStatus(false)">{{ $t("bought") }}</button>
        </div>
        <CategoryChipFilter v-model="categoryPick" class="mt-2" :all-label="$t('all')" @change="handleCategory" />
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import { CategoryChipFilter } from "@/entities/categories"
import config from "../config/config"
import SearchObject from "./SearchObject"
import { pickedCategory, parseBool } from "./functions"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<
    Emits & {
        "update:modelValue": (value: SearchObject) => true
        filter: (value: SearchObject) => true
        "toggle-adv": () => void
        close: () => void
    }
>()

const props = withDefaults(
    defineProps<{
        resultCount?: number
        showToggleAdv?: boolean
    }>(),
    {
        showToggleAdv: config.isComplex,
    }
)
const searchObject = defineModel<SearchObject>({
    default: () => new SearchObject(),
})

const { filterIsActive, handleReset, handleUpdate, handleToggle } = useFilter({
    searchObject,
    emit,
    Constructor: SearchObject,
})

// useRouteOverview hands the search object back parsed from the URL: values arrive as strings
const status = computed(() => parseBool(searchObject.value.isActive))
function setStatus(value?: boolean) {
    searchObject.value.isActive = value
    handleUpdate()
}
const categoryPick = computed({
    get: () => pickedCategory(searchObject.value.categoryId),
    set: () => {
        /* written through handleCategory */
    },
})
function handleCategory(ids?: Array<number>) {
    searchObject.value.categoryId = ids
    handleUpdate()
}
</script>
