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
            <VenueInputSelector
                v-model="filterLocation"
                v-model:idValue="searchObject.locationId as number"
                :canEdit="false"
                :placeholder="$t('location')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <EventCategoryInputSelector
                v-model="filterEventCategory"
                v-model:idValue="searchObject.eventCategoryId as number"
                :canEdit="false"
                :placeholder="$t('eventCategory')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <NullableCheckBox v-model="searchObject.isFeatured" id="isFeatured" :label="$t('featuredOnly')" @update:modelValue="handleUpdate" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as VenueInputSelector } from "@/entities/locations"
import type { Entity as Venue } from "@/entities/locations"
import { InputSelector as EventCategoryInputSelector } from "@/entities/event-categories"
import type { Entity as EventCategory } from "@/entities/event-categories"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterLocation = ref<Venue>()
const filterEventCategory = ref<EventCategory>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterLocation.value = undefined
    filterEventCategory.value = undefined
}
</script>
