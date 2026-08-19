<!-- Owned-collection editor for Reservation.Attendees — an editable table of rows that carry BOTH a
     relation (an internal Employee, via InputSelector) and scalars (external guest name/email, response).
     Bind it in the parent Form.vue to the ARRAY: <ReservationAttendeeOverview v-model="item.attendees" />
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged
     rows so `Related()` deletes them by omission. New rows mint negative temp ids and insert with save(). -->
<script setup lang="ts">
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { IconButton } from "@regira/modules/vue/ui"
import ReservationAttendee, { ResponseStatuses } from "./Entity"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"

const props = defineProps<{ modelValue?: Array<ReservationAttendee>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<ReservationAttendee>] }>()
// items: writable computed over the collection (never undefined — [] until the parent has one)
// newItem: the add-row, minted by createRow so the model's field defaults and getters are present
// handleSave: appends newItem with a negative temp id, then mints the next add-row
const { items, newItem, handleSave } = useOwnedCollection<ReservationAttendee>({ props, emit, createRow: () => new ReservationAttendee() })

function clearExternal(row: ReservationAttendee) {
    row.externalName = undefined
    row.externalEmail = undefined
}
function clearEmployee(row: ReservationAttendee) {
    row.employeeId = undefined
    row.employee = undefined
}
</script>

<template>
    <div class="attendees-editor">
        <div class="row fw-bold border-bottom pb-1 mb-1 d-none d-md-flex">
            <div class="col-4">{{ $t("employee") }}</div>
            <div class="col-3">{{ $t("orExternalGuest") }}</div>
            <div class="col-3">{{ $t("response") }}</div>
        </div>
        <div v-for="row in items" :key="row.id" class="row g-2 mb-2 align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col-md-4">
                <EmployeeInputSelector
                    v-model="row.employee"
                    v-model:idValue="row.employeeId"
                    :readonly="readonly || row._deleted || !!row.externalName"
                    @select="clearExternal(row)"
                />
            </div>
            <div class="col-md-3">
                <input
                    v-model="row.externalName"
                    :readonly="readonly || row._deleted || !!row.employeeId"
                    class="form-control form-control-sm mb-1"
                    :placeholder="$t('guestName')"
                    @input="row.externalName && clearEmployee(row)"
                />
                <input
                    v-model="row.externalEmail"
                    type="email"
                    :readonly="readonly || row._deleted || !!row.employeeId"
                    class="form-control form-control-sm"
                    :placeholder="$t('guestEmail')"
                />
            </div>
            <div class="col-md-3">
                <select v-model="row.responseStatus" :disabled="readonly || row._deleted" class="form-select form-select-sm">
                    <option v-for="s in ResponseStatuses" :key="s" :value="s">{{ s }}</option>
                </select>
            </div>
            <div v-if="!readonly" class="col-md-2 col-auto">
                <IconButton
                    :icon="row._deleted ? 'restore' : 'delete'"
                    class="btn-sm btn-outline-danger"
                    :title="row._deleted ? $t('restore') : $t('remove')"
                    @click="row._deleted = !row._deleted"
                />
            </div>
        </div>

        <div v-if="newItem && !readonly" class="row g-2 mb-1 align-items-center">
            <div class="col-md-4">
                <EmployeeInputSelector v-model="newItem.employee" v-model:idValue="newItem.employeeId" />
            </div>
            <div class="col-md-3">
                <input v-model="newItem.externalName" class="form-control form-control-sm mb-1" :placeholder="$t('guestName')" />
                <input v-model="newItem.externalEmail" type="email" class="form-control form-control-sm" :placeholder="$t('guestEmail')" />
            </div>
            <div class="col-md-3">
                <select v-model="newItem.responseStatus" class="form-select form-select-sm">
                    <option v-for="s in ResponseStatuses" :key="s" :value="s">{{ s }}</option>
                </select>
            </div>
            <div class="col-md-2 col-auto">
                <IconButton icon="new" class="btn-sm btn-success" @click="handleSave({ saved: newItem, isNew: true })" />
            </div>
        </div>
        <p v-if="items.length === 0 && (!newItem || readonly)" class="text-muted small mb-0">{{ $t("noAttendeesYet") }}</p>
    </div>
</template>
