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
            <EventItemInputSelector
                v-model="filterEvent"
                v-model:idValue="searchObject.eventId as number"
                :canEdit="false"
                :placeholder="$t('event')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <SpeakerInputSelector
                v-model="filterSpeaker"
                v-model:idValue="searchObject.speakerId as number"
                :canEdit="false"
                :placeholder="$t('speaker')"
                @select="handleUpdate"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as EventItemInputSelector, type Entity as EventItem } from "@/entities/events"
import { InputSelector as SpeakerInputSelector, type Entity as Speaker } from "@/entities/speakers"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterEvent = ref<EventItem>()
const filterSpeaker = ref<Speaker>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterEvent.value = undefined
    filterSpeaker.value = undefined
}
</script>
