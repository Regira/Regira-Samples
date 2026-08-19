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
            <EmployeeInputSelector
                v-model="filterOrganizer"
                v-model:idValue="searchObject.organizerId as number"
                :canEdit="false"
                :placeholder="$t('organizer')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <select v-model="searchObject.status" class="form-select" @change="handleUpdate">
                <option :value="undefined">{{ $t("anyStatus") }}</option>
                <option v-for="s in ReservationStatuses" :key="s" :value="s">{{ s }}</option>
            </select>
        </div>
        <div class="row">
            <div class="col-sm mb-2">
                <div class="input-group">
                    <div class="input-group-text"><Icon name="from" /></div>
                    <input type="date" v-model="searchObject.minStartTime" class="form-control" @change="handleUpdate" />
                </div>
            </div>
            <div class="col-sm mb-2">
                <div class="input-group">
                    <div class="input-group-text"><Icon name="to" /></div>
                    <input type="date" v-model="searchObject.maxStartTime" class="form-control" @change="handleUpdate" />
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { Icon, IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import type { Entity as Employee } from "@/entities/employees"
import { ReservationStatuses } from "../data/Entity"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterOrganizer = ref<Employee>()
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too — resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterOrganizer.value = undefined
}
</script>
