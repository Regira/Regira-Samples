<script setup lang="ts">
import { computed, reactive, ref, watch } from "vue"
import { onAuthenticated } from "@regira/modules/vue/auth"
import { Feedback, LoadingContainer, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { useEntityStore as useTicketStore, type Entity as Ticket } from "@/entities/tickets"
import { useEntityStore as useStatusStore, type Entity as Status } from "@/entities/statuses"
import { SelectorDropdown as SupportTeamSelectorDropdown } from "@/entities/support-teams"
import { SelectorDropdown as PrioritySelectorDropdown } from "@/entities/priorities"
import ColorBadge from "@/components/tickets/ColorBadge.vue"
import SlaIndicator from "@/components/tickets/SlaIndicator.vue"
import PersonChip from "@/components/tickets/PersonChip.vue"

// Kanban: one column per status (ordered by SortOrder). Each column is its own server-side search, capped per
// column with the total shown; dropping a card PUTs the ticket with its new status through the pooled service.
const PER_COLUMN = 40
const CLOSED_PER_COLUMN = 15

const { translate } = useLang()
const feedback = useFeedback()
const ticketStore = useTicketStore()
const statusStore = useStatusStore()

const statuses = ref<Array<Status>>([])
const columns = reactive<Record<number, { items: Array<Ticket>; count: number; loading: boolean }>>({})
const filter = reactive({ q: "", supportTeamId: undefined as number | undefined, priorityId: undefined as number | undefined, mine: false })
const isLoading = ref(false)

function query(status: Status) {
    return {
        statusId: status.id,
        q: filter.q || undefined,
        supportTeamId: filter.supportTeamId,
        priorityId: filter.priorityId,
        assignedToMe: filter.mine || undefined,
        includes: ["Categories"],
        sortBy: status.isClosed ? ["LastActivity"] : ["Priority"],
        pageSize: status.isClosed ? CLOSED_PER_COLUMN : PER_COLUMN,
    }
}
async function loadColumn(status: Status) {
    if (!columns[status.id]) columns[status.id] = { items: [], count: 0, loading: false }
    const col = columns[status.id]! // read back through the reactive proxy (the ??= result is the raw object)
    col.loading = true
    try {
        const result = await ticketStore.service.search(query(status))
        col.items = ticketStore.fromPool(result.items)
        col.count = result.count ?? result.items.length
    } finally {
        col.loading = false
    }
}
async function load() {
    isLoading.value = true
    try {
        if (!statuses.value.length) statuses.value = statusStore.fromPool(await statusStore.list({ pageSize: 0 }))
        await Promise.all(statuses.value.map(loadColumn))
        feedback.reset()
    } catch (ex: any) {
        console.error("Loading the board failed", ex)
        feedback.fail(translate("loadFailed"), toFeedbackError(ex) ?? ex?.message)
    } finally {
        isLoading.value = false
    }
}
onAuthenticated(() => load())
watch(() => [filter.supportTeamId, filter.priorityId, filter.mine], () => load())

// ---- drag & drop (plus a "move to" select for touch screens) ----
const dragging = ref<{ ticket: Ticket; from: number }>()
const dropTarget = ref<number>()
function onDragStart(ticket: Ticket, from: number, ev: DragEvent) {
    dragging.value = { ticket, from }
    ev.dataTransfer?.setData("text/plain", String(ticket.id))
    if (ev.dataTransfer) ev.dataTransfer.effectAllowed = "move"
}
function onDragEnd() {
    dragging.value = undefined
    dropTarget.value = undefined
}
async function onDrop(status: Status) {
    const d = dragging.value
    onDragEnd()
    if (d && d.from !== status.id) await move(d.ticket, d.from, status)
}
async function move(ticket: Ticket, fromId: number, to: Status) {
    const from = columns[fromId]
    const target = columns[to.id]
    if (!from || !target) return
    const previous = { statusId: ticket.statusId, status: ticket.status }
    // optimistic move
    from.items = from.items.filter((t) => t.id !== ticket.id)
    from.count--
    ticket.statusId = to.id
    ticket.status = to
    target.items = [ticket, ...target.items]
    target.count++
    feedback.pending(translate("saving"))
    try {
        await ticketStore.service.save(ticket)
        feedback.success(translate("movedTo", { code: ticket.code ?? "", status: to.title }))
    } catch (ex: any) {
        console.error("Moving the ticket failed", ex)
        ticket.statusId = previous.statusId
        ticket.status = previous.status
        await Promise.all([loadColumn(statuses.value.find((s) => s.id === fromId)!), loadColumn(to)])
        feedback.fail(translate("moveFailed"), toFeedbackError(ex) ?? ex?.message)
    }
}
function onSelectMove(ticket: Ticket, fromId: number, ev: Event) {
    const id = Number((ev.target as HTMLSelectElement).value)
    const to = statuses.value.find((s) => s.id === id)
    if (to && id !== fromId) move(ticket, fromId, to)
}

const totalOpen = computed(() => statuses.value.filter((s) => !s.isClosed).reduce((sum, s) => sum + (columns[s.id]?.count ?? 0), 0))
let timer: ReturnType<typeof setTimeout> | undefined
function onSearchInput() {
    clearTimeout(timer)
    timer = setTimeout(load, 350)
}
</script>

<template>
    <section>
        <div class="d-flex flex-wrap align-items-center gap-2 mb-3">
            <i class="bi bi-kanban fs-4 text-primary"></i>
            <h1 class="hd-page-title me-2">{{ $t("kanbanBoard") }}</h1>
            <span class="badge rounded-pill text-bg-light border">{{ totalOpen }} {{ $t("open") }}</span>
            <div class="flex-grow-1"></div>
            <router-link :to="{ name: 'TicketDetails', params: { id: 'new' } }" class="btn btn-primary btn-sm">
                <i class="bi bi-plus-lg me-1"></i>{{ $t("newTicket") }}
            </router-link>
        </div>

        <div class="hd-card p-2 mb-3">
            <div class="row g-2 align-items-center">
                <div class="col-12 col-md-4">
                    <div class="input-group input-group-sm">
                        <span class="input-group-text"><i class="bi bi-search"></i></span>
                        <input v-model.trim="filter.q" type="search" class="form-control" :placeholder="$t('ticketKeywords')" @input="onSearchInput" />
                    </div>
                </div>
                <div class="col-6 col-md-3">
                    <div class="input-group input-group-sm">
                        <span class="input-group-text" :title="$t('supportTeam')"><i class="bi bi-people me-1"></i><span class="d-none d-xl-inline">{{ $t("supportTeam") }}</span></span>
                        <SupportTeamSelectorDropdown v-model:idValue="filter.supportTeamId" class="form-select-sm" :title="$t('supportTeam')" />
                    </div>
                </div>
                <div class="col-6 col-md-3">
                    <div class="input-group input-group-sm">
                        <span class="input-group-text" :title="$t('priority')"><i class="bi bi-flag me-1"></i><span class="d-none d-xl-inline">{{ $t("priority") }}</span></span>
                        <PrioritySelectorDropdown v-model:idValue="filter.priorityId" class="form-select-sm" :title="$t('priority')" />
                    </div>
                </div>
                <div class="col-12 col-md-2">
                    <div class="form-check form-switch mb-0">
                        <input id="b-mine" v-model="filter.mine" class="form-check-input" type="checkbox" />
                        <label class="form-check-label small" for="b-mine">{{ $t("onlyMine") }}</label>
                    </div>
                </div>
            </div>
        </div>
        <Feedback :feedback="feedback" />

        <LoadingContainer :is-loading="isLoading && !statuses.length">
            <div class="hd-board">
                <div
                    v-for="s in statuses"
                    :key="s.id"
                    class="hd-column"
                    :class="{ 'hd-drop-target': dropTarget === s.id && dragging?.from !== s.id }"
                    :style="{ '--hd-col-color': s.color }"
                    @dragover.prevent="dropTarget = s.id"
                    @dragleave="dropTarget === s.id && (dropTarget = undefined)"
                    @drop.prevent="onDrop(s)"
                >
                    <div class="hd-column-header">
                        <span class="hd-dot" :style="{ backgroundColor: s.color }"></span>
                        <span class="text-truncate">{{ s.title }}</span>
                        <span class="badge rounded-pill text-bg-light border ms-auto">{{ columns[s.id]?.count ?? 0 }}</span>
                    </div>
                    <div class="hd-column-body">
                        <div
                            v-for="t in columns[s.id]?.items ?? []"
                            :key="t.id"
                            class="hd-ticket-card"
                            :class="{ 'hd-dragging': dragging?.ticket.id === t.id }"
                            :style="{ '--hd-prio-color': t.priority?.color }"
                            draggable="true"
                            @dragstart="onDragStart(t, s.id, $event)"
                            @dragend="onDragEnd"
                        >
                            <div class="d-flex justify-content-between align-items-center mb-1">
                                <span class="hd-code">{{ t.code }}</span>
                                <ColorBadge :title="t.priority?.title" :color="t.priority?.color" />
                            </div>
                            <router-link :to="{ name: 'TicketDetails', params: { id: t.id } }" class="hd-ticket-title">{{ t.title }}</router-link>
                            <div class="d-flex flex-wrap gap-1 my-1">
                                <ColorBadge v-for="c in t.categories ?? []" :key="c.categoryId" :title="c.category?.title" :color="c.category?.color" :icon="c.category?.icon" />
                            </div>
                            <div class="small text-muted text-truncate"><i class="bi bi-building me-1"></i>{{ t.customer?.company ?? t.customer?.fullName }}</div>
                            <div class="d-flex justify-content-between align-items-center mt-2 gap-2">
                                <PersonChip :person="t.assignedEmployee" :muted="$t('unassigned')" class="small" />
                                <span class="d-flex align-items-center gap-2">
                                    <span v-if="t.commentCount" class="small text-muted"><i class="bi bi-chat-dots me-1"></i>{{ t.commentCount }}</span>
                                    <SlaIndicator :due-date="t.dueDate" :closed-at="t.closedAt" :is-closed="s.isClosed" compact />
                                </span>
                            </div>
                            <select class="form-select form-select-sm mt-2 d-lg-none" :value="s.id" :aria-label="$t('moveTo')" @change="onSelectMove(t, s.id, $event)">
                                <option v-for="o in statuses" :key="o.id" :value="o.id">{{ $t("moveTo") }}: {{ o.title }}</option>
                            </select>
                        </div>
                        <p v-if="!columns[s.id]?.loading && !(columns[s.id]?.items.length)" class="text-muted small fst-italic text-center my-3">{{ $t("emptyColumn") }}</p>
                        <router-link
                            v-if="(columns[s.id]?.count ?? 0) > (columns[s.id]?.items.length ?? 0)"
                            :to="{ name: 'TicketOverview', query: { statusId: String(s.id) } }"
                            class="small text-center d-block"
                        >
                            {{ $t("showAll", { count: columns[s.id]?.count }) }}
                        </router-link>
                    </div>
                </div>
            </div>
        </LoadingContainer>
    </section>
</template>
