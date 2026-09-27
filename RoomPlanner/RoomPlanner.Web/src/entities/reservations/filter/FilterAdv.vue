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
        <div class="row g-2">
            <div class="col-md-6 mb-2">
                <DateInput v-model="searchObject.from" @update:modelValue="handleUpdate" />
                <FormLabel :label="$t('from')" />
            </div>
            <div class="col-md-6 mb-2">
                <DateInput v-model="searchObject.to" @update:modelValue="handleUpdate" />
                <FormLabel :label="$t('until')" />
            </div>
        </div>
        <div class="row g-2">
            <div class="col-md-6 mb-2">
                <EmployeeInputSelector v-model="filterOrganizer" v-model:idValue="searchObject.organizerId as number" :canEdit="false" :placeholder="$t('organizer')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <EmployeeInputSelector v-model="filterEmployee" v-model:idValue="searchObject.employeeId as number" :canEdit="false" :placeholder="$t('participant')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <BuildingInputSelector v-model="filterBuilding" v-model:idValue="searchObject.buildingId as number" :canEdit="false" :placeholder="$t('building')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <RoomInputSelector v-model="filterRoom" v-model:idValue="searchObject.roomId as number" :canEdit="false" :placeholder="$t('room')" @select="handleUpdate" />
            </div>
        </div>
        <div class="mb-2">
            <FormLabel :label="$t('status')" />
            <div class="d-flex flex-wrap gap-1">
                <button
                    v-for="s in statuses"
                    :key="s"
                    type="button"
                    class="btn btn-sm rounded-pill"
                    :class="searchObject.status?.includes(s) ? `btn-${statusVariant[s]}` : 'btn-outline-secondary'"
                    @click="toggleStatus(s)"
                >
                    <i :class="statusIcon[s]" class="me-1"></i>{{ $t(s) }}
                </button>
            </div>
        </div>
        <NullableCheckBox v-model="searchObject.awaitingApproval" id="resAwaiting" :label="$t('awaitingApproval')" @update:modelValue="handleUpdate" />
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox, DateInput, FormLabel } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import type { Entity as Employee } from "@/entities/employees"
import { InputSelector as BuildingInputSelector } from "@/entities/buildings"
import type { Entity as Building } from "@/entities/buildings"
import { InputSelector as RoomInputSelector } from "@/entities/rooms"
import type { Entity as Room } from "@/entities/rooms"
import { ReservationStatus, statusIcon, statusVariant } from "@/utilities/planning"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterOrganizer = ref<Employee>()
const filterEmployee = ref<Employee>()
const filterBuilding = ref<Building>()
const filterRoom = ref<Room>()
const statuses = Object.values(ReservationStatus)
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })

function toggleStatus(s: ReservationStatus) {
    const current = searchObject.value.status ?? []
    const next = current.includes(s) ? current.filter((x) => x !== s) : [...current, s]
    searchObject.value.status = next.length ? next : undefined
    handleUpdate()
}
function handleReset() {
    resetSearchObject()
    filterOrganizer.value = undefined
    filterEmployee.value = undefined
    filterBuilding.value = undefined
    filterRoom.value = undefined
}
</script>
