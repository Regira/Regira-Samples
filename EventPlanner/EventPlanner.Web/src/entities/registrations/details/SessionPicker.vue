<!-- Picks the sessions of ONE event for a registration (owned RegistrationSession join rows).
     A bounded checklist instead of InputSelectorInline chips: the choice is always among the few sessions of the
     selected event, and each option needs its time/room/seats shown. Unticking a persisted row marks it `_deleted`
     (re-ticking restores it); the service's prepareItem drops marked rows so Related() deletes them by omission. -->
<template>
    <LoadingContainer :is-loading="isLoading">
        <p v-if="!eventId" class="italic-muted mb-0">{{ $t("event") }}...</p>
        <p v-else-if="!sessions.length" class="italic-muted mb-0">{{ $t("noSessions") }}</p>
        <template v-else>
            <p class="small text-muted mb-2">{{ $t("chooseSessions") }}</p>
            <div v-for="day in days" :key="day.key" class="mb-2">
                <div class="ep-day-label">{{ day.label }}</div>
                <label
                    v-for="s in day.sessions"
                    :key="s.id"
                    class="ep-session-option"
                    :class="{ 'is-checked': isChecked(s.id), 'is-full': s.$seatsLeft <= 0 && !isChecked(s.id) }"
                >
                    <input
                        type="checkbox"
                        class="form-check-input mt-0"
                        :checked="isChecked(s.id)"
                        :disabled="readonly || (s.$seatsLeft <= 0 && !isChecked(s.id))"
                        @change="toggle(s.id)"
                    />
                    <span class="time">{{ s.$timeRange }}</span>
                    <span class="flex-grow-1 min-w-0">
                        <span class="d-block fw-semibold text-truncate">{{ s.title }}</span>
                        <small class="text-muted">
                            <span v-if="s.room"><i class="bi bi-door-open me-1"></i>{{ s.room }}</span>
                            <span v-if="speakerNames(s)" class="ms-2"><i class="bi bi-mic me-1"></i>{{ speakerNames(s) }}</span>
                        </small>
                    </span>
                    <span class="badge" :class="s.$seatsLeft > 0 ? 'text-bg-light' : 'text-bg-danger'">
                        {{ s.$seatsLeft > 0 ? `${s.$seatsLeft} ${$t("seatsLeft")}` : $t("full") }}
                    </span>
                </label>
            </div>
        </template>
    </LoadingContainer>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { LoadingContainer } from "@regira/modules/vue/ui"
import { onAuthenticated } from "@regira/modules/vue/auth"
import useEventStore from "@/entities/events/data/store"
import type { Entity as Session } from "@/entities/events/sessions"
import type { RegistrationSession } from "../data/Entity"

const props = defineProps<{ eventId?: number; readonly?: boolean }>()
const rows = defineModel<Array<RegistrationSession> | undefined>()

const { service: eventService } = useEventStore()
const sessions = ref<Array<Session>>([])
const isLoading = ref(false)
async function load() {
    sessions.value = []
    if (!props.eventId) return
    isLoading.value = true
    try {
        const ev = await eventService.details(props.eventId)
        sessions.value = (ev?.sessions ?? []) as Array<Session>
    } finally {
        isLoading.value = false
    }
}
watch(() => props.eventId, load)
onAuthenticated(load)

const days = computed(() => {
    const map = new Map<string, Array<Session>>()
    for (const s of [...sessions.value].sort((a, b) => (a.startTime?.getTime() ?? 0) - (b.startTime?.getTime() ?? 0)))
        map.set(s.$dayKey, [...(map.get(s.$dayKey) ?? []), s])
    return [...map.entries()].map(([key, list]) => ({
        key,
        label: list[0]?.startTime?.toLocaleDateString(undefined, { weekday: "long", day: "numeric", month: "long" }) ?? key,
        sessions: list,
    }))
})
const speakerNames = (s: Session) =>
    (s.speakers ?? [])
        .map((x) => [x.speaker?.firstName, x.speaker?.lastName].filter((n) => n).join(" "))
        .filter((n) => n)
        .join(", ")

const isChecked = (sessionId: number) => (rows.value ?? []).some((r) => r.sessionId === sessionId && !r._deleted)
function toggle(sessionId: number) {
    const list = [...(rows.value ?? [])]
    const row = list.find((r) => r.sessionId === sessionId)
    if (!row) list.push({ sessionId })
    else if (row.id) row._deleted = !row._deleted // persisted: mark / restore
    else list.splice(list.indexOf(row), 1) // added this session: remove outright
    rows.value = list
}
</script>
