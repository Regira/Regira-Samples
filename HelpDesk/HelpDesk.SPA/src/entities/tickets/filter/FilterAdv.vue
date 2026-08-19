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

        <div class="mb-2 form-check">
            <input id="isUnassigned" type="checkbox" class="form-check-input" v-model="searchObject.isUnassigned" @change="handleUpdate" />
            <label for="isUnassigned" class="form-check-label">{{ $t("unassignedTickets") }}</label>
        </div>
        <div class="mb-2 form-check">
            <input id="isClosedFilter" type="checkbox" class="form-check-input" v-model="searchObject.isClosed" @change="handleUpdate" />
            <label for="isClosedFilter" class="form-check-label">{{ $t("isClosed") }}</label>
        </div>
        <div class="mb-2">
            <CategoryInputSelector
                v-model="filterCategory"
                v-model:idValue="searchObject.categoryId as number"
                :canEdit="false"
                :placeholder="$t('category')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <PriorityInputSelector
                v-model="filterPriority"
                v-model:idValue="searchObject.priorityId as number"
                :canEdit="false"
                :placeholder="$t('priority')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <StatusInputSelector
                v-model="filterStatus"
                v-model:idValue="searchObject.statusId as number"
                :canEdit="false"
                :placeholder="$t('status')"
                @select="handleUpdate"
            />
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
        <div class="mb-2">
            <PersonInputSelector
                v-model="filterCustomer"
                v-model:idValue="searchObject.customerId as number"
                :canEdit="false"
                :placeholder="$t('customer')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <PersonInputSelector
                v-model="filterAssignedEmployee"
                v-model:idValue="searchObject.assignedEmployeeId as number"
                :canEdit="false"
                :placeholder="$t('assignedEmployee')"
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
import { InputSelector as PriorityInputSelector } from "@/entities/priorities"
import type { Entity as Priority } from "@/entities/priorities"
import { InputSelector as StatusInputSelector } from "@/entities/statuses"
import type { Entity as Status } from "@/entities/statuses"
import { InputSelector as SupportTeamInputSelector } from "@/entities/support-teams"
import type { Entity as SupportTeam } from "@/entities/support-teams"
import { InputSelector as PersonInputSelector } from "@/entities/people"
import type { Entity as Person } from "@/entities/people"
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import type { Entity as Category } from "@/entities/categories"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterPriority = ref<Priority>()
const filterStatus = ref<Status>()
const filterSupportTeam = ref<SupportTeam>()
const filterCustomer = ref<Person>()
const filterAssignedEmployee = ref<Person>()
const filterCategory = ref<Category>()
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too — resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterPriority.value = undefined
    filterStatus.value = undefined
    filterSupportTeam.value = undefined
    filterCustomer.value = undefined
    filterAssignedEmployee.value = undefined
    filterCategory.value = undefined
}
</script>
