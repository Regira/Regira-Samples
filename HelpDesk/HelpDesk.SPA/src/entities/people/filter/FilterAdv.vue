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
            <div v-for="r in PERSON_ROLES" :key="r" class="form-check form-check-inline">
                <input :id="'role-' + r" type="checkbox" class="form-check-input" :checked="(searchObject.role ?? []).includes(r)" @change="toggleRole(r)" />
                <label :for="'role-' + r" class="form-check-label">{{ $t(r) }}</label>
            </div>
        </div>
        <div class="mb-2 form-check">
            <input id="isActive" type="checkbox" class="form-check-input" v-model="searchObject.isActive" @change="handleUpdate" />
            <label for="isActive" class="form-check-label">{{ $t("isActive") }}</label>
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
import { IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { PERSON_ROLES, type PersonRole } from "../data/Entity"
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

function toggleRole(r: PersonRole) {
    const current = searchObject.value.role ?? []
    searchObject.value.role = current.includes(r) ? current.filter((x) => x !== r) : [...current, r]
    handleUpdate()
}
</script>
