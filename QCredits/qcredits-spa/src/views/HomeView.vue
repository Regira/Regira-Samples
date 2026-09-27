<!-- Home: a personal balance card for every employee, plus the organisation overview and the
     approval queue for administrators. Figures come from the API's dashboard endpoints (row-scoped). -->
<script setup lang="ts">
import { computed, ref } from "vue"
import type { AxiosInstance } from "axios"
import { get } from "@regira/modules/vue/ioc"
import { onAuthenticated } from "@regira/modules/vue/auth"
import { LoadingContainer } from "@regira/modules/vue/ui"
import { formatDate } from "@regira/modules/vue/formatters"
import { Dashboard } from "@/components/entity-navigation"
import { CreditBar, StatusBadge } from "@/components/credits"
import { activityTypeIcon, formatCredits, formatEuro, yearOptions, type ActivityType, type RequestStatus } from "@/domain/enums"
import { useAccess } from "@/access"

interface Balance {
    year: number
    annualCredits: number
    reservedCredits: number
    reservedUsed: number
    carriedOver: number
    minBalance: number
    freeCredits: number
    usedCredits: number
    pendingCredits: number
    remainingCredits: number
    requestable: number
}
interface Me {
    email: string
    isAdmin: boolean
    employee?: { id: number; firstName: string; lastName: string; jobTitle?: string; department?: string }
    balance?: Balance
}
interface DashboardData {
    year: number
    scope: string
    totals: Record<string, number>
    requestsByStatus: Array<{ status: RequestStatus; count: number; credits: number }>
    usageByType: Array<{ type: ActivityType; credits: number; cost: number }>
    usageByDepartment: Array<{ department: string; employees: number; free: number; used: number }>
    groupTrainings: { count: number; totalCost: number; participants: number; days: number }
    pendingApprovals: Array<{ id: number; title: string; year: number; totalCredits: number; submittedAt: string; employee: string; department: string }>
}
interface RecentRequest {
    id: number
    title: string
    year: number
    status: RequestStatus
    totalCredits: number
    created: string
}

const { isAdmin } = useAccess()
const year = ref(new Date().getFullYear())
const me = ref<Me>()
const dash = ref<DashboardData>()
const recent = ref<Array<RecentRequest>>([])
const isLoading = ref(false)

async function load() {
    isLoading.value = true
    try {
        const axios = get<AxiosInstance>("axios")!
        const [meRes, dashRes, recentRes] = await Promise.all([
            axios.get("/dashboard/me", { params: { year: year.value } }),
            axios.get("/dashboard", { params: { year: year.value } }),
            isAdmin() ? Promise.resolve({ data: { items: [] } }) : axios.get("/credit-requests/search", { params: { pageSize: 5, sortBy: "Newest" } }),
        ])
        me.value = meRes.data
        dash.value = dashRes.data
        recent.value = recentRes.data.items
    } finally {
        isLoading.value = false
    }
}
onAuthenticated(() => load())

const balance = computed(() => me.value?.balance)
const totals = computed(() => dash.value?.totals ?? {})
const usedPct = computed(() => (totals.value.free ? Math.round(((totals.value.used ?? 0) / totals.value.free) * 100) : 0))
const pendingCount = computed(() => dash.value?.requestsByStatus.find((x) => x.status === "Submitted")?.count ?? 0)
const maxTypeCredits = computed(() => Math.max(1, ...(dash.value?.usageByType ?? []).map((x) => x.credits)))
const firstName = computed(() => me.value?.employee?.firstName ?? "")
</script>

<template>
    <section class="qc-home">
        <div class="d-flex flex-wrap align-items-center justify-content-between gap-2 my-3">
            <div>
                <h1 class="h3 mb-0">{{ $t("welcome", { name: firstName }) }}</h1>
                <div class="text-muted small" v-if="me?.employee">{{ me.employee.jobTitle }} &middot; {{ me.employee.department }}</div>
            </div>
            <div class="d-flex gap-2 align-items-center">
                <select v-model.number="year" class="form-select form-select-sm w-auto" @change="load">
                    <option v-for="y in yearOptions()" :key="y" :value="y">{{ y }}</option>
                </select>
                <RouterLink :to="{ name: 'CreditRequestDetails', params: { id: 'new' } }" class="btn btn-primary btn-sm">
                    <i class="bi bi-plus-lg me-1"></i>{{ $t("newRequest") }}
                </RouterLink>
            </div>
        </div>

        <LoadingContainer :is-loading="isLoading">
            <div class="row g-3">
                <!-- personal balance -->
                <div class="col-lg-6" v-if="me?.employee">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body">
                            <h2 class="h6 text-uppercase text-muted mb-3"><i class="bi bi-wallet2 me-1"></i>{{ $t("myCredits", { year }) }}</h2>
                            <template v-if="balance">
                                <div class="d-flex align-items-baseline gap-2 mb-2">
                                    <span class="display-6 fw-semibold" :class="balance.remainingCredits < 0 ? 'text-danger' : 'text-success'">{{ formatCredits(balance.remainingCredits) }}</span>
                                    <span class="text-muted">/ {{ formatCredits(balance.freeCredits) }} {{ $t("creditsLeft") }}</span>
                                </div>
                                <CreditBar :free="balance.freeCredits" :used="balance.usedCredits" :pending="balance.pendingCredits" :min-balance="balance.minBalance" show-legend />
                                <div class="row g-2 text-center mt-3">
                                    <div class="col-3"><div class="qc-stat"><div class="qc-stat-value">{{ formatCredits(balance.annualCredits) }}</div><div class="qc-stat-label">{{ $t("annualCredits") }}</div></div></div>
                                    <div class="col-3"><div class="qc-stat"><div class="qc-stat-value">{{ formatCredits(balance.reservedUsed) }}/{{ formatCredits(balance.reservedCredits) }}</div><div class="qc-stat-label">{{ $t("companyDaysShort") }}</div></div></div>
                                    <div class="col-3"><div class="qc-stat"><div class="qc-stat-value">{{ balance.carriedOver > 0 ? "+" : "" }}{{ formatCredits(balance.carriedOver) }}</div><div class="qc-stat-label">{{ $t("carriedOver") }}</div></div></div>
                                    <div class="col-3"><div class="qc-stat"><div class="qc-stat-value">{{ formatCredits(balance.requestable) }}</div><div class="qc-stat-label">{{ $t("requestable") }}</div></div></div>
                                </div>
                                <p class="small text-muted mt-3 mb-0"><i class="bi bi-info-circle me-1"></i>{{ $t("creditRule") }}</p>
                            </template>
                            <p v-else class="text-muted mb-0">{{ $t("noAllocation", { year }) }}</p>
                        </div>
                    </div>
                </div>

                <!-- my recent requests (employees) -->
                <div class="col-lg-6" v-if="!isAdmin()">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body">
                            <h2 class="h6 text-uppercase text-muted mb-3"><i class="bi bi-mortarboard me-1"></i>{{ $t("myRecentRequests") }}</h2>
                            <p v-if="!recent.length" class="text-muted">{{ $t("noRequestsYet") }}</p>
                            <RouterLink
                                v-for="r in recent"
                                :key="r.id"
                                :to="{ name: 'CreditRequestDetails', params: { id: r.id } }"
                                class="d-flex align-items-center gap-2 py-2 border-bottom text-decoration-none text-body"
                            >
                                <span class="flex-grow-1 text-truncate">{{ r.title }} <small class="text-muted">({{ r.year }})</small></span>
                                <span class="fw-semibold">{{ formatCredits(r.totalCredits) }}</span>
                                <StatusBadge :status="r.status" />
                            </RouterLink>
                            <RouterLink :to="{ name: 'CreditRequestOverview' }" class="btn btn-link px-0 mt-2">{{ $t("allMyRequests") }} <i class="bi bi-arrow-right"></i></RouterLink>
                        </div>
                    </div>
                </div>

                <!-- approval queue (admins) -->
                <div class="col-lg-6" v-if="isAdmin() && dash">
                    <div class="card h-100 shadow-sm">
                        <div class="card-body">
                            <h2 class="h6 text-uppercase text-muted mb-3">
                                <i class="bi bi-inbox me-1"></i>{{ $t("awaitingApproval") }}
                                <span class="badge text-bg-warning ms-1">{{ pendingCount }}</span>
                            </h2>
                            <p v-if="!dash.pendingApprovals.length" class="text-muted"><i class="bi bi-check2-all me-1"></i>{{ $t("inboxZero") }}</p>
                            <RouterLink
                                v-for="r in dash.pendingApprovals"
                                :key="r.id"
                                :to="{ name: 'CreditRequestDetails', params: { id: r.id } }"
                                class="d-flex align-items-center gap-2 py-2 border-bottom text-decoration-none text-body"
                            >
                                <span class="flex-grow-1 text-truncate">
                                    {{ r.title }}
                                    <small class="d-block text-muted text-truncate">{{ r.employee }} &middot; {{ r.department }} &middot; {{ formatDate(new Date(r.submittedAt), "en-GB") }}</small>
                                </span>
                                <span class="fw-semibold">{{ formatCredits(r.totalCredits) }}</span>
                                <i class="bi bi-chevron-right text-muted"></i>
                            </RouterLink>
                            <RouterLink :to="{ name: 'CreditRequestOverview', query: { status: 'Submitted', sortBy: 'SubmittedAt' } }" class="btn btn-link px-0 mt-2">
                                {{ $t("openQueue") }} <i class="bi bi-arrow-right"></i>
                            </RouterLink>
                        </div>
                    </div>
                </div>

                <!-- organisation overview (admins) -->
                <template v-if="isAdmin() && dash">
                    <div class="col-12">
                        <div class="row g-3">
                            <div class="col-6 col-md-4 col-xl-2"><div class="card qc-kpi shadow-sm"><div class="card-body"><div class="qc-kpi-label">{{ $t("employees") }}</div><div class="qc-kpi-value">{{ totals.employees }}</div></div></div></div>
                            <div class="col-6 col-md-4 col-xl-2"><div class="card qc-kpi shadow-sm"><div class="card-body"><div class="qc-kpi-label">{{ $t("freeBudget") }}</div><div class="qc-kpi-value">{{ formatCredits(totals.free) }}</div></div></div></div>
                            <div class="col-6 col-md-4 col-xl-2"><div class="card qc-kpi shadow-sm"><div class="card-body"><div class="qc-kpi-label">{{ $t("usedCredits") }}</div><div class="qc-kpi-value text-primary">{{ formatCredits(totals.used) }} <small class="text-muted fs-6">({{ usedPct }}%)</small></div></div></div></div>
                            <div class="col-6 col-md-4 col-xl-2"><div class="card qc-kpi shadow-sm"><div class="card-body"><div class="qc-kpi-label">{{ $t("pendingCredits") }}</div><div class="qc-kpi-value text-warning">{{ formatCredits(totals.pending) }}</div></div></div></div>
                            <div class="col-6 col-md-4 col-xl-2"><div class="card qc-kpi shadow-sm"><div class="card-body"><div class="qc-kpi-label">{{ $t("approvedSpend") }}</div><div class="qc-kpi-value">{{ formatEuro(totals.approvedCost) }}</div></div></div></div>
                            <div class="col-6 col-md-4 col-xl-2"><div class="card qc-kpi shadow-sm"><div class="card-body"><div class="qc-kpi-label">{{ $t("overdrawn") }}</div><div class="qc-kpi-value" :class="totals.overdrawn ? 'text-danger' : ''">{{ totals.overdrawn }}</div></div></div></div>
                        </div>
                    </div>

                    <div class="col-lg-6">
                        <div class="card h-100 shadow-sm">
                            <div class="card-body">
                                <h2 class="h6 text-uppercase text-muted mb-3"><i class="bi bi-diagram-3 me-1"></i>{{ $t("usageByDepartment") }}</h2>
                                <div v-for="d in dash.usageByDepartment" :key="d.department" class="mb-2">
                                    <div class="d-flex justify-content-between small">
                                        <span>{{ d.department }} <span class="text-muted">({{ d.employees }})</span></span>
                                        <span class="text-muted">{{ formatCredits(d.used) }} / {{ formatCredits(d.free) }}</span>
                                    </div>
                                    <CreditBar :free="d.free" :used="d.used" :min-balance="0" compact />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-6">
                        <div class="card h-100 shadow-sm">
                            <div class="card-body">
                                <h2 class="h6 text-uppercase text-muted mb-3"><i class="bi bi-bar-chart me-1"></i>{{ $t("usageByType") }}</h2>
                                <div v-for="t in dash.usageByType" :key="t.type" class="mb-2">
                                    <div class="d-flex justify-content-between small">
                                        <span><i :class="activityTypeIcon[t.type]" class="me-1"></i>{{ $t(`activity${t.type}`) }}</span>
                                        <span class="text-muted">{{ formatCredits(t.credits) }} {{ $t("credits") }} &middot; {{ formatEuro(t.cost) }}</span>
                                    </div>
                                    <div class="progress qc-bar-thin"><div class="progress-bar bg-info" :style="{ width: `${(t.credits / maxTypeCredits) * 100}%` }"></div></div>
                                </div>
                                <hr />
                                <div class="d-flex flex-wrap gap-3 small">
                                    <span v-for="s in dash.requestsByStatus" :key="s.status"><StatusBadge :status="s.status" /> {{ s.count }}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                </template>

                <!-- group trainings -->
                <div class="col-12" v-if="dash">
                    <div class="card shadow-sm">
                        <div class="card-body d-flex flex-wrap align-items-center gap-4">
                            <h2 class="h6 text-uppercase text-muted mb-0"><i class="bi bi-people me-1"></i>{{ isAdmin() ? $t("groupTrainingsYear", { year }) : $t("myGroupTrainings", { year }) }}</h2>
                            <span><strong>{{ dash.groupTrainings.count }}</strong> {{ $t("trainings") }}</span>
                            <span><strong>{{ dash.groupTrainings.days }}</strong> {{ $t("days") }}</span>
                            <span v-if="isAdmin()"><strong>{{ dash.groupTrainings.participants }}</strong> {{ $t("participants") }}</span>
                            <span v-if="isAdmin()"><strong>{{ formatEuro(dash.groupTrainings.totalCost) }}</strong> {{ $t("separateBudget") }}</span>
                            <span class="small text-muted"><i class="bi bi-piggy-bank me-1"></i>{{ $t("groupTrainingFundingShort") }}</span>
                            <RouterLink :to="{ name: 'GroupTrainingOverview', query: { year } }" class="btn btn-link ms-auto">{{ $t("view") }} <i class="bi bi-arrow-right"></i></RouterLink>
                        </div>
                    </div>
                </div>
            </div>
        </LoadingContainer>

        <h2 class="h6 text-uppercase text-muted mt-4 mb-2">{{ $t("goTo") }}</h2>
        <Dashboard />
    </section>
</template>
