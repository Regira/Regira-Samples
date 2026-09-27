<!-- Owned-collection editor for GroupTraining.Participants: employee rows with an `attended` flag.
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged
     rows so `Related()` deletes them by omission. New rows mint negative temp ids and insert with save(). -->
<script setup lang="ts">
import { computed, ref } from "vue"
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { InputSelector as EmployeeInputSelector, FormModalButton as EmployeeButton, useEntityStore as useEmployeeStore } from "@/entities/employees"
import type { Entity as Employee } from "@/entities/employees"
import GroupTrainingParticipant from "./Entity"

const props = defineProps<{ modelValue?: Array<GroupTrainingParticipant>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<GroupTrainingParticipant>] }>()
const { items, newItem, handleSave } = useOwnedCollection<GroupTrainingParticipant>({ props, emit, createRow: () => new GroupTrainingParticipant() })
const { fromPool: getEmployee } = useEmployeeStore()

const picked = ref<Employee>()
const currentIds = computed(() => items.value.filter((x) => !x._deleted && x.employeeId).map((x) => x.employeeId!))
const attendedCount = computed(() => items.value.filter((x) => !x._deleted && x.attended).length)

function handleSelect(employee?: Employee) {
    if (!employee || !newItem.value || currentIds.value.includes(employee.id)) return
    newItem.value.employeeId = employee.id
    newItem.value.employee = employee
    handleSave({ saved: newItem.value, isNew: true })
    picked.value = undefined
}
</script>

<template>
    <div class="qc-participants">
        <div v-if="!readonly" class="mb-3">
            <EmployeeInputSelector
                v-model="picked"
                :canEdit="false"
                :filter-defaults="{ exclude: currentIds, isActive: true }"
                :placeholder="$t('addParticipant')"
                @select="handleSelect"
            />
        </div>
        <p v-if="!items.length" class="italic-muted">{{ $t("noParticipants") }}</p>
        <div class="row row-cols-1 row-cols-md-2 g-2">
            <div v-for="row in items" :key="row.id" class="col">
                <div class="d-flex align-items-center gap-2 border rounded px-2 py-1" :class="{ 'is-deleted': row._deleted }">
                    <EmployeeButton v-if="row.employee" :model-value="getEmployee(row.employee)" class="btn btn-sm btn-link p-0" />
                    <span class="flex-grow-1 text-truncate">
                        {{ getEmployee(row.employee)?.$title ?? row.employeeId }}
                        <small class="text-muted d-block text-truncate">{{ row.employee?.department?.title }}</small>
                    </span>
                    <div class="form-check form-switch mb-0" :title="$t('attended')">
                        <input :id="`attended-${row.id}`" v-model="row.attended" type="checkbox" class="form-check-input" :disabled="readonly || row._deleted" />
                        <label :for="`attended-${row.id}`" class="form-check-label small">{{ $t("attended") }}</label>
                    </div>
                    <button v-if="!readonly" type="button" class="btn btn-sm" :class="row._deleted ? 'btn-outline-secondary' : 'btn-outline-danger'" @click="row._deleted = !row._deleted">
                        <i :class="row._deleted ? 'bi bi-arrow-counterclockwise' : 'bi bi-x-lg'"></i>
                    </button>
                </div>
            </div>
        </div>
        <div class="small text-muted mt-2" v-if="items.length">
            {{ currentIds.length }} {{ $t("participants") }} &middot; {{ attendedCount }} {{ $t("attended").toLowerCase() }}
        </div>
    </div>
</template>
