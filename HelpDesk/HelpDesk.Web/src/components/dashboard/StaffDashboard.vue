<script setup lang="ts">
import { computed, ref } from "vue"
import { useAxios } from "@regira/modules/vue/http"
import { onAuthenticated } from "@regira/modules/vue/auth"
import { Feedback, LoadingContainer, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { Dashboard } from "@/components/entity-navigation"
import { queues } from "@/components/queues"
import { useMeStore } from "@/infrastructure/me"

interface Bucket {
    id: number
    title: string
    color?: string
    count?: number
    open?: number
    overdue?: number
    isClosed?: boolean
}
interface DashboardData {
    total: number
    open: number
    unassigned: number
    overdue: number
    closedLast30: number
    avgFirstResponseHours?: number
    byStatus: Array<Bucket>
    byPriority: Array<Bucket>
    byTeam: Array<Bucket>
    series: Array<{ date: string; created: number; closed: number }>
}

// GET /dashboard is a plain custom endpoint (no slice, no pool): its own axios call + feedback
const data = ref<DashboardData>()
const isLoading = ref(false)
const feedback = useFeedback()
const { translate } = useLang()
const meStore = useMeStore()

async function load() {
    isLoading.value = true
    try {
        data.value = (await useAxios().get("/dashboard")).data.item
        feedback.reset()
    } catch (ex: any) {
        console.error("Loading the dashboard failed", ex)
        feedback.fail(translate("loadFailed"), toFeedbackError(ex) ?? ex?.message)
    } finally {
        isLoading.value = false
    }
}
onAuthenticated(() => load())

const kpis = computed(() => {
    const d = data.value
    if (!d) return []
    return [
        { key: "openTickets", value: d.open, icon: "bi bi-envelope-open", color: "#2563eb", query: queues.find((q) => q.key === "queueOpen")!.query },
        { key: "unassigned", value: d.unassigned, icon: "bi bi-inbox", color: "#7c3aed", query: queues.find((q) => q.key === "queueUnassigned")!.query },
        { key: "overdue", value: d.overdue, icon: "bi bi-alarm", color: "#dc2626", query: queues.find((q) => q.key === "queueOverdue")!.query },
        { key: "closedLast30", value: d.closedLast30, icon: "bi bi-check2-circle", color: "#16a34a", query: { isClosed: "true", sortBy: "LastActivity" } },
    ]
})
const maxStatus = computed(() => Math.max(1, ...(data.value?.byStatus.map((s) => s.count ?? 0) ?? [1])))
const maxPriority = computed(() => Math.max(1, ...(data.value?.byPriority.map((s) => s.count ?? 0) ?? [1])))
const maxSeries = computed(() => Math.max(1, ...(data.value?.series.map((s) => Math.max(s.created, s.closed)) ?? [1])))
const greeting = computed(() => meStore.me?.person?.givenName)
</script>

<template>
    <section>
        <div class="d-flex flex-wrap align-items-end justify-content-between gap-2 mb-3">
            <div>
                <h1 class="hd-page-title">{{ $t("dashboard") }}</h1>
                <div class="text-muted small">{{ greeting ? $t("welcomeBack", { name: greeting }) : $t("supportCenter") }}</div>
            </div>
            <div class="d-flex gap-2">
                <router-link :to="{ name: 'board' }" class="btn btn-outline-primary btn-sm"><i class="bi bi-kanban me-1"></i>{{ $t("kanbanBoard") }}</router-link>
                <router-link :to="{ name: 'TicketDetails', params: { id: 'new' } }" class="btn btn-primary btn-sm"><i class="bi bi-plus-lg me-1"></i>{{ $t("newTicket") }}</router-link>
            </div>
        </div>
        <Feedback :feedback="feedback" />

        <LoadingContainer :is-loading="isLoading && !data">
            <template v-if="data">
                <div class="row g-3 mb-3">
                    <div v-for="k in kpis" :key="k.key" class="col-6 col-xl-3">
                        <router-link :to="{ name: 'TicketOverview', query: k.query }" class="hd-card hd-kpi d-flex align-items-center gap-3 text-decoration-none text-body h-100">
                            <span class="hd-kpi-icon" :style="{ backgroundColor: k.color + '1a', color: k.color }"><i :class="k.icon"></i></span>
                            <span>
                                <span class="hd-kpi-value d-block">{{ k.value }}</span>
                                <span class="hd-kpi-label">{{ $t(k.key) }}</span>
                            </span>
                        </router-link>
                    </div>
                </div>

                <div class="row g-3 mb-3">
                    <div class="col-lg-8">
                        <div class="hd-card p-3 h-100">
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <h2 class="h6 mb-0">{{ $t("last30Days") }}</h2>
                                <div class="small text-muted">
                                    <span class="hd-dot me-1" style="background: #2563eb"></span>{{ $t("created") }}
                                    <span class="hd-dot ms-3 me-1" style="background: #16a34a"></span>{{ $t("closed") }}
                                </div>
                            </div>
                            <svg viewBox="0 0 300 110" class="w-100" style="height: 190px" preserveAspectRatio="none" role="img" :aria-label="$t('last30Days')">
                                <line x1="0" y1="100" x2="300" y2="100" stroke="#e2e8f0" />
                                <g v-for="(p, i) in data.series" :key="p.date">
                                    <rect :x="i * 10 + 1" :y="100 - (p.created / maxSeries) * 95" width="4" :height="(p.created / maxSeries) * 95" fill="#2563eb" rx="1">
                                        <title>{{ p.date }}: {{ p.created }} {{ $t("created") }}</title>
                                    </rect>
                                    <rect :x="i * 10 + 5" :y="100 - (p.closed / maxSeries) * 95" width="4" :height="(p.closed / maxSeries) * 95" fill="#16a34a" rx="1">
                                        <title>{{ p.date }}: {{ p.closed }} {{ $t("closed") }}</title>
                                    </rect>
                                </g>
                            </svg>
                            <div class="d-flex justify-content-between small text-muted">
                                <span>{{ data.series[0]?.date }}</span><span>{{ data.series[data.series.length - 1]?.date }}</span>
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-4">
                        <div class="hd-card p-3 h-100">
                            <h2 class="h6">{{ $t("byStatus") }}</h2>
                            <router-link
                                v-for="s in data.byStatus"
                                :key="s.id"
                                :to="{ name: 'TicketOverview', query: { statusId: String(s.id) } }"
                                class="d-block text-decoration-none text-body mb-2"
                            >
                                <div class="d-flex justify-content-between small"><span>{{ s.title }}</span><strong>{{ s.count }}</strong></div>
                                <div class="progress" style="height: 6px">
                                    <div class="progress-bar" :style="{ width: ((s.count ?? 0) / maxStatus) * 100 + '%', backgroundColor: s.color }"></div>
                                </div>
                            </router-link>
                            <div class="border-top pt-2 mt-3 small text-muted">
                                <i class="bi bi-reply me-1"></i>{{ $t("avgFirstResponse") }}:
                                <strong class="text-body">{{ data.avgFirstResponseHours ?? "-" }}h</strong>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="row g-3 mb-4">
                    <div class="col-lg-5">
                        <div class="hd-card p-3 h-100">
                            <h2 class="h6">{{ $t("openByPriority") }}</h2>
                            <router-link
                                v-for="p in data.byPriority"
                                :key="p.id"
                                :to="{ name: 'TicketOverview', query: { priorityId: String(p.id), isClosed: 'false' } }"
                                class="d-flex align-items-center gap-2 text-decoration-none text-body mb-2"
                            >
                                <span class="small" style="width: 70px">{{ p.title }}</span>
                                <div class="progress flex-grow-1" style="height: 10px">
                                    <div class="progress-bar" :style="{ width: ((p.count ?? 0) / maxPriority) * 100 + '%', backgroundColor: p.color }"></div>
                                </div>
                                <strong class="small" style="width: 32px; text-align: right">{{ p.count }}</strong>
                            </router-link>
                        </div>
                    </div>
                    <div class="col-lg-7">
                        <div class="hd-card p-3 h-100">
                            <h2 class="h6">{{ $t("teamQueues") }}</h2>
                            <div class="table-responsive">
                                <table class="table table-sm align-middle mb-0">
                                    <thead>
                                        <tr class="small text-muted">
                                            <th>{{ $t("supportTeam") }}</th>
                                            <th class="text-end">{{ $t("open") }}</th>
                                            <th class="text-end">{{ $t("overdue") }}</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr v-for="t in data.byTeam" :key="t.id">
                                            <td>
                                                <router-link :to="{ name: 'TicketOverview', query: { supportTeamId: String(t.id), isClosed: 'false' } }" class="text-decoration-none">
                                                    <span class="hd-dot me-2" :style="{ backgroundColor: t.color }"></span>{{ t.title }}
                                                </router-link>
                                            </td>
                                            <td class="text-end">{{ t.open }}</td>
                                            <td class="text-end" :class="{ 'hd-sla-overdue': (t.overdue ?? 0) > 0 }">{{ t.overdue }}</td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        </LoadingContainer>

        <h2 class="h6 text-uppercase text-muted mb-2">{{ $t("quickAccess") }}</h2>
        <Dashboard />
    </section>
</template>
