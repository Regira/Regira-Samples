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

        <input v-model.lazy.trim="searchObject.q" class="form-control mb-2" :placeholder="$t('vehicleKeywords')" @change="handleUpdate" />
        <div class="row g-2">
            <div class="col-md-4 mb-2">
                <FormLabel :label="$t('vehicleType')" />
                <select v-model="searchObject.vehicleType" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("all") }}</option>
                    <option v-for="t in VehicleTypes" :key="t" :value="t">{{ t }}</option>
                </select>
            </div>
            <div class="col-md-4 mb-2">
                <FormLabel :label="$t('fuelType')" />
                <select v-model="searchObject.fuelType" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("all") }}</option>
                    <option v-for="t in FuelTypes" :key="t" :value="t">{{ fuelLabel[t] ?? t }}</option>
                </select>
            </div>
            <div class="col-md-4 mb-2">
                <FormLabel :label="$t('status')" />
                <select v-model="searchObject.status" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("all") }}</option>
                    <option v-for="s in VehicleStatuses" :key="s" :value="s">{{ vehicleStatusBadge[s]!.label }}</option>
                </select>
            </div>
            <div class="col-md-4 mb-2">
                <FormLabel :label="$t('department')" />
                <input v-model.lazy.trim="searchObject.department" class="form-control" @change="handleUpdate" />
            </div>
            <div class="col-md-4 mb-2">
                <FormLabel :label="$t('serviceDueBefore')" />
                <input v-model="searchObject.serviceDueBefore" type="date" class="form-control" @change="handleUpdate" />
            </div>
            <div class="col-md-4 mb-2">
                <FormLabel :label="$t('sortBy')" />
                <select v-model="searchObject.sortBy" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("licensePlate") }}</option>
                    <option value="NextServiceDate">{{ $t("nextService") }} (soonest)</option>
                    <option value="MileageDesc">{{ $t("mileage") }} (highest)</option>
                    <option value="YearDesc">{{ $t("year") }} (newest)</option>
                    <option value="Year">{{ $t("year") }} (oldest)</option>
                    <option value="Make">{{ $t("make") }}</option>
                    <option value="Status">{{ $t("status") }}</option>
                </select>
            </div>
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('year')" />
                <div class="input-group">
                    <input v-model.lazy.number="searchObject.minYear" type="number" class="form-control" placeholder="from" @change="handleUpdate" />
                    <input v-model.lazy.number="searchObject.maxYear" type="number" class="form-control" placeholder="to" @change="handleUpdate" />
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { FormLabel, IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import { FuelTypes, VehicleStatuses, VehicleTypes, fuelLabel, vehicleStatusBadge } from "@/infrastructure/domain"
import SearchObject from "./SearchObject"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const { handleReset, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
</script>
