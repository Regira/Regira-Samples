<!-- Owned-collection editor for Reservation.Attendees — an editable table (employee picker + response + optional).
     Removal marks `_deleted` (undoable until save); new rows mint negative temp ids and insert with the parent's save(). -->
<script setup lang="ts">
import { computed } from "vue"
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { InputSelector as EmployeeSelector, FormModalButton as EmployeeButton, useEntityStore as useEmployeeStore } from "@/entities/employees"
import type { Entity as Employee } from "@/entities/employees"
import { AttendeeResponse, statusVariant } from "@/utilities/planning"
import ReservationAttendee from "./Entity"

const props = defineProps<{ modelValue?: Array<ReservationAttendee>; readonly?: boolean; organizerId?: number }>()
const emit = defineEmits<{ "update:modelValue": [Array<ReservationAttendee>] }>()
const { items, newItem, handleSave } = useOwnedCollection<ReservationAttendee>({ props, emit, createRow: () => new ReservationAttendee() })

const { fromPool } = useEmployeeStore()
const hydrate = (x?: Partial<Employee>) => (x ? fromPool(x as Employee) : undefined)
const excluded = computed(() => [...items.value.filter((x) => !x._deleted && x.employeeId).map((x) => x.employeeId!), ...(props.organizerId ? [props.organizerId] : [])])
const responses = Object.values(AttendeeResponse)
const summary = computed(() => {
    const live = items.value.filter((x) => !x._deleted)
    return responses.map((r) => ({ response: r, count: live.filter((x) => x.response === r).length })).filter((x) => x.count > 0)
})

function add() {
    if (!newItem.value?.employeeId) return
    handleSave({ saved: newItem.value, isNew: true })
}
</script>

<template>
    <div class="attendee-editor">
        <div class="mb-2 d-flex flex-wrap gap-2">
            <span v-for="s in summary" :key="s.response" class="badge" :class="`text-bg-${statusVariant[s.response]}`">{{ $t(s.response) }}: {{ s.count }}</span>
        </div>
        <p v-if="!items.length" class="italic-muted mb-2">{{ $t("noAttendees") }}</p>
        <div v-for="row in items" :key="row.id" class="row g-2 mb-2 align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col d-flex align-items-center gap-2 text-truncate">
                <EmployeeButton v-if="row.employee" :model-value="hydrate(row.employee)!" />
                <span class="avatar">{{ hydrate(row.employee)?.$initials }}</span>
                <span class="text-truncate">
                    {{ hydrate(row.employee)?.$title }}
                    <small class="text-muted d-none d-md-inline ms-1">{{ hydrate(row.employee)?.department }}</small>
                </span>
            </div>
            <div class="col-5 col-md-3">
                <select v-model="row.response" :disabled="readonly || row._deleted" class="form-select form-select-sm">
                    <option v-for="r in responses" :key="r" :value="r">{{ $t(r) }}</option>
                </select>
            </div>
            <div class="col-auto">
                <div class="form-check mb-0">
                    <input :id="`att-opt-${row.id}`" type="checkbox" v-model="row.isOptional" :disabled="readonly || row._deleted" class="form-check-input" />
                    <label :for="`att-opt-${row.id}`" class="form-check-label small">{{ $t("optional") }}</label>
                </div>
            </div>
            <div v-if="!readonly" class="col-auto">
                <button type="button" class="btn btn-sm" :class="row._deleted ? 'btn-outline-secondary' : 'btn-outline-danger'" :title="row._deleted ? $t('restore') : $t('delete')" @click="row._deleted = !row._deleted">
                    <i :class="row._deleted ? 'bi bi-arrow-counterclockwise' : 'bi bi-x-lg'"></i>
                </button>
            </div>
        </div>
        <div v-if="newItem && !readonly" class="row g-2 mt-1 align-items-center border-top pt-2">
            <div class="col">
                <EmployeeSelector v-model="newItem.employee" v-model:idValue="newItem.employeeId" :filter-defaults="{ exclude: excluded, isActive: true }" :placeholder="$t('inviteAttendee')" />
            </div>
            <div class="col-auto">
                <div class="form-check mb-0">
                    <input id="att-opt-new" type="checkbox" v-model="newItem.isOptional" class="form-check-input" />
                    <label for="att-opt-new" class="form-check-label small">{{ $t("optional") }}</label>
                </div>
            </div>
            <div class="col-auto">
                <button type="button" class="btn btn-success" :disabled="!newItem.employeeId" @click="add"><i class="bi bi-person-plus"></i></button>
            </div>
        </div>
    </div>
</template>
