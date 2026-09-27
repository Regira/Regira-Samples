<template>
    <form @submit.prevent="handleSubmit">
        <div class="row form-toolbar align-items-center mb-3">
            <div class="col col-md-auto order-1 d-flex flex-wrap gap-2 align-items-center">
                <FormButtonsRow
                    :item="item"
                    :readonly="isReadonly"
                    :feedback="feedback"
                    :show-delete="item?.id > 0"
                    :labels="{ save: $t('save'), cancel: $t('cancel'), delete: $t('delete'), restore: $t('restore') }"
                    :modal-title="$t('delete')"
                    @cancel="handleCancel"
                    @remove="handleRemove"
                    @restore="handleRestore"
                >
                    <template #delete>{{ $t("deleteItem", { title: item?.$title }) }}</template>
                </FormButtonsRow>
                <ConfirmButton
                    v-if="item.id && !item.$isCancelled && !readonly"
                    icon="bi bi-calendar-x"
                    :button-label="$t('cancelReservation')"
                    :modal-type="ModalType.warning"
                    :modal-title="$t('cancelReservation')"
                    :modal-labels="{ cancel: $t('close'), submit: $t('cancelReservation') }"
                    class="btn-outline-warning"
                    @confirm="handleCancelReservation"
                >
                    <p>{{ $t("cancelReservationConfirm", { title: item.$title }) }}</p>
                    <input v-model="cancelReason" class="form-control" :placeholder="$t('reason')" maxlength="512" />
                </ConfirmButton>
            </div>
            <div class="col-auto order-2 order-md-3">
                <RouterLink v-if="isPopup" :to="{ name: `${config.key}Details`, params: { id: item.$id } }" target="_blank" class="btn btn-outline-secondary" :title="$t('popOut')">
                    <Icon name="popOut" />
                </RouterLink>
                <RouterLink v-else-if="overviewUrl" :to="overviewUrl" class="btn btn-outline-info">
                    <Icon name="list" /> <span class="d-none d-md-inline ms-1">{{ $t("overview") }}</span>
                </RouterLink>
            </div>
            <div class="col-md order-3 order-md-2">
                <Feedback :feedback="feedback" />
                <Feedback :feedback="actionFeedback" />
            </div>
        </div>

        <!-- status banner -->
        <div v-if="item.id" class="alert d-flex flex-wrap align-items-center gap-2 py-2" :class="`alert-${statusVariant[item.status]}`">
            <StatusBadge :status="item.status" />
            <span v-if="item.$isCancelled">
                {{ $t("cancelledOn") }} {{ formatDay(item.cancelledOn) }} {{ formatTime(item.cancelledOn) }}<template v-if="item.cancelReason"> — {{ item.cancelReason }}</template>
            </span>
            <span v-else-if="item.status === 'Pending'">{{ $t("pendingInfo") }}</span>
            <span v-else-if="item.status === 'Rejected' || item.status === 'PartiallyApproved'">{{ $t("rejectedInfo") }}</span>
            <span class="ms-auto small">{{ item.$when }} · {{ formatDuration(item.start, item.end) }}</span>
        </div>

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t('meeting')" :readonly="isReadonly">
                    <div class="mb-2">
                        <input v-model="item.title" :readonly="isReadonly" class="form-control" required maxlength="128" />
                        <FormLabel :label="$t('subject')" />
                    </div>
                    <div class="mb-2">
                        <EmployeeInputSelector v-model="item.organizer" v-model:idValue="item.organizerId" :readonly="isReadonly" :filter-defaults="{ isActive: true }" />
                        <FormLabel :label="$t('organizer')" />
                    </div>
                    <div class="row g-2">
                        <div class="col-md-5 mb-2">
                            <DateInput :model-value="item.start" :show-time="true" :readonly="isReadonly" @update:modelValue="handleStartChange" />
                            <FormLabel :label="$t('start')" />
                        </div>
                        <div class="col-md-5 mb-2">
                            <DateInput v-model="item.end" :show-time="true" :readonly="isReadonly" />
                            <FormLabel :label="$t('end')" />
                        </div>
                        <div class="col-md-2 mb-2">
                            <div class="form-control-plaintext fw-semibold"><i class="bi bi-stopwatch me-1"></i>{{ formatDuration(item.start, item.end) }}</div>
                        </div>
                    </div>
                    <div v-if="!isReadonly" class="mb-3 d-flex flex-wrap gap-1">
                        <button v-for="d in durations" :key="d" type="button" class="btn btn-sm btn-outline-secondary" @click="setDuration(d)">{{ d < 60 ? `${d}m` : `${d / 60}h` }}</button>
                    </div>
                    <div class="mb-2">
                        <textarea v-model="item.description" :readonly="isReadonly" class="form-control" rows="3" maxlength="2048"></textarea>
                        <FormLabel :label="$t('agendaNotes')" />
                    </div>
                </FormSection>

                <FormSection :title="$t('rooms')" :readonly="isReadonly">
                    <ReservationRoomOverview v-model="item.rooms" :readonly="isReadonly" />
                    <div class="mt-2 d-flex flex-wrap gap-3 small">
                        <span :class="capacityOk ? 'text-success' : 'text-danger'">
                            <i class="bi bi-people me-1"></i>{{ peopleCount }} / {{ totalCapacity }} {{ $t("seats") }}
                        </span>
                        <span v-if="needsApproval" class="text-warning"><i class="bi bi-shield-lock me-1"></i>{{ $t("approvalNeededInfo") }}</span>
                    </div>
                    <div v-if="selectedRooms.length && item.start && item.end" class="mt-3">
                        <h6 class="text-muted"><i class="bi bi-calendar3-range me-1"></i>{{ $t("availabilityOn") }} {{ formatDay(item.start) }}</h6>
                        <RoomDayAvailability :rooms="selectedRooms" :day="item.start" :highlight="{ start: item.start, end: item.end }" :exclude-id="item.id" @select-slot="handleSelectSlot" />
                    </div>
                </FormSection>
            </template>
            <template #attendees>
                <FormSection :title="$t('attendees')" :readonly="isReadonly">
                    <ReservationAttendeeOverview v-model="item.attendees" :readonly="isReadonly" :organizer-id="item.organizerId" />
                </FormSection>
            </template>
            <template #approval>
                <FormSection :title="$t('approval')">
                    <div v-for="row in item.rooms?.filter((r) => r.id && r.id > 0)" :key="row.roomId" class="row g-2 align-items-center border-bottom py-2">
                        <div class="col-12 col-md-4 text-truncate">
                            <span class="room-dot me-2" :style="{ backgroundColor: getRoom(row.room)?.$color }"></span>{{ getRoom(row.room)?.$title }}
                            <i v-if="getRoom(row.room)?.requiresApproval" class="bi bi-shield-lock text-warning ms-1" :title="$t('requiresApproval')"></i>
                        </div>
                        <div class="col-auto"><StatusBadge :status="row.approvalStatus" /></div>
                        <div class="col small text-muted text-truncate">
                            <template v-if="row.decidedOn">{{ formatDay(new Date(row.decidedOn)) }} {{ formatTime(new Date(row.decidedOn)) }}</template>
                            <template v-if="row.decisionNote"> — {{ row.decisionNote }}</template>
                        </div>
                        <div v-if="!item.$isCancelled && !readonly" class="col-12 col-lg-auto d-flex gap-1">
                            <input v-model="notes[row.roomId]" class="form-control form-control-sm" :placeholder="$t('note')" maxlength="512" />
                            <button type="button" class="btn btn-sm btn-success text-nowrap" :disabled="row.approvalStatus === 'Approved' || actionFeedback.isPending" @click="decide(row.roomId, true)">
                                <i class="bi bi-check-lg"></i><span class="d-none d-md-inline ms-1">{{ $t("approve") }}</span>
                            </button>
                            <button type="button" class="btn btn-sm btn-danger text-nowrap" :disabled="row.approvalStatus === 'Rejected' || actionFeedback.isPending" @click="decide(row.roomId, false)">
                                <i class="bi bi-x-lg"></i><span class="d-none d-md-inline ms-1">{{ $t("reject") }}</span>
                            </button>
                        </div>
                    </div>
                </FormSection>
            </template>
        </TabContainer>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue"
import { RouterLink, useRoute, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab, DateInput, ConfirmButton, ModalType, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { get } from "@regira/modules/vue/ioc"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import type EntityService from "../data/EntityService"
import useEntityStore from "../data/store"
import { ReservationRoomOverview } from "../reservation-rooms"
import { ReservationAttendeeOverview } from "../reservation-attendees"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import { useEntityStore as useRoomStore, type Entity as Room } from "@/entities/rooms"
import StatusBadge from "@/components/planning/StatusBadge.vue"
import RoomDayAvailability from "@/components/planning/RoomDayAvailability.vue"
import { addMinutes, formatDay, formatDuration, formatTime, nextQuarter, statusVariant, MINUTE } from "@/utilities/planning"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const store = useEntityStore()
const { service: entityService } = store
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
const isReadonly = computed(() => props.readonly || item.value.$isCancelled)

// --- rooms / capacity ---
const { fromPool: poolRoom, service: roomService } = useRoomStore()
const getRoom = (x?: Partial<Room>) => (x ? poolRoom(x as Room) : undefined)
const selectedRooms = computed(() => (item.value.rooms ?? []).filter((r) => !r._deleted && r.room).map((r) => getRoom(r.room)!))
const totalCapacity = computed(() => selectedRooms.value.reduce((sum, r) => sum + (r.capacity ?? 0), 0))
const peopleCount = computed(() => (item.value.attendees?.filter((a) => !a._deleted).length ?? 0) + 1)
const capacityOk = computed(() => !selectedRooms.value.length || peopleCount.value <= totalCapacity.value)
const needsApproval = computed(() => selectedRooms.value.some((r) => r.requiresApproval))

// --- schedule helpers ---
const durations = [30, 60, 90, 120, 240]
function setDuration(minutes: number) {
    if (!item.value.start) item.value.start = nextQuarter()
    item.value.end = addMinutes(item.value.start, minutes)
}
function handleStartChange(value?: Date | string) {
    // moving the start keeps the duration (the model still holds the previous start here)
    if (!(value instanceof Date)) return
    const previous = item.value.start && item.value.end ? item.value.end.getTime() - item.value.start.getTime() : 60 * MINUTE
    item.value.start = value
    item.value.end = new Date(value.getTime() + Math.max(previous, 15 * MINUTE))
}
function handleSelectSlot({ start }: { roomId: number; start: Date }) {
    if (isReadonly.value) return
    const duration = item.value.start && item.value.end ? item.value.end.getTime() - item.value.start.getTime() : 60 * MINUTE
    item.value.start = start
    item.value.end = new Date(start.getTime() + duration)
}

// --- tabs ---
const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { icon: "calendar", title: translate("reservation"), isDefault: true }),
    Tab.create("attendees", { icon: "people", title: `${translate("attendees")} (${item.value.attendees?.filter((a) => !a._deleted).length ?? 0})` }),
    item.value.id ? Tab.create("approval", { icon: "bi bi-shield-check", title: translate("approval") }) : undefined,
])

// --- prefill a new reservation from the route (planner / room finder / room page links) ---
const route = useRoute()
onMounted(async () => {
    if (item.value.id) return
    const q = route.query
    const start = typeof q.start === "string" ? new Date(q.start) : undefined
    const end = typeof q.end === "string" ? new Date(q.end) : undefined
    item.value.start = start && !isNaN(start.getTime()) ? start : nextQuarter()
    item.value.end = end && !isNaN(end.getTime()) ? end : addMinutes(item.value.start, 60)
    if (typeof q.organizerId === "string") item.value.organizerId = parseInt(q.organizerId, 10)
    const roomIds = ([] as Array<unknown>).concat(q.roomId ?? []).map((x) => parseInt(String(x), 10)).filter((x) => !isNaN(x))
    if (roomIds.length && !item.value.rooms?.length) {
        const rooms = await roomService.list({ ids: roomIds, pageSize: 0 })
        item.value.rooms = rooms.map((r) => ({ roomId: r.id, room: r }))
    }
})

// --- workflow actions (custom endpoints on the raw service) ---
const actionFeedback = useFeedback()
const notes = reactive<Record<number, string>>({})
const cancelReason = ref<string>()
const rawService = () => get<EntityService>(Entity.name)!
function apply(updated: Entity) {
    store.set(updated) // write-through the pool, so every list shows the new state
    Object.assign(item.value, { status: updated.status, rooms: updated.rooms, cancelledOn: updated.cancelledOn, cancelReason: updated.cancelReason, lastModified: updated.lastModified })
}
async function decide(roomId: number, approve: boolean) {
    actionFeedback.pending(translate("saving"))
    try {
        const svc = rawService()
        const updated = approve ? await svc.approveRoom(item.value.id, roomId, notes[roomId]) : await svc.rejectRoom(item.value.id, roomId, notes[roomId])
        apply(updated)
        notes[roomId] = ""
        actionFeedback.success(translate(approve ? "approved" : "rejected"))
    } catch (ex) {
        console.error(ex)
        actionFeedback.fail(translate("actionFailed"), toFeedbackError(ex))
    }
}
async function handleCancelReservation() {
    actionFeedback.pending(translate("saving"))
    try {
        apply(await rawService().cancel(item.value.id, cancelReason.value))
        actionFeedback.success(translate("Cancelled"))
    } catch (ex) {
        console.error(ex)
        actionFeedback.fail(translate("actionFailed"), toFeedbackError(ex))
    }
}
</script>
