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

        <div class="row g-2 mb-2">
            <div class="col-sm-6">
                <input v-model.lazy.trim="searchObject.code" class="form-control" :placeholder="$t('orderCode')" @change="handleUpdate" />
            </div>
            <div class="col-sm-6">
                <input type="email" v-model.lazy.trim="searchObject.email" class="form-control" :placeholder="$t('email')" @change="handleUpdate" />
            </div>
        </div>
        <select v-model="searchObject.status" class="form-select mb-2" @change="handleUpdate">
            <option :value="undefined">{{ $t("allStatuses") }}</option>
            <option v-for="s in OrderStatuses" :key="s" :value="s">{{ s }}</option>
        </select>
        <div class="row g-2 mb-2">
            <div class="col">
                <input type="number" min="0" v-model.lazy.number="searchObject.minTotal" class="form-control" :placeholder="$t('minTotal')" @change="handleUpdate" />
            </div>
            <div class="col">
                <input type="number" min="0" v-model.lazy.number="searchObject.maxTotal" class="form-control" :placeholder="$t('maxTotal')" @change="handleUpdate" />
            </div>
        </div>
        <select v-model="searchObject.sortBy" class="form-select mb-2" @change="handleUpdate">
            <option :value="undefined">{{ $t("sortNewestFirst") }}</option>
            <option value="Oldest">{{ $t("sortOldestFirst") }}</option>
            <option value="TotalDesc">{{ $t("sortTotalDesc") }}</option>
            <option value="TotalAsc">{{ $t("sortTotalAsc") }}</option>
            <option value="Customer">{{ $t("sortCustomer") }}</option>
        </select>
    </div>
</template>

<script setup lang="ts">
import { IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { OrderStatuses } from "../data/Entity"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
</script>
