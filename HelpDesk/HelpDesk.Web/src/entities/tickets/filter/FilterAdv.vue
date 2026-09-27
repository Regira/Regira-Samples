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

        <input v-model.lazy.trim="searchObject.q" class="form-control mb-3" :placeholder="$t('ticketKeywords')" @change="handleUpdate" />

        <div class="row g-2 mb-2">
            <div class="col-md-6">
                <FormLabel :label="$t('status')" />
                <StatusInputSelector v-model="filterStatus" v-model:idValue="searchObject.statusId as number" :canEdit="false" :placeholder="$t('status')" @select="handleUpdate" />
            </div>
            <div class="col-md-6">
                <FormLabel :label="$t('priority')" />
                <PriorityInputSelector v-model="filterPriority" v-model:idValue="searchObject.priorityId as number" :canEdit="false" :placeholder="$t('priority')" @select="handleUpdate" />
            </div>
            <div class="col-md-6">
                <FormLabel :label="$t('category')" />
                <CategoryInputSelector v-model="filterCategory" v-model:idValue="searchObject.categoryId as number" :canEdit="false" :placeholder="$t('category')" @select="handleUpdate" />
            </div>
            <template v-if="isStaff">
                <div class="col-md-6">
                    <FormLabel :label="$t('supportTeam')" />
                    <SupportTeamInputSelector v-model="filterSupportTeam" v-model:idValue="searchObject.supportTeamId as number" :canEdit="false" :placeholder="$t('supportTeam')" @select="handleUpdate" />
                </div>
                <div class="col-md-6">
                    <FormLabel :label="$t('customer')" />
                    <PersonInputSelector v-model="filterCustomer" v-model:idValue="searchObject.customerId as number" :canEdit="false" :placeholder="$t('customer')" :filter-defaults="{ role: 'Customer' }" @select="handleUpdate" />
                </div>
                <div class="col-md-6">
                    <FormLabel :label="$t('assignedEmployee')" />
                    <PersonInputSelector v-model="filterAssignedEmployee" v-model:idValue="searchObject.assignedEmployeeId as number" :canEdit="false" :placeholder="$t('assignedEmployee')" :filter-defaults="{ role: 'Employee' }" @select="handleUpdate" />
                </div>
            </template>
        </div>

        <div class="d-flex flex-wrap gap-3 my-3">
            <NullableCheckBox v-model="searchObject.isClosed" id="f-isClosed" :label="$t('closed')" @update:modelValue="handleUpdate" />
            <NullableCheckBox v-model="searchObject.isOverdue" id="f-isOverdue" :label="$t('overdue')" @update:modelValue="handleUpdate" />
            <NullableCheckBox v-model="searchObject.hasAttachment" id="f-hasAttachment" :label="$t('hasAttachment')" @update:modelValue="handleUpdate" />
            <template v-if="isStaff">
                <NullableCheckBox v-model="searchObject.isAssigned" id="f-isAssigned" :label="$t('assigned')" @update:modelValue="handleUpdate" />
                <NullableCheckBox v-model="searchObject.assignedToMe" id="f-assignedToMe" :label="$t('assignedToMe')" @update:modelValue="handleUpdate" />
            </template>
        </div>

        <div class="row g-2">
            <div class="col-md-6">
                <FormLabel :label="$t('sortBy')" />
                <select v-model="searchObject.sortBy" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("sortNewest") }}</option>
                    <option v-for="s in sortOptions" :key="s" :value="s">{{ $t("sort" + s) }}</option>
                </select>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox, FormLabel } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as PersonInputSelector } from "@/entities/persons"
import type { Entity as Person } from "@/entities/persons"
import { InputSelector as StatusInputSelector } from "@/entities/statuses"
import type { Entity as Status } from "@/entities/statuses"
import { InputSelector as PriorityInputSelector } from "@/entities/priorities"
import type { Entity as Priority } from "@/entities/priorities"
import { InputSelector as SupportTeamInputSelector } from "@/entities/support-teams"
import type { Entity as SupportTeam } from "@/entities/support-teams"
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import type { Entity as Category } from "@/entities/categories"
import { useAccess } from "@/infrastructure/access"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const { isStaff } = useAccess()
const sortOptions = ["LastActivity", "Priority", "DueDate", "Oldest", "Code"]

const searchObject = defineModel<SearchObject>({ required: true })
const filterCustomer = ref<Person>()
const filterAssignedEmployee = ref<Person>()
const filterStatus = ref<Status>()
const filterPriority = ref<Priority>()
const filterSupportTeam = ref<SupportTeam>()
const filterCategory = ref<Category>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterCustomer.value = undefined
    filterAssignedEmployee.value = undefined
    filterStatus.value = undefined
    filterPriority.value = undefined
    filterSupportTeam.value = undefined
    filterCategory.value = undefined
}
</script>
