<!-- Owned-collection editor for EventItem.Sessions. Each session is a card (too many fields for a table row) with
     its speakers as InputSelectorInline chips (a pure join → nested Related() on the server).
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged rows at
     both levels so Related() deletes them by omission. New rows mint negative temp ids and insert with save(). -->
<script setup lang="ts">
import { computed } from "vue"
import { useOwnedCollection, InputSelectorInline } from "@regira/modules/vue/entities"
import { DateInput, FormLabel } from "@regira/modules/vue/ui"
import {
    InputSelector as SpeakerInputSelector,
    FormModalButton as SpeakerButton,
    useEntityStore as useSpeakerStore,
    type Entity as Speaker,
} from "@/entities/speakers"
import Session, { type SessionSpeaker } from "./Entity"

const props = defineProps<{ modelValue?: Array<Session>; readonly?: boolean; defaultDate?: string; defaultCapacity?: number }>()
const emit = defineEmits<{ "update:modelValue": [Array<Session>] }>()

function createRow(): Session {
    const start = new Date(`${props.defaultDate ?? new Date().toLocaleDateString("sv-SE")}T09:00:00`)
    return Session.create({ startTime: start, endTime: new Date(start.getTime() + 60 * 60000), capacity: props.defaultCapacity ?? 50, speakers: [] })
}
const { items, newItem, handleSave } = useOwnedCollection<Session>({ props, emit, createRow })

// the agenda reads chronologically, whatever order rows were added in
const sorted = computed(() => [...items.value].sort((a, b) => (a.startTime?.getTime() ?? 0) - (b.startTime?.getTime() ?? 0)))

const { fromPool } = useSpeakerStore()
const hydrate = (s?: Partial<Speaker>): Speaker | undefined => fromPool(s as Speaker)
const addSpeaker = (add: (row: SessionSpeaker) => void, s?: Speaker) => s && add({ speakerId: s.id, speaker: s })
function addSession() {
    if (!newItem.value) return
    handleSave({ saved: newItem.value, isNew: true })
}
</script>

<template>
    <div class="sessions-editor">
        <p v-if="!items.length" class="italic-muted">{{ $t("noSessions") }}</p>
        <div v-for="row in sorted" :key="row.id" class="card mb-3 ep-session-edit" :class="{ 'is-deleted': row._deleted }">
            <div class="card-body">
                <div class="row g-2">
                    <div class="col-md-6">
                        <input v-model="row.title" :readonly="readonly || row._deleted" class="form-control fw-semibold" required maxlength="160" />
                        <FormLabel :label="$t('title')" />
                    </div>
                    <div class="col-6 col-md-3">
                        <DateInput v-model="row.startTime" :show-time="true" :readonly="readonly || row._deleted" />
                        <FormLabel :label="$t('startTime')" />
                    </div>
                    <div class="col-6 col-md-3">
                        <DateInput v-model="row.endTime" :show-time="true" :readonly="readonly || row._deleted" />
                        <FormLabel :label="$t('endTime')" />
                    </div>
                    <div class="col-6 col-md-3">
                        <input v-model="row.room" :readonly="readonly || row._deleted" class="form-control" maxlength="96" />
                        <FormLabel :label="$t('room')" />
                    </div>
                    <div class="col-6 col-md-3">
                        <input v-model="row.track" :readonly="readonly || row._deleted" class="form-control" maxlength="64" />
                        <FormLabel :label="$t('track')" />
                    </div>
                    <div class="col-6 col-md-2">
                        <input v-model.number="row.capacity" type="number" min="1" :readonly="readonly || row._deleted" class="form-control" />
                        <FormLabel :label="$t('capacity')" />
                    </div>
                    <div class="col-6 col-md-4 d-flex align-items-start justify-content-end">
                        <span v-if="row.registeredCount != null" class="badge text-bg-light me-2 mt-2">{{ row.registeredCount }} / {{ row.capacity }} {{ $t("seats") }}</span>
                        <button
                            v-if="!readonly"
                            type="button"
                            class="btn"
                            :class="row._deleted ? 'btn-outline-secondary' : 'btn-outline-danger'"
                            :title="row._deleted ? $t('restore') : $t('delete')"
                            @click="row._deleted = !row._deleted"
                        >
                            <i :class="row._deleted ? 'bi bi-arrow-counterclockwise' : 'bi bi-trash'"></i>
                        </button>
                    </div>
                    <div class="col-12">
                        <InputSelectorInline
                            v-model="row.speakers"
                            :row-key="(r: SessionSpeaker) => r.speakerId"
                            :exclude-key="(r: SessionSpeaker) => r.speakerId"
                        >
                            <template #chip="{ row: link }">
                                <SpeakerButton :modelValue="hydrate(link.speaker)" />
                                {{ hydrate(link.speaker)?.$title }}
                            </template>
                            <template #selector="{ add, exclude }">
                                <SpeakerInputSelector
                                    v-if="!readonly && !row._deleted"
                                    :filter-defaults="{ exclude }"
                                    :placeholder="$t('sessionSpeakers')"
                                    @select="(s?: Speaker) => addSpeaker(add, s)"
                                />
                            </template>
                        </InputSelectorInline>
                        <FormLabel :label="$t('sessionSpeakers')" />
                    </div>
                    <div class="col-12">
                        <textarea v-model="row.description" :readonly="readonly || row._deleted" class="form-control" rows="2" maxlength="4000"></textarea>
                        <FormLabel :label="$t('description')" />
                    </div>
                </div>
            </div>
        </div>
        <button v-if="!readonly && newItem" type="button" class="btn btn-success" @click="addSession">
            <i class="bi bi-plus-lg me-1"></i>{{ $t("addSession") }}
        </button>
    </div>
</template>
