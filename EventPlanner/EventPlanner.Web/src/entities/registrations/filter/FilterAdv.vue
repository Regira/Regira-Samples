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
        <div class="mb-2">
            <EventItemInputSelector v-model="event" v-model:idValue="searchObject.eventId" :can-edit="false" :placeholder="$t('event')" @select="handleUpdate" />
        </div>
        <div v-if="isAdmin()" class="mb-2">
            <EmployeeInputSelector v-model="user" v-model:idValue="searchObject.userId" :can-edit="false" :placeholder="$t('employee')" @select="handleUpdate" />
        </div>
        <div class="row g-2 align-items-center">
            <div class="col-md-6">
                <select v-model="searchObject.status" class="form-select mb-2" @change="handleUpdate">
                    <option :value="undefined">{{ $t("status") }}: {{ $t("all") }}</option>
                    <option v-for="s in statuses" :key="s" :value="s">{{ $t(s) }}</option>
                </select>
            </div>
            <div class="col-md-6 mb-2">
                <NullableCheckBox v-model="searchObject.upcoming" id="regUpcoming" :label="$t('upcoming')" @update:modelValue="handleUpdate" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import { InputSelector as EventItemInputSelector } from "@/entities/events"
import type { Entity as EventItem } from "@/entities/events"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import type { Entity as Employee } from "@/entities/employees"
import { useAccess } from "@/access"
import { RegistrationStatus } from "../data/Entity"
import SearchObject from "./SearchObject"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const { isAdmin } = useAccess()
const statuses = Object.values(RegistrationStatus)
const event = ref<EventItem>()
const user = ref<Employee>()
const searchObject = defineModel<SearchObject>({ required: true })
const { handleReset, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
</script>
