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

        <FormLabel :label="$t('status')" class="mb-1" />
        <div class="d-flex flex-wrap gap-2 mb-2">
            <div class="form-check form-check-inline" v-for="s in statusOptions" :key="s">
                <input
                    type="checkbox"
                    class="form-check-input"
                    :id="`status-${s}`"
                    :checked="searchObject.status?.includes(s)"
                    @change="toggleStatus(s)"
                />
                <label class="form-check-label" :for="`status-${s}`">{{ $t(`vehicleStatus.${s}`) }}</label>
            </div>
        </div>

        <FormLabel :label="$t('type')" class="mb-1" />
        <div class="d-flex flex-wrap gap-2 mb-2">
            <div class="form-check form-check-inline" v-for="t in typeOptions" :key="t">
                <input type="checkbox" class="form-check-input" :id="`type-${t}`" :checked="searchObject.type?.includes(t)" @change="toggleType(t)" />
                <label class="form-check-label" :for="`type-${t}`">{{ $t(`vehicleType.${t}`) }}</label>
            </div>
        </div>

        <div class="row">
            <div class="col-6 mb-2">
                <input v-model.number="searchObject.minYear" type="number" class="form-control" :placeholder="$t('minYear')" @change="handleUpdate" />
            </div>
            <div class="col-6 mb-2">
                <input v-model.number="searchObject.maxYear" type="number" class="form-control" :placeholder="$t('maxYear')" @change="handleUpdate" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { FormLabel, IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { VehicleType, VehicleStatus } from "../data/Entity"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const { handleReset, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })

const statusOptions = Object.values(VehicleStatus)
const typeOptions = Object.values(VehicleType)

function toggleStatus(s: VehicleStatus) {
    const current = searchObject.value.status ?? []
    searchObject.value.status = current.includes(s) ? current.filter((x) => x !== s) : [...current, s]
    handleUpdate()
}
function toggleType(t: VehicleType) {
    const current = searchObject.value.type ?? []
    searchObject.value.type = current.includes(t) ? current.filter((x) => x !== t) : [...current, t]
    handleUpdate()
}
</script>
