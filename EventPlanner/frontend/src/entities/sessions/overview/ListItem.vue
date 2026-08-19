<template>
    <div class="agenda-row card mb-2 border-0 shadow-sm">
        <div class="card-body py-2 px-3">
            <div class="row align-items-center g-2">
                <div class="col-auto agenda-time text-center">
                    <div class="fw-bold">{{ formatTime(item.startTime) }}</div>
                    <div class="small text-muted">{{ formatTime(item.endTime) }}</div>
                </div>
                <div class="col-auto vr d-none d-sm-block" />
                <div class="col">
                    <div class="d-flex align-items-center gap-2 flex-wrap">
                        <h3 class="h6 mb-0">{{ item.$title }}</h3>
                        <span v-if="item.room" class="badge text-bg-light border"><Icon name="bi bi-door-open-fill" class="me-1" />{{ item.room }}</span>
                    </div>
                    <div class="small text-muted text-truncate">
                        <EventItemButton :model-value="getEvent(item.event)" /> {{ getEvent(item.event)?.$title }}
                        <span v-if="item.sessionSpeakers?.length"> · {{ speakerNames }}</span>
                    </div>
                </div>
                <div class="col-auto text-end" style="min-width: 110px">
                    <div class="small text-muted">{{ item.seatsTaken ?? 0 }}/{{ item.capacity }} {{ $t("seats") }}</div>
                    <div class="progress" style="height: 6px">
                        <div
                            class="progress-bar"
                            :class="fillRatio >= 1 ? 'bg-danger' : fillRatio >= 0.75 ? 'bg-warning' : 'bg-success'"
                            :style="{ width: `${Math.min(fillRatio * 100, 100)}%` }"
                        />
                    </div>
                </div>
                <div class="col-auto">
                    <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                        <Icon name="edit" />
                    </RouterLink>
                </div>
                <div class="col-auto">
                    <ConfirmButton icon="delete" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
                        {{ $t("deleteItem", { title: item?.$title }) }}
                    </ConfirmButton>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import { formatDateTime } from "@regira/modules/vue/formatters"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import { FormModalButton as EventItemButton, useEntityStore as useEventItemStore } from "@/entities/events"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getEvent } = useEventItemStore()

function formatTime(date?: Date) {
    return date ? formatDateTime(date, "HH:mm") : "—"
}
const speakerNames = computed(() => item.value.sessionSpeakers?.map((s) => s.speaker?.title).filter(Boolean).join(", "))
const fillRatio = computed(() => (item.value.capacity ? (item.value.seatsTaken ?? 0) / item.value.capacity : 0))
</script>

<style scoped>
.agenda-time {
    min-width: 64px;
}
</style>
