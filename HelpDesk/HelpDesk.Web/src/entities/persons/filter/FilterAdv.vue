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

        <!-- TODO: one input per SearchObject filter field (placeholder `title` — keep in sync with SearchObject.ts).
             Native <input> → @change="handleUpdate". A custom component (InputSelector, NullableCheckBox,
             DateInput) emits Vue events only → @select="handleUpdate" / @update:modelValue="handleUpdate",
             or the results and the count go stale. A checkbox filter needs its own label — pass `label`
             (with an `id`, so clicking the text toggles the box). e.g.:
                 <BarInputSelector v-model="bar" v-model:idValue="searchObject.barId" @select="handleUpdate" />
                 <NullableCheckBox v-model="searchObject.isActive" id="isActive" :label="$t('isActive')" @update:modelValue="handleUpdate" /> -->
        <div class="row g-2 mb-2">
            <div class="col-md-6">
                <FormLabel :label="$t('role')" />
                <select v-model="searchObject.role" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("all") }}</option>
                    <option value="Customer">{{ $t("customers") }}</option>
                    <option value="Employee">{{ $t("employees") }}</option>
                </select>
            </div>
            <div class="col-md-6">
                <FormLabel :label="$t('sortBy')" />
                <select v-model="searchObject.sortBy" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("sortName") }}</option>
                    <option value="Company">{{ $t("company") }}</option>
                    <option value="Newest">{{ $t("sortNewest") }}</option>
                </select>
            </div>
        </div>
        <div class="d-flex flex-wrap gap-3 my-2">
            <NullableCheckBox v-model="searchObject.hasAccount" id="f-hasAccount" :label="$t('hasAccount')" @update:modelValue="handleUpdate" />
            <NullableCheckBox v-model="searchObject.isActive" id="f-isActive" :label="$t('active')" @update:modelValue="handleUpdate" />
        </div>
        <div class="mb-2">
            <SupportTeamInputSelector
                v-model="filterSupportTeam"
                v-model:idValue="searchObject.supportTeamId as number"
                :canEdit="false"
                :placeholder="$t('supportTeam')"
                @select="handleUpdate"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox, FormLabel } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as SupportTeamInputSelector } from "@/entities/support-teams"
import type { Entity as SupportTeam } from "@/entities/support-teams"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterSupportTeam = ref<SupportTeam>()
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too — resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterSupportTeam.value = undefined
}
</script>
