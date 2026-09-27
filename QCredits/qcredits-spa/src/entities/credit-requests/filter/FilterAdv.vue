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
        <div class="row g-2 mb-2">
            <div class="col-md-4">
                <select v-model.number="searchObject.year" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("allYears") }}</option>
                    <option v-for="y in yearOptions()" :key="y" :value="y">{{ y }}</option>
                </select>
            </div>
            <div class="col-md-4">
                <select v-model="searchObject.status" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("allStatuses") }}</option>
                    <option v-for="s in Object.values(RequestStatus)" :key="s" :value="s">{{ $t(`status${s}`) }}</option>
                </select>
            </div>
            <div class="col-md-4">
                <select v-model="searchObject.activityType" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("allActivityTypes") }}</option>
                    <option v-for="t in Object.values(ActivityType)" :key="t" :value="t">{{ $t(`activity${t}`) }}</option>
                </select>
            </div>
        </div>
        <template v-if="isAdmin()">
            <div class="mb-2">
                <EmployeeInputSelector
                    v-model="filterEmployee"
                    v-model:idValue="searchObject.employeeId as number"
                    :canEdit="false"
                    :placeholder="$t('employee')"
                    @select="handleUpdate"
                />
            </div>
            <div class="mb-2">
                <DepartmentInputSelector
                    v-model="filterDepartment"
                    v-model:idValue="searchObject.departmentId as number"
                    :canEdit="false"
                    :placeholder="$t('department')"
                    @select="handleUpdate"
                />
            </div>
        </template>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import type { Entity as Employee } from "@/entities/employees"
import { InputSelector as DepartmentInputSelector } from "@/entities/departments"
import type { Entity as Department } from "@/entities/departments"
import { ActivityType, RequestStatus, yearOptions } from "@/domain/enums"
import { useAccess } from "@/access"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const { isAdmin } = useAccess()
const searchObject = defineModel<SearchObject>({ required: true })
const filterEmployee = ref<Employee>()
const filterDepartment = ref<Department>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterEmployee.value = undefined
    filterDepartment.value = undefined
}
</script>
