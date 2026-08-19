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
        <input v-model.lazy.trim="searchObject.q" class="form-control mb-2" :placeholder="$t('keywords')" @change="handleUpdate" />
        <div class="row">
            <div class="col-sm mb-2">
                <select v-model="searchObject.department" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("allDepartments") }}</option>
                    <option v-for="dep in departments" :key="dep" :value="dep">{{ dep }}</option>
                </select>
            </div>
            <div class="col-sm mb-2">
                <input type="date" v-model="searchObject.minTrainingDate" class="form-control" @change="handleUpdate" />
            </div>
            <div class="col-sm mb-2">
                <input type="date" v-model="searchObject.maxTrainingDate" class="form-control" @change="handleUpdate" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const departments = ["Engineering", "Sales", "Marketing", "Human Resources", "Finance", "Operations", "Customer Support", "Product", "Legal", "IT"]

const searchObject = defineModel<SearchObject>({ required: true })
const { handleReset, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
</script>
