<!-- The public event page: banner, key facts, registration panel, agenda (per day) and speaker cards.
     Registration writes go through the registrations slice (deep imports: that slice imports this barrel). -->
<template>
    <article class="ep-event-page">
        <header class="ep-hero" :style="item.$bannerStyle">
            <div class="ep-hero-inner">
                <div class="d-flex flex-wrap gap-2 mb-2">
                    <span class="ep-category-badge ep-on-dark" :style="{ '--ep-cat': item.$color }"><i :class="item.$icon" class="me-1"></i>{{ item.category?.title }}</span>
                    <span v-if="item.status !== 'Published'" class="badge ep-status" :class="`ep-status-${item.status}`">{{ $t(item.status) }}</span>
                    <span v-if="item.isFeatured" class="badge text-bg-warning"><i class="bi bi-star-fill me-1"></i>{{ $t("featured") }}</span>
                </div>
                <h1 class="ep-hero-title">{{ item.title }}</h1>
                <p v-if="item.summary" class="ep-hero-summary">{{ item.summary }}</p>
                <div class="ep-hero-facts">
                    <span><i class="bi bi-calendar3 me-2"></i>{{ item.$dateRange }}</span>
                    <span v-if="item.location"><i class="bi bi-geo-alt me-2"></i>{{ item.location.title }}<template v-if="item.location.city">, {{ item.location.city }}</template></span>
                    <span v-if="item.maxParticipants"><i class="bi bi-people me-2"></i>{{ item.registrationCount ?? 0 }} / {{ item.maxParticipants }}</span>
                </div>
            </div>
            <div class="ep-hero-actions">
                <RouterLink v-if="canEdit" :to="{ name: 'EventItemForm', params: { id: item.$id } }" class="btn btn-light btn-sm">
                    <i class="bi bi-pencil me-1"></i>{{ $t("editEvent") }}
                </RouterLink>
                <RouterLink v-if="overviewUrl" :to="overviewUrl" class="btn btn-outline-light btn-sm">
                    <i class="bi bi-grid me-1"></i><span class="d-none d-sm-inline">{{ $t("overview") }}</span>
                </RouterLink>
            </div>
        </header>

        <div class="row g-4 mt-1">
            <!-- main column -->
            <div class="col-lg-8 order-2 order-lg-1">
                <section v-if="item.description" class="mb-4">
                    <h3 class="ep-section-title">{{ $t("description") }}</h3>
                    <p v-for="(p, i) in paragraphs" :key="i" class="text-body-secondary">{{ p }}</p>
                </section>

                <section class="mb-4">
                    <h3 class="ep-section-title"><i class="bi bi-calendar-week me-2"></i>{{ $t("agenda") }}</h3>
                    <p v-if="!days.length" class="italic-muted">{{ $t("noSessions") }}</p>
                    <div v-for="(day, di) in days" :key="day.key" class="mb-3">
                        <div class="ep-day-label">
                            <span class="ep-day-number" :style="{ background: item.$color }">{{ $t("day") }} {{ di + 1 }}</span>
                            {{ day.label }}
                        </div>
                        <div class="ep-timeline">
                            <div v-for="s in day.sessions" :key="s.id" class="ep-timeline-item" :class="{ 'is-mine': mySessionIds.has(s.id) }">
                                <div class="ep-timeline-time">{{ s.$timeRange }}</div>
                                <div class="ep-timeline-card">
                                    <div class="d-flex justify-content-between gap-2">
                                        <h5 class="mb-1">{{ s.title }}</h5>
                                        <span v-if="mySessionIds.has(s.id)" class="badge text-bg-success align-self-start"><i class="bi bi-check2"></i></span>
                                    </div>
                                    <div class="small text-muted mb-2">
                                        <span v-if="s.room" class="me-3"><i class="bi bi-door-open me-1"></i>{{ s.room }}</span>
                                        <span v-if="s.track" class="me-3"><i class="bi bi-signpost-split me-1"></i>{{ s.track }}</span>
                                        <span><i class="bi bi-person-check me-1"></i>{{ s.registeredCount ?? 0 }} / {{ s.capacity }}</span>
                                    </div>
                                    <p v-if="s.description" class="small mb-2">{{ s.description }}</p>
                                    <div class="d-flex flex-wrap gap-2">
                                        <RouterLink
                                            v-for="link in s.speakers ?? []"
                                            :key="link.speakerId"
                                            :to="{ name: 'SpeakerDetails', params: { id: link.speakerId } }"
                                            class="ep-speaker-chip"
                                        >
                                            <SpeakerAvatar :speaker="speaker(link.speaker)" size="sm" />
                                            {{ speaker(link.speaker)?.$title }}
                                        </RouterLink>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>

                <section class="mb-4">
                    <h3 class="ep-section-title"><i class="bi bi-mic me-2"></i>{{ $t("speakers") }}</h3>
                    <p v-if="!speakers.length" class="italic-muted">{{ $t("noSpeakers") }}</p>
                    <div class="row row-cols-1 row-cols-sm-2 row-cols-xl-3 g-3">
                        <div v-for="sp in speakers" :key="sp.id" class="col">
                            <RouterLink :to="{ name: 'SpeakerDetails', params: { id: sp.id } }" class="card ep-speaker-card h-100 text-reset text-decoration-none">
                                <div class="card-body d-flex gap-3 align-items-center">
                                    <SpeakerAvatar :speaker="sp" size="lg" />
                                    <div class="min-w-0">
                                        <div class="fw-semibold text-truncate">{{ sp.$title }}</div>
                                        <div class="small text-muted text-truncate">{{ sp.jobTitle }}</div>
                                        <div class="small ep-accent-text text-truncate">{{ sp.company }}</div>
                                    </div>
                                </div>
                            </RouterLink>
                        </div>
                    </div>
                </section>
            </div>

            <!-- side column: registration -->
            <aside class="col-lg-4 order-1 order-lg-2">
                <div class="card ep-register-card shadow-sm">
                    <div class="card-body">
                        <div v-if="item.maxParticipants" class="mb-3">
                            <div class="d-flex justify-content-between small mb-1">
                                <span>{{ item.registrationCount ?? 0 }} {{ $t("registrationsCount") }}</span>
                                <span v-if="item.$spotsLeft">{{ item.$spotsLeft }} {{ $t("seatsLeft") }}</span>
                                <span v-else class="text-danger fw-semibold">{{ $t("full") }}</span>
                            </div>
                            <div class="progress" style="height: 0.5rem">
                                <div class="progress-bar" :style="{ width: item.$fillPercent + '%', background: item.$color }"></div>
                            </div>
                            <small v-if="item.waitlistCount" class="text-muted">{{ item.waitlistCount }} {{ $t("onWaitlist") }}</small>
                        </div>

                        <Feedback :feedback="feedback" />

                        <!-- already registered -->
                        <template v-if="myRegistration?.$isActive">
                            <div class="ep-registered" :class="`is-${myRegistration.status}`">
                                <i class="bi" :class="myRegistration.status === 'Waitlisted' ? 'bi-hourglass-split' : 'bi-check-circle-fill'"></i>
                                <span>{{ myRegistration.status === "Waitlisted" ? $t("waitlisted") : $t("registered") }}</span>
                            </div>
                            <p class="small text-muted mt-2 mb-2">
                                {{ myRegistration.sessions?.length ? `${$t("sessions")}: ${myRegistration.sessions.length}` : $t("noSessionsSelected") }}
                            </p>
                            <div class="d-grid gap-2">
                                <RouterLink :to="{ name: 'RegistrationDetails', params: { id: myRegistration.id } }" class="btn btn-outline-primary">
                                    <i class="bi bi-sliders me-1"></i>{{ $t("manageRegistration") }}
                                </RouterLink>
                                <ConfirmButton
                                    icon="bi bi-x-circle"
                                    class="btn btn-outline-danger"
                                    :button-label="$t('cancelRegistration')"
                                    :modal-type="ModalType.danger"
                                    :modal-title="$t('cancelRegistration')"
                                    :modal-labels="{ cancel: $t('cancel'), submit: $t('cancelRegistration') }"
                                    @confirm="cancelMine"
                                >
                                    {{ item.title }}
                                </ConfirmButton>
                            </div>
                        </template>

                        <!-- open for registration -->
                        <template v-else-if="item.$isOpen">
                            <h5 class="mb-2">{{ $t("registerNow") }}</h5>
                            <SessionPicker v-model="picked" :event-id="item.id" />
                            <textarea v-model="notes" class="form-control my-2" rows="2" maxlength="1000" :placeholder="$t('notes')"></textarea>
                            <button type="button" class="btn ep-btn-register w-100" :disabled="feedback.isPending" @click="register">
                                <i class="bi bi-ticket-perforated me-1"></i>{{ item.$spotsLeft === 0 ? $t("waitlisted") : $t("register") }}
                            </button>
                        </template>

                        <p v-else class="mb-0 text-muted">
                            <i class="bi bi-info-circle me-1"></i>
                            {{ item.status === "Cancelled" ? $t("eventCancelled") : item.$isPast ? $t("eventEnded") : $t("notOpen") }}
                        </p>
                    </div>
                </div>

                <div v-if="item.location" class="card mt-3">
                    <div class="card-body">
                        <h6 class="card-title"><i class="bi bi-geo-alt me-1"></i>{{ item.location.title }}</h6>
                        <div class="small text-muted">{{ item.location.address }}</div>
                        <div class="small text-muted">{{ item.location.city }} {{ item.location.country }}</div>
                    </div>
                </div>

                <RouterLink v-if="canEdit" :to="{ name: 'EventItemForm', params: { id: item.$id }, hash: '#participants' }" class="btn btn-outline-secondary w-100 mt-3">
                    <i class="bi bi-people me-1"></i>{{ $t("participants") }}
                </RouterLink>
            </aside>
        </div>
    </article>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, useFeedback, toFeedbackError, ConfirmButton, ModalType } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { onAuthenticated } from "@regira/modules/vue/auth"
import { SpeakerAvatar, useEntityStore as useSpeakerStore, type Entity as Speaker } from "@/entities/speakers"
import useRegistrationStore from "@/entities/registrations/data/store"
import Registration, { RegistrationStatus, type RegistrationSession } from "@/entities/registrations/data/Entity"
import SessionPicker from "@/entities/registrations/details/SessionPicker.vue"
import { useAccess } from "@/access"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import type Session from "../sessions/Entity"

const props = defineProps<{ modelValue: Entity; overviewUrl?: string | RouteRecordRaw; readonly?: boolean }>()
const emit = defineEmits<{ (e: "update:modelValue", value: Entity): void }>()
const item = computed(() => props.modelValue)

const { canWrite } = useAccess()
const canEdit = computed(() => canWrite("EventItem"))
const { translate } = useLang()

const paragraphs = computed(() => (item.value.description ?? "").split(/\n+/).filter((p) => p.trim()))

// agenda grouped per day
const days = computed(() => {
    const map = new Map<string, Array<Session>>()
    const sorted = [...(item.value.sessions ?? [])].sort((a, b) => (a.startTime?.getTime() ?? 0) - (b.startTime?.getTime() ?? 0))
    for (const s of sorted) map.set(s.$dayKey, [...(map.get(s.$dayKey) ?? []), s])
    return [...map.entries()].map(([key, sessions]) => ({
        key,
        label: sessions[0]?.startTime?.toLocaleDateString(undefined, { weekday: "long", day: "numeric", month: "long" }) ?? key,
        sessions,
    }))
})

// speakers: nested DTOs rehydrated through the speaker pool
const { fromPool } = useSpeakerStore()
const speaker = (s?: Partial<Speaker>) => fromPool(s as Speaker)
const speakers = computed(() => {
    const seen = new Map<number, Speaker>()
    for (const s of item.value.sessions ?? []) for (const link of s.speakers ?? []) if (link.speaker && !seen.has(link.speakerId)) seen.set(link.speakerId, speaker(link.speaker)!)
    return [...seen.values()]
})

// my registration (the API tells which one is mine: myRegistrationId)
const { service: registrationService } = useRegistrationStore()
const { service: eventService } = useEntityStore()
const myRegistration = ref<Registration>()
const mySessionIds = computed(() => new Set((myRegistration.value?.sessions ?? []).map((s) => s.sessionId)))
async function loadMine() {
    myRegistration.value = item.value.myRegistrationId ? await registrationService.details(item.value.myRegistrationId) : undefined
}
watch(() => item.value.myRegistrationId, loadMine)
onAuthenticated(loadMine)

async function reloadEvent() {
    const fresh = await eventService.details(item.value.id)
    if (fresh) emit("update:modelValue", fresh)
}

const feedback = useFeedback()
const picked = ref<Array<RegistrationSession>>([])
const notes = ref<string>()
async function register() {
    feedback.pending(translate("register"))
    try {
        const sessions = picked.value.filter((x) => !x._deleted)
        // a cancelled registration of mine is re-activated (one registration per employee per event)
        const reg = myRegistration.value
            ? Object.assign(myRegistration.value, { status: RegistrationStatus.Confirmed, notes: notes.value ?? myRegistration.value.notes, sessions })
            : Object.assign(new Registration(), { eventId: item.value.id, notes: notes.value, sessions })
        const result = await registrationService.save(reg)
        feedback.success(translate("registrationSaved"))
        picked.value = []
        notes.value = undefined
        myRegistration.value = result?.saved as Registration | undefined
        await reloadEvent()
    } catch (ex) {
        console.error(ex)
        feedback.fail(translate("registrationFailed"), toFeedbackError(ex))
    }
}
async function cancelMine() {
    if (!myRegistration.value) return
    feedback.pending(translate("cancelRegistration"))
    try {
        myRegistration.value.status = RegistrationStatus.Cancelled
        await registrationService.save(myRegistration.value)
        feedback.success(translate("registrationCancelled"))
        await reloadEvent()
    } catch (ex) {
        console.error(ex)
        feedback.fail(translate("registrationFailed"), toFeedbackError(ex))
    }
}
</script>
