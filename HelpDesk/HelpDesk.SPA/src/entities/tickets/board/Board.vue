<!-- Kanban board: tickets grouped into columns by Status, drag-and-drop between columns to change status.
     Reads through the pooled Ticket store (so a drag-drop update is reflected everywhere else too);
     the status change itself is a narrow PATCH (not a full save()) via the shared axios instance. -->
<template>
    <div class="board-view">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h1 class="h4 mb-0">{{ $t("kanban") }}</h1>
            <div class="d-flex align-items-center gap-2">
                <SupportTeamInputSelector v-model="filterTeam" v-model:idValue="filterTeamId" :canEdit="false" :placeholder="$t('supportTeam')" />
                <RouterLink :to="{ name: 'TicketOverview' }" class="btn btn-outline-secondary btn-sm">
                    <Icon name="list" /> {{ $t("list") }}
                </RouterLink>
            </div>
        </div>

        <LoadingContainer :is-loading="loading">
            <div class="board-columns">
                <div v-for="status in statuses" :key="status.id" class="board-column" @dragover.prevent @drop="handleDrop(status.id)">
                    <div class="board-column-header" :style="{ borderColor: status.colorHex }">
                        <span>{{ status.title }}</span>
                        <span class="badge text-bg-secondary">{{ columnTickets(status.id).length }}</span>
                    </div>
                    <div class="board-column-body">
                        <div
                            v-for="ticket in columnTickets(status.id)"
                            :key="ticket.id"
                            class="board-card"
                            draggable="true"
                            @dragstart="handleDragStart(ticket)"
                        >
                            <RouterLink :to="{ name: 'TicketDetails', params: { id: ticket.id } }" class="board-card-title">
                                {{ ticket.title }}
                            </RouterLink>
                            <div class="d-flex justify-content-between align-items-center mt-1">
                                <span class="badge" :style="{ backgroundColor: ticket.priority?.colorHex || '#6c757d' }">{{
                                    ticket.priority?.title
                                }}</span>
                                <small class="text-muted text-truncate ms-1">{{ ticket.assignedEmployee?.fullName || $t("unassigned") }}</small>
                            </div>
                            <div class="text-muted small text-truncate mt-1">{{ ticket.customer?.fullName }}</div>
                        </div>
                        <p v-if="!columnTickets(status.id).length" class="text-muted small">{{ $t("noResults") }}</p>
                    </div>
                </div>
            </div>
        </LoadingContainer>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { RouterLink } from "vue-router"
import { Icon, LoadingContainer } from "@regira/modules/vue/ui"
import { useAxios } from "@regira/modules/vue/http"
import useTicketStore from "../data/store"
import type Ticket from "../data/Entity"
import { InputSelector as SupportTeamInputSelector, type Entity as SupportTeam } from "@/entities/support-teams"
import useStatusStore from "@/entities/statuses/data/store"
import type Status from "@/entities/statuses/data/Entity"

const { service } = useTicketStore()
const { list: listStatuses } = useStatusStore()
const axios = useAxios()

const statuses = ref<Array<Status>>([])
const tickets = ref<Array<Ticket>>([])
const loading = ref(false)
const filterTeam = ref<SupportTeam>()
const filterTeamId = ref<number>()

const dragged = ref<Ticket>()

const columnTickets = computed(() => (statusId: number) => tickets.value.filter((t) => t.statusId === statusId))

async function load() {
    loading.value = true
    try {
        statuses.value = await listStatuses() // pre-sorted by SortOrder server-side
        const so: Record<string, any> = { pageSize: 0 }
        if (filterTeamId.value) so.supportTeamId = filterTeamId.value
        const result = await service.search(so)
        tickets.value = result.items
    } finally {
        loading.value = false
    }
}

function handleDragStart(ticket: Ticket) {
    dragged.value = ticket
}

async function handleDrop(statusId: number) {
    const ticket = dragged.value
    dragged.value = undefined
    if (!ticket || ticket.statusId === statusId) return

    const previous = ticket.statusId
    ticket.statusId = statusId // optimistic move
    try {
        await axios.patch(`/tickets/${ticket.id}`, { statusId })
    } catch {
        ticket.statusId = previous // revert on failure
    }
}

onMounted(load)
watch(filterTeamId, load)
</script>

<style scoped>
.board-columns {
    display: flex;
    gap: 1rem;
    overflow-x: auto;
    padding-bottom: 1rem;
}
.board-column {
    flex: 0 0 16rem;
    background-color: var(--bs-tertiary-bg, #f5f5f5);
    border-radius: 0.5rem;
    display: flex;
    flex-direction: column;
    max-height: 75vh;
}
.board-column-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.5rem 0.75rem;
    font-weight: 600;
    border-top: 3px solid #6c757d;
    border-radius: 0.5rem 0.5rem 0 0;
    background-color: var(--bs-body-bg, #fff);
}
.board-column-body {
    padding: 0.5rem;
    overflow-y: auto;
    flex: 1 1 auto;
}
.board-card {
    background-color: var(--bs-body-bg, #fff);
    border-radius: 0.375rem;
    padding: 0.5rem 0.6rem;
    margin-bottom: 0.5rem;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
    cursor: grab;
}
.board-card-title {
    font-weight: 500;
    text-decoration: none;
    color: inherit;
    display: block;
}
.board-card-title:hover {
    text-decoration: underline;
}
</style>
