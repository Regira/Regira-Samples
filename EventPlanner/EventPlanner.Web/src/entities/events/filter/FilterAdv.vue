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
                <EventCategoryInputSelector v-model="category" v-model:idValue="searchObject.categoryId" :can-edit="false" :placeholder="$t('category')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <LocationItemInputSelector v-model="location" v-model:idValue="searchObject.locationId" :can-edit="false" :placeholder="$t('location')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2">
                <SpeakerInputSelector v-model="speaker" v-model:idValue="searchObject.speakerId" :can-edit="false" :placeholder="$t('speaker')" @select="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2" v-if="isAdmin()">
                <select v-model="searchObject.status" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("status") }}: {{ $t("all") }}</option>
                    <option v-for="s in statuses" :key="s" :value="s">{{ $t(s) }}</option>
                </select>
            </div>
            <div class="col-6 col-md-3 mb-2">
                <input v-model="searchObject.minDate" type="date" class="form-control" @change="handleUpdate" />
                <FormLabel :label="$t('from')" />
            </div>
            <div class="col-6 col-md-3 mb-2">
                <input v-model="searchObject.maxDate" type="date" class="form-control" @change="handleUpdate" />
                <FormLabel :label="$t('until')" />
            </div>
            <div class="col-6 col-md-3 mb-2">
                <NullableCheckBox v-model="searchObject.upcoming" id="evUpcoming" :label="$t('upcoming')" @update:modelValue="handleUpdate" />
            </div>
            <div class="col-6 col-md-3 mb-2">
                <NullableCheckBox v-model="searchObject.isFeatured" id="evFeatured" :label="$t('featured')" @update:modelValue="handleUpdate" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox, FormLabel } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import { InputSelector as EventCategoryInputSelector } from "@/entities/event-categories"
import type { Entity as EventCategory } from "@/entities/event-categories"
import { InputSelector as LocationItemInputSelector } from "@/entities/locations"
import type { Entity as LocationItem } from "@/entities/locations"
import { InputSelector as SpeakerInputSelector } from "@/entities/speakers"
import type { Entity as Speaker } from "@/entities/speakers"
import { useAccess } from "@/access"
import { EventStatus } from "../data/Entity"
import SearchObject from "./SearchObject"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const { isAdmin } = useAccess()
const statuses = Object.values(EventStatus)
const category = ref<EventCategory>()
const location = ref<LocationItem>()
const speaker = ref<Speaker>()
const searchObject = defineModel<SearchObject>({ required: true })
const { handleReset, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
</script>
