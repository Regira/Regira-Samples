<!-- Approval queue: reservations with at least one room awaiting a facility manager's decision. -->
<script setup lang="ts">
import { onMounted, reactive, ref } from "vue"
import { RouterLink } from "vue-router"
import { LoadingContainer, Feedback, Paging, ResultSummary, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { PagingInfo } from "@regira/modules/vue/entities"
import { get } from "@regira/modules/vue/ioc"
import { useLang } from "@regira/modules/vue/lang"
import { Entity as ReservationEntity, EntityService, useEntityStore as useReservationStore, type Entity as Reservation } from "@/entities/reservations"
import { useEntityStore as useRoomStore, type Entity as Room } from "@/entities/rooms"
import StatusBadge from "@/components/planning/StatusBadge.vue"
import { formatDay, formatDuration, formatTime, RoomApprovalStatus } from "@/utilities/planning"

const { translate } = useLang()
const feedback = useFeedback()
const store = useReservationStore()
const { fromPool: poolRoom } = useRoomStore()
const getRoom = (x?: Partial<Room>) => (x ? poolRoom(x as Room) : undefined)

const items = ref<Array<Reservation>>([])
const count = ref(0)
const pagingInfo = ref(new PagingInfo(10, 1))
const includePast = ref(false)
const isLoading = ref(false)
const notes = reactive<Record<string, string>>({})

async function load() {
    isLoading.value = true
    try {
        const result = await store.service.search({
            awaitingApproval: true,
            from: includePast.value ? undefined : new Date(),
            includes: ["All"],
            sortBy: ["Start"],
            pageSize: pagingInfo.value.pageSize,
            page: pagingInfo.value.page,
        })
        items.value = result.items
        count.value = result.count ?? 0
    } finally {
        isLoading.value = false
    }
}
onMounted(load)

async function decide(r: Reservation, roomId: number, approve: boolean) {
    const key = `${r.id}-${roomId}`
    feedback.pending(translate("saving"))
    try {
        const svc = get<EntityService>(ReservationEntity.name)!
        const updated = approve ? await svc.approveRoom(r.id, roomId, notes[key]) : await svc.rejectRoom(r.id, roomId, notes[key])
        store.set(updated)
        Object.assign(r, { status: updated.status, rooms: updated.rooms })
        feedback.success(`${r.title}: ${translate(approve ? "approved" : "rejected")}`)
        if (!r.rooms?.some((x) => x.approvalStatus === RoomApprovalStatus.Pending)) {
            items.value = items.value.filter((x) => x.id !== r.id)
            count.value--
        }
    } catch (ex) {
        console.error(ex)
        feedback.fail(translate("actionFailed"), toFeedbackError(ex))
    }
}
</script>

<template>
    <section class="approvals">
        <div class="d-flex flex-wrap align-items-center gap-3 mb-3">
            <h2 class="h4 mb-0"><i class="bi bi-shield-check me-2 text-primary"></i>{{ $t("approvals") }}</h2>
            <span class="badge text-bg-warning fs-6">{{ count }}</span>
            <div class="form-check form-switch mb-0 ms-auto">
                <input id="appr-past" v-model="includePast" type="checkbox" class="form-check-input" @change="load" />
                <label for="appr-past" class="form-check-label">{{ $t("includePast") }}</label>
            </div>
        </div>
        <Feedback :feedback="feedback" />
        <LoadingContainer :is-loading="isLoading">
            <div v-for="r in items" :key="r.id" class="card mb-3">
                <div class="card-header d-flex flex-wrap align-items-center gap-2">
                    <RouterLink :to="{ name: 'ReservationDetails', params: { id: r.id } }" class="fw-semibold">{{ r.title }}</RouterLink>
                    <StatusBadge :status="r.status" />
                    <span class="small text-muted ms-auto">
                        <i class="bi bi-clock me-1"></i>{{ formatDay(r.start) }} {{ formatTime(r.start) }}–{{ formatTime(r.end) }} ({{ formatDuration(r.start, r.end) }})
                    </span>
                </div>
                <div class="card-body py-2">
                    <div class="small mb-2">
                        <i class="bi bi-person me-1"></i>{{ r.organizer?.title }} <span class="text-muted">· {{ r.organizer?.department }}</span>
                        <span class="ms-3"><i class="bi bi-people me-1"></i>{{ r.attendeeCount + 1 }} {{ $t("people") }}</span>
                    </div>
                    <div v-for="row in r.rooms" :key="row.roomId" class="row g-2 align-items-center border-top py-2">
                        <div class="col-12 col-md-4 text-truncate">
                            <span class="room-dot me-2" :style="{ backgroundColor: getRoom(row.room)?.$color }"></span>
                            <RouterLink :to="{ name: 'RoomDetails', params: { id: row.roomId } }">{{ getRoom(row.room)?.$title }}</RouterLink>
                            <small class="text-muted ms-1">{{ getRoom(row.room)?.$location }} · <i class="bi bi-people"></i> {{ getRoom(row.room)?.capacity }}</small>
                        </div>
                        <div class="col-auto"><StatusBadge :status="row.approvalStatus" /></div>
                        <template v-if="row.approvalStatus === 'Pending'">
                            <div class="col"><input v-model="notes[`${r.id}-${row.roomId}`]" class="form-control form-control-sm" :placeholder="$t('note')" maxlength="512" /></div>
                            <div class="col-auto d-flex gap-1">
                                <button type="button" class="btn btn-sm btn-success" :disabled="feedback.isPending" @click="decide(r, row.roomId, true)"><i class="bi bi-check-lg me-1"></i>{{ $t("approve") }}</button>
                                <button type="button" class="btn btn-sm btn-danger" :disabled="feedback.isPending" @click="decide(r, row.roomId, false)"><i class="bi bi-x-lg me-1"></i>{{ $t("reject") }}</button>
                            </div>
                        </template>
                        <div v-else class="col small text-muted text-truncate">{{ row.decisionNote }}</div>
                    </div>
                </div>
            </div>
            <p v-if="!items.length" class="alert alert-success"><i class="bi bi-check2-all me-1"></i>{{ $t("nothingToApprove") }}</p>
            <div class="d-flex justify-content-between align-items-center">
                <Paging v-if="count > pagingInfo.pageSize!" v-model="pagingInfo" :count="count" @change="load" />
                <ResultSummary v-if="items.length" :visible-count="items.length" :total-count="count" />
            </div>
        </LoadingContainer>
    </section>
</template>
