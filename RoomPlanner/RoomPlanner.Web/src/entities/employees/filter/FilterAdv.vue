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
        <select v-model="searchObject.department" class="form-select mb-2" @change="handleUpdate">
            <option :value="undefined">{{ $t("allDepartments") }}</option>
            <option v-for="d in departments" :key="d" :value="d">{{ d }}</option>
        </select>
        <NullableCheckBox v-model="searchObject.isActive" id="employeeIsActive" :label="$t('isActive')" @update:modelValue="handleUpdate" />
    </div>
</template>

<script setup lang="ts">
import { IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { departments } from "../data/Entity"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
</script>
