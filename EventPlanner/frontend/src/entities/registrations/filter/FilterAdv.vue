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
            <EventItemInputSelector
                v-model="filterEvent"
                v-model:idValue="searchObject.eventId as number"
                :canEdit="false"
                :placeholder="$t('event')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <select v-model="searchObject.status" class="form-select" @change="handleUpdate">
                <option :value="undefined">{{ $t("allStatuses") }}</option>
                <option v-for="s in statuses" :key="s" :value="s">{{ $t(`registrationStatus.${s}`) }}</option>
            </select>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { RegistrationStatus } from "../data/Entity"
import { InputSelector as EmployeeInputSelector, type Entity as Employee } from "@/entities/employees"
import { InputSelector as EventItemInputSelector, type Entity as EventItem } from "@/entities/events"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterEmployee = ref<Employee>()
const filterEvent = ref<EventItem>()
const statuses = Object.values(RegistrationStatus)
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterEmployee.value = undefined
    filterEvent.value = undefined
}
</script>
