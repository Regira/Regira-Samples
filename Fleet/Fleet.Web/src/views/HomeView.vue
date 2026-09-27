<script setup lang="ts">
import { computed, onMounted, ref } from "vue"
import { RouterLink } from "vue-router"
import { Feedback, LoadingContainer, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useAxios } from "@regira/modules/vue/http"
import { formatCurrencyCompact } from "@regira/modules/vue/formatters"
import { useEntityStore as useInterventionStore, type Entity as Intervention } from "@/entities/interventions"
import KpiTile from "@/components/dashboard/KpiTile.vue"
import ColumnChart from "@/components/dashboard/ColumnChart.vue"
import BarList from "@/components/dashboard/BarList.vue"
import SegmentBar from "@/components/dashboard/SegmentBar.vue"
import StatusBadge from "@/components/StatusBadge.vue"
import DueDate from "@/components/DueDate.vue"
import {
    categoryIcon,
    culture,
    fmtDay,
    fmtKm,
    fmtMoney,
    interventionStatusBadge,
    invoiceStatusBadge,
    priorityBadge,
    todayIso,
    vehicleStatusBadge,
    vehicleTypeIcon,
} from "@/infrastructure/domain"

type Count = { key: string; count: number }
interface DashboardData {
    kpis: {
        fleetSize: number
        inMaintenance: number
        availability: number
        openInterventions: number
        urgentOpen: number
        overduePlanned: number
        completedNotInvoiced: number
        spendYtd: number
        openInvoiceAmount: number
        openInvoiceCount: number
        overdueInvoiceAmount: number
        overdueInvoiceCount: number
        serviceDueCount: number
    }
    vehiclesByStatus: Array<Count>
    vehiclesByType: Array<Count>
    interventionsByStatus: Array<Count>
    invoicesByStatus: Array<Count>
    spendByMonth: Array<{ month: string; total: number; count: number }>
    spendByCategory: Array<{ category: string; total: number; count: number }>
    topSuppliers: Array<{ id: number; title: string; total: number; count: number }>
    upcomingServices: Array<{ id: number; licensePlate: string; make: string; model: string; vehicleType: string; nextServiceDate: string; mileage: number }>
}

// Cross-entity aggregates come from one read-only endpoint (GET /api/dashboard) - a plain useAxios call
// with its own feedback, not an entity slice.
const axios = useAxios()
const feedback = useFeedback()
const data = ref<DashboardData>()
const isLoading = ref(false)

const { service: interventionService } = useInterventionStore()
const attention = ref<Array<Intervention>>([])

async function load() {
    isLoading.value = true
    try {
        const [dashboard, open] = await Promise.all([
            axios.get<DashboardData>("/dashboard"),
            interventionService.search({
                status: ["Planned", "InProgress"],
                priority: ["Urgent", "High"],
                maxDate: todayIso(14),
                sortBy: ["ScheduledDate"],
                pageSize: 8,
            }),
        ])
        data.value = dashboard.data
        attention.value = open.items
    } catch (ex) {
        console.error(ex)
        feedback.fail("Loading the dashboard failed", toFeedbackError(ex))
    } finally {
        isLoading.value = false
    }
}
onMounted(load)

const k = computed(() => data.value?.kpis)
const monthLabel = (m: string) => {
    const [y, mo] = m.split("-").map(Number)
    return new Date(y!, mo! - 1, 1).toLocaleDateString(culture, { month: "short", year: "2-digit" })
}
const spendPoints = computed(() =>
    (data.value?.spendByMonth ?? []).map((x) => ({ label: monthLabel(x.month), value: x.total, note: `${x.count} completed interventions` }))
)
const spend12m = computed(() => (data.value?.spendByMonth ?? []).reduce((s, x) => s + x.total, 0))
const categoryRows = computed(() =>
    (data.value?.spendByCategory ?? []).map((x) => ({ key: x.category, label: x.category, value: x.total, note: `${x.count}x`, icon: categoryIcon[x.category] }))
)
const supplierRows = computed(() =>
    (data.value?.topSuppliers ?? []).map((x) => ({
        key: x.id,
        label: x.title,
        value: x.total,
        note: `${x.count} jobs`,
        to: { name: "SupplierDetails", params: { id: x.id } },
    }))
)
const typeRows = computed(() =>
    (data.value?.vehiclesByType ?? []).map((x) => ({
        key: x.key,
        label: x.key,
        value: x.count,
        icon: vehicleTypeIcon[x.key],
        to: { name: "VehicleOverview", query: { vehicleType: x.key } },
    }))
)
const countOf = (list: Array<Count> | undefined, key: string) => list?.find((x) => x.key === key)?.count ?? 0
// categorical slots (blue, orange, aqua) + neutral grey for the terminal "cancelled" state
const interventionSegments = computed(() => [
    { key: "Planned", label: "Planned", value: countOf(data.value?.interventionsByStatus, "Planned"), color: "#2a78d6", icon: interventionStatusBadge.Planned!.icon },
    { key: "InProgress", label: "In progress", value: countOf(data.value?.interventionsByStatus, "InProgress"), color: "#eb6834", icon: interventionStatusBadge.InProgress!.icon },
    { key: "Completed", label: "Completed", value: countOf(data.value?.interventionsByStatus, "Completed"), color: "#1baf7a", icon: interventionStatusBadge.Completed!.icon },
    { key: "Cancelled", label: "Cancelled", value: countOf(data.value?.interventionsByStatus, "Cancelled"), color: "#b5b3ac", icon: interventionStatusBadge.Cancelled!.icon },
])
const invoiceSegments = computed(() => [
    { key: "Received", label: "Received", value: countOf(data.value?.invoicesByStatus, "Received"), color: "#2a78d6", icon: invoiceStatusBadge.Received!.icon },
    { key: "Approved", label: "Approved", value: countOf(data.value?.invoicesByStatus, "Approved"), color: "#eb6834", icon: invoiceStatusBadge.Approved!.icon },
    { key: "Paid", label: "Paid", value: countOf(data.value?.invoicesByStatus, "Paid"), color: "#1baf7a", icon: invoiceStatusBadge.Paid!.icon },
    { key: "Disputed", label: "Disputed", value: countOf(data.value?.invoicesByStatus, "Disputed"), color: "#b5b3ac", icon: invoiceStatusBadge.Disputed!.icon },
])
const compactMoney = (v: number) => formatCurrencyCompact(v, culture, "EUR")
</script>

<template>
    <section class="fleet-dashboard">
        <div class="d-flex flex-wrap align-items-end gap-2 mb-3">
            <div class="me-auto">
                <h1 class="fleet-page-title mb-0">{{ $t("fleetDashboard") }}</h1>
                <p class="text-secondary mb-0 small">{{ $t("dashboardSubtitle") }}</p>
            </div>
            <RouterLink :to="{ name: 'InterventionDetails', params: { id: 'new' } }" class="btn btn-primary">
                <i class="bi bi-plus-lg me-1"></i>{{ $t("planIntervention") }}
            </RouterLink>
            <button type="button" class="btn btn-outline-secondary" :disabled="isLoading" :title="$t('refresh')" @click="load">
                <i class="bi bi-arrow-clockwise"></i>
            </button>
        </div>
        <Feedback :feedback="feedback" />

        <LoadingContainer :is-loading="isLoading && !data">
            <template v-if="k">
                <!-- KPI row -->
                <div class="row g-3 mb-3">
                    <div class="col-6 col-md-4 col-xl-2">
                        <KpiTile
                            label="Fleet availability"
                            :value="`${k.availability}%`"
                            icon="bi bi-speedometer"
                            :sub="`${k.fleetSize} active vehicles`"
                            :to="{ name: 'VehicleOverview' }"
                        />
                    </div>
                    <div class="col-6 col-md-4 col-xl-2">
                        <KpiTile
                            label="In maintenance"
                            :value="String(k.inMaintenance)"
                            icon="bi bi-wrench-adjustable"
                            :sub="`${countOf(data?.vehiclesByStatus, 'OutOfService')} out of service`"
                            :to="{ name: 'VehicleOverview', query: { status: 'InMaintenance' } }"
                        />
                    </div>
                    <div class="col-6 col-md-4 col-xl-2">
                        <KpiTile
                            label="Open interventions"
                            :value="String(k.openInterventions)"
                            icon="bi bi-tools"
                            :status="k.urgentOpen > 0 ? 'critical' : 'good'"
                            :status-text="`${k.urgentOpen} urgent`"
                            :to="{ name: 'InterventionOverview', query: { status: 'Planned' } }"
                        />
                    </div>
                    <div class="col-6 col-md-4 col-xl-2">
                        <KpiTile
                            label="Service due (30 days)"
                            :value="String(k.serviceDueCount)"
                            icon="bi bi-calendar-check"
                            :status="k.serviceDueCount > 0 ? 'warning' : 'good'"
                            :status-text="k.serviceDueCount > 0 ? 'plan now' : 'all good'"
                            :to="{ name: 'VehicleOverview', query: { serviceDueBefore: todayIso(30), sortBy: 'NextServiceDate' } }"
                        />
                    </div>
                    <div class="col-6 col-md-4 col-xl-2">
                        <KpiTile label="Spend year-to-date" :value="compactMoney(k.spendYtd)" icon="bi bi-cash-stack" :sub="fmtMoney(k.spendYtd)" />
                    </div>
                    <div class="col-6 col-md-4 col-xl-2">
                        <KpiTile
                            label="Unpaid invoices"
                            :value="compactMoney(k.openInvoiceAmount)"
                            icon="bi bi-receipt"
                            :status="k.overdueInvoiceCount > 0 ? 'critical' : 'good'"
                            :status-text="`${k.overdueInvoiceCount} overdue`"
                            :to="{ name: 'InvoiceOverview', query: { isOverdue: 'true' } }"
                        />
                    </div>
                </div>

                <div class="row g-3 mb-3">
                    <div class="col-lg-8">
                        <div class="fleet-card h-100">
                            <div class="fleet-card__head">
                                <div>
                                    <h2 class="fleet-card__title">{{ $t("maintenanceSpend") }}</h2>
                                    <span class="fleet-card__sub">Completed interventions, last 12 months (excl. VAT)</span>
                                </div>
                                <div class="text-end">
                                    <div class="fs-5 fw-semibold fleet-num">{{ fmtMoney(spend12m) }}</div>
                                    <span class="fleet-card__sub">12-month total</span>
                                </div>
                            </div>
                            <ColumnChart :points="spendPoints" :format="fmtMoney" :format-axis="compactMoney" chart-label="Maintenance spend per month" />
                        </div>
                    </div>
                    <div class="col-lg-4">
                        <div class="fleet-card h-100">
                            <div class="fleet-card__head">
                                <div>
                                    <h2 class="fleet-card__title">{{ $t("spendByCategory") }}</h2>
                                    <span class="fleet-card__sub">Last 12 months</span>
                                </div>
                            </div>
                            <BarList :rows="categoryRows" :format="compactMoney" />
                        </div>
                    </div>
                </div>

                <div class="row g-3 mb-3">
                    <div class="col-xl-8">
                        <div class="fleet-card h-100">
                            <div class="fleet-card__head">
                                <div>
                                    <h2 class="fleet-card__title">{{ $t("needsAttention") }}</h2>
                                    <span class="fleet-card__sub">Open high/urgent interventions due within 14 days</span>
                                </div>
                                <RouterLink :to="{ name: 'InterventionOverview', query: { priority: 'Urgent' } }" class="btn btn-sm btn-outline-primary">
                                    {{ $t("viewAll") }}
                                </RouterLink>
                            </div>
                            <div v-if="attention.length" class="entity-list fleet-table">
                                <div class="row fleet-table__head">
                                    <div class="col-3 col-md-2">{{ $t("date") }}</div>
                                    <div class="col-4 col-md-3">{{ $t("vehicle") }}</div>
                                    <div class="col d-none d-md-block">{{ $t("interventionTypes") }}</div>
                                    <div class="col-auto">{{ $t("priority") }}</div>
                                    <div class="col-auto fleet-col-status">{{ $t("status") }}</div>
                                </div>
                                <div v-for="row in attention" :key="row.id" class="row fleet-table__row align-items-center">
                                    <div class="col-3 col-md-2"><DueDate :value="row.scheduledDate" :soon-days="3" :inactive="row.status === 'InProgress'" /></div>
                                    <div class="col-4 col-md-3 text-truncate">
                                        <RouterLink :to="{ name: 'InterventionDetails', params: { id: row.id } }" class="fleet-plate">{{ row.vehicle?.licensePlate }}</RouterLink>
                                    </div>
                                    <div class="col d-none d-md-block text-truncate small">{{ (row.lines ?? []).map((l) => l.interventionType?.title).join(", ") }}</div>
                                    <div class="col-auto"><StatusBadge :value="row.priority" :map="priorityBadge" :compact="true" /></div>
                                    <div class="col-auto fleet-col-status"><StatusBadge :value="row.status" :map="interventionStatusBadge" :compact="true" /></div>
                                </div>
                            </div>
                            <p v-else class="text-secondary mb-0"><i class="bi bi-check-circle me-1"></i>{{ $t("nothingUrgent") }}</p>
                        </div>
                    </div>
                    <div class="col-xl-4">
                        <div class="fleet-card mb-3">
                            <div class="fleet-card__head">
                                <h2 class="fleet-card__title">{{ $t("interventionStatus") }}</h2>
                                <span v-if="k.overduePlanned" class="fleet-badge fleet-badge--critical"><i class="bi bi-exclamation-triangle"></i><span>{{ k.overduePlanned }} overdue</span></span>
                            </div>
                            <SegmentBar :segments="interventionSegments" />
                            <RouterLink v-if="k.completedNotInvoiced" :to="{ name: 'InterventionOverview', query: { status: 'Completed', isInvoiced: 'false' } }" class="small d-inline-block mt-2">
                                <i class="bi bi-hourglass-split me-1"></i>{{ k.completedNotInvoiced }} completed, not yet invoiced
                            </RouterLink>
                        </div>
                        <div class="fleet-card">
                            <div class="fleet-card__head">
                                <h2 class="fleet-card__title">{{ $t("invoices") }}</h2>
                                <span class="fleet-card__sub">{{ k.openInvoiceCount }} unpaid &middot; {{ fmtMoney(k.overdueInvoiceAmount) }} overdue</span>
                            </div>
                            <SegmentBar :segments="invoiceSegments" />
                        </div>
                    </div>
                </div>

                <div class="row g-3 mb-3">
                    <div class="col-lg-5">
                        <div class="fleet-card h-100">
                            <div class="fleet-card__head">
                                <div>
                                    <h2 class="fleet-card__title">{{ $t("upcomingServices") }}</h2>
                                    <span class="fleet-card__sub">Overdue or due within 30 days</span>
                                </div>
                                <RouterLink :to="{ name: 'VehicleOverview', query: { serviceDueBefore: todayIso(30), sortBy: 'NextServiceDate' } }" class="btn btn-sm btn-outline-primary">
                                    {{ $t("viewAll") }}
                                </RouterLink>
                            </div>
                            <div class="entity-list fleet-table">
                                <div v-for="v in data?.upcomingServices" :key="v.id" class="row fleet-table__row align-items-center">
                                    <div class="col-4">
                                        <RouterLink :to="{ name: 'VehicleDetails', params: { id: v.id } }" class="fleet-plate">{{ v.licensePlate }}</RouterLink>
                                    </div>
                                    <div class="col text-truncate small">
                                        <i :class="vehicleTypeIcon[v.vehicleType]" class="text-secondary me-1"></i>{{ v.make }} {{ v.model }}
                                        <span class="d-block text-secondary">{{ fmtKm(v.mileage) }}</span>
                                    </div>
                                    <div class="col-auto small"><DueDate :value="v.nextServiceDate" /></div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-4">
                        <div class="fleet-card h-100">
                            <div class="fleet-card__head">
                                <div>
                                    <h2 class="fleet-card__title">{{ $t("topSuppliers") }}</h2>
                                    <span class="fleet-card__sub">By spend, last 12 months</span>
                                </div>
                            </div>
                            <BarList :rows="supplierRows" :format="compactMoney" />
                        </div>
                    </div>
                    <div class="col-lg-3">
                        <div class="fleet-card h-100">
                            <div class="fleet-card__head">
                                <h2 class="fleet-card__title">{{ $t("fleetComposition") }}</h2>
                            </div>
                            <BarList :rows="typeRows" :format="(v: number) => String(v)" />
                            <ul class="fleet-legend mt-3">
                                <li v-for="s in data?.vehiclesByStatus ?? []" :key="s.key">
                                    <StatusBadge :value="s.key" :map="vehicleStatusBadge" />
                                    <strong class="fleet-num ms-auto">{{ s.count }}</strong>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>
                <p class="small text-secondary">{{ $t("asOf") }} {{ fmtDay(todayIso()) }}</p>
            </template>
        </LoadingContainer>
    </section>
</template>
