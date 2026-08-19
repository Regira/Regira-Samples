<template>
    <LoadingContainer :is-loading="loading">
        <div v-if="summary" class="fleet-dashboard">
            <!-- KPI row -->
            <div class="row row-cols-2 row-cols-md-3 row-cols-xl-4 g-3 mb-4">
                <div class="col"><KpiCard :label="$t('kpi.totalVehicles')" :value="summary.totalVehicles" icon="bi bi-truck" variant="primary" /></div>
                <div class="col"><KpiCard :label="$t('kpi.vehiclesInMaintenance')" :value="summary.vehiclesInMaintenance" icon="bi bi-wrench-adjustable" variant="warning" /></div>
                <div class="col"><KpiCard :label="$t('kpi.openInterventions')" :value="summary.openInterventions" icon="bi bi-tools" variant="info" /></div>
                <div class="col"><KpiCard :label="$t('kpi.overdueInterventions')" :value="summary.overdueInterventions" icon="bi bi-exclamation-triangle" variant="danger" /></div>
                <div class="col"><KpiCard :label="$t('kpi.completedThisMonth')" :value="summary.completedThisMonth" icon="bi bi-check-circle" variant="success" /></div>
                <div class="col"><KpiCard :label="$t('kpi.totalSpend')" :value="formatCurrency(summary.totalSpend, undefined, 'EUR')" icon="bi bi-cash-stack" variant="primary" /></div>
                <div class="col"><KpiCard :label="$t('kpi.activeSuppliers')" :value="`${summary.activeSuppliers} / ${summary.totalSuppliers}`" icon="bi bi-building-gear" variant="info" /></div>
                <div class="col"><KpiCard :label="$t('kpi.outstandingAmount')" :value="formatCurrency(summary.outstandingAmount, undefined, 'EUR')" icon="bi bi-receipt" variant="danger" /></div>
            </div>

            <div class="row g-3 mb-4">
                <!-- interventions by status -->
                <div class="col-lg-4">
                    <div class="card h-100 border-0 shadow-sm">
                        <div class="card-body">
                            <h6 class="card-title mb-3">{{ $t("interventionsByStatus") }}</h6>
                            <div v-for="s in statusCounts" :key="s.status" class="d-flex align-items-center justify-content-between mb-2">
                                <StatusBadge :status="s.status" :variants="interventionStatusVariants" :label="$t(`interventionStatus.${s.status}`)" />
                                <div class="flex-grow-1 mx-2">
                                    <div class="progress" style="height: 6px">
                                        <div class="progress-bar" :class="`bg-${interventionStatusVariants[s.status] ?? 'secondary'}`" :style="{ width: `${statusPercent(s.count)}%` }" />
                                    </div>
                                </div>
                                <span class="fw-semibold">{{ s.count }}</span>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- spend by month -->
                <div class="col-lg-8">
                    <div class="card h-100 border-0 shadow-sm">
                        <div class="card-body">
                            <h6 class="card-title mb-3">{{ $t("spendByMonth") }} ({{ currentYear }})</h6>
                            <div class="spend-chart d-flex align-items-end gap-1">
                                <div v-for="m in spendByMonth" :key="m.month" class="spend-bar-wrap flex-grow-1 text-center">
                                    <div class="spend-bar bg-primary-subtle border border-primary rounded-top mx-auto" :style="{ height: `${spendBarHeight(m.total)}px` }" :title="formatCurrency(m.total, undefined, 'EUR')" />
                                    <div class="small text-muted mt-1">{{ monthLabel(m.month) }}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- top suppliers table -->
            <div class="card border-0 shadow-sm mb-4">
                <div class="card-body">
                    <h6 class="card-title mb-3">{{ $t("topSuppliers") }}</h6>
                    <div class="table-responsive">
                        <table class="table table-sm align-middle mb-0">
                            <thead>
                                <tr class="text-muted small">
                                    <th>{{ $t("supplier") }}</th>
                                    <th class="text-end">{{ $t("intervention") }}</th>
                                    <th class="text-end">{{ $t("totalAmount") }}</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="s in topSuppliers" :key="s.supplierId">
                                    <td>{{ s.supplierName }}</td>
                                    <td class="text-end">{{ s.interventionCount }}</td>
                                    <td class="text-end fw-semibold">{{ formatCurrency(s.totalSpend, undefined, "EUR") }}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </LoadingContainer>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue"
import { LoadingContainer } from "@regira/modules/vue/ui"
import { useAxios } from "@regira/modules/vue/http"
import { formatCurrency } from "@regira/modules/vue/formatters"
import KpiCard from "./KpiCard.vue"
import StatusBadge from "@/components/status/StatusBadge.vue"
import { interventionStatusVariants } from "@/components/status/variants"
import type { DashboardSummary, MonthlySpend, StatusCount, TopSupplier } from "./types"

const axios = useAxios()
const loading = ref(true)
const summary = ref<DashboardSummary>()
const spendByMonth = ref<Array<MonthlySpend>>([])
const statusCounts = ref<Array<StatusCount>>([])
const topSuppliers = ref<Array<TopSupplier>>([])
const currentYear = new Date().getFullYear()

const maxMonthlySpend = computed(() => Math.max(1, ...spendByMonth.value.map((m) => m.total)))
const totalStatusCount = computed(() => Math.max(1, statusCounts.value.reduce((sum, s) => sum + s.count, 0)))

function spendBarHeight(total: number) {
    return Math.round((total / maxMonthlySpend.value) * 100) + 8
}
function statusPercent(count: number) {
    return Math.round((count / totalStatusCount.value) * 100)
}
function monthLabel(month: number) {
    return new Date(2000, month - 1, 1).toLocaleString("en", { month: "short" })
}

onMounted(async () => {
    try {
        const [summaryRes, spendRes, statusRes, suppliersRes] = await Promise.all([
            axios.get<DashboardSummary>("/dashboard/summary"),
            axios.get<Array<MonthlySpend>>("/dashboard/spend-by-month", { params: { year: currentYear } }),
            axios.get<Array<StatusCount>>("/dashboard/interventions-by-status"),
            axios.get<Array<TopSupplier>>("/dashboard/top-suppliers", { params: { take: 5 } }),
        ])
        summary.value = summaryRes.data
        spendByMonth.value = spendRes.data
        statusCounts.value = statusRes.data
        topSuppliers.value = suppliersRes.data
    } finally {
        loading.value = false
    }
})
</script>

<style scoped>
.spend-chart {
    height: 140px;
}
.spend-bar-wrap {
    min-width: 0;
}
.spend-bar {
    width: 70%;
    min-height: 4px;
}
</style>
