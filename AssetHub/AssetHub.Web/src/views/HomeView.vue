<script setup lang="ts">
import { computed, ref } from "vue"
import type { AxiosInstance } from "axios"
import { RouterLink } from "vue-router"
import { get } from "@regira/modules/vue/ioc"
import { onAuthenticated } from "@regira/modules/vue/auth"
import { LoadingContainer, Icon } from "@regira/modules/vue/ui"
import { Dashboard } from "@/components/entity-navigation"
import { fmtMoney, fmtDate, isoDay } from "@/utilities/format"

interface DashboardData {
    totalAssets: number
    assigned: number
    available: number
    totalValue: number
    employees: number
    warrantiesExpiring: number
    maintenanceDue: number
    byStatus: Array<{ id: number; title: string; color: string; kind: string; count: number }>
    byCategory: Array<{ id: number; title: string; icon?: string; count: number }>
    byLocation: Array<{ id: number; title: string; count: number }>
    recentAssignments: Array<{
        id: number
        assetId: number
        assetCode: string
        assetTitle: string
        employeeId: number
        employeeName: string
        assignedOn: string
        returnedOn?: string
    }>
}

const data = ref<DashboardData>()
const isLoading = ref(false)
const axios = get<AxiosInstance>("axios")!

async function load() {
    isLoading.value = true
    try {
        const response = await axios.get<DashboardData>("/dashboard")
        data.value = response.data
    } finally {
        isLoading.value = false
    }
}
onAuthenticated(() => load())

const maxCategory = computed(() => Math.max(1, ...(data.value?.byCategory.map((c) => c.count) ?? [1])))
const in60 = isoDay(60)
const in30 = isoDay(30)
</script>

<template>
    <section class="ah-home">
        <LoadingContainer :is-loading="isLoading && !data">
            <template v-if="data">
                <div class="row g-2 g-md-3 mb-3">
                    <div class="col-6 col-lg-3">
                        <RouterLink :to="{ name: 'AssetOverview' }" class="ah-kpi text-decoration-none">
                            <div class="ah-kpi__label"><Icon name="boxes" /> {{ $t("totalAssets") }}</div>
                            <div class="ah-kpi__value">{{ data.totalAssets }}</div>
                            <div class="ah-kpi__sub">{{ fmtMoney(data.totalValue) }}</div>
                        </RouterLink>
                    </div>
                    <div class="col-6 col-lg-3">
                        <RouterLink :to="{ name: 'AssetOverview', query: { isAssigned: 'true' } }" class="ah-kpi text-decoration-none">
                            <div class="ah-kpi__label"><Icon name="user" /> {{ $t("assigned") }}</div>
                            <div class="ah-kpi__value">{{ data.assigned }}</div>
                            <div class="ah-kpi__sub">{{ Math.round((100 * data.assigned) / Math.max(1, data.totalAssets)) }}% {{ $t("utilisation") }}</div>
                        </RouterLink>
                    </div>
                    <div class="col-6 col-lg-3">
                        <RouterLink :to="{ name: 'AssetOverview', query: { warrantyExpiresBefore: in60 } }" class="ah-kpi ah-kpi--warn text-decoration-none">
                            <div class="ah-kpi__label"><Icon name="security" /> {{ $t("warrantiesExpiring") }}</div>
                            <div class="ah-kpi__value">{{ data.warrantiesExpiring }}</div>
                            <div class="ah-kpi__sub">{{ $t("next60Days") }}</div>
                        </RouterLink>
                    </div>
                    <div class="col-6 col-lg-3">
                        <RouterLink :to="{ name: 'AssetOverview', query: { maintenanceDueBefore: in30 } }" class="ah-kpi ah-kpi--danger text-decoration-none">
                            <div class="ah-kpi__label"><Icon name="wrench" /> {{ $t("maintenanceDue") }}</div>
                            <div class="ah-kpi__value">{{ data.maintenanceDue }}</div>
                            <div class="ah-kpi__sub">{{ $t("overdueOrNext30Days") }}</div>
                        </RouterLink>
                    </div>
                </div>

                <div class="row g-3 mb-3">
                    <div class="col-lg-7">
                        <div class="ah-panel h-100">
                            <h2 class="ah-panel__title">{{ $t("assetsByStatus") }}</h2>
                            <div class="ah-stackbar mb-3" role="img" :aria-label="$t('assetsByStatus')">
                                <div
                                    v-for="s in data.byStatus.filter((x) => x.count)"
                                    :key="s.id"
                                    class="ah-stackbar__seg"
                                    :style="{ width: (100 * s.count) / Math.max(1, data.totalAssets) + '%', background: s.color }"
                                    :title="`${s.title}: ${s.count}`"
                                ></div>
                            </div>
                            <div class="row g-2">
                                <div v-for="s in data.byStatus" :key="s.id" class="col-6 col-md-4">
                                    <RouterLink :to="{ name: 'AssetOverview', query: { statusId: s.id } }" class="ah-status-tile text-decoration-none" :style="{ '--ah-status-color': s.color }">
                                        <span class="ah-status__dot"></span>
                                        <span class="flex-grow-1 text-truncate">{{ s.title }}</span>
                                        <span class="fw-semibold">{{ s.count }}</span>
                                    </RouterLink>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-5">
                        <div class="ah-panel h-100">
                            <h2 class="ah-panel__title">{{ $t("assetsByCategory") }}</h2>
                            <RouterLink
                                v-for="c in data.byCategory"
                                :key="c.id"
                                :to="{ name: 'AssetOverview', query: { categoryId: c.id } }"
                                class="ah-hbar text-decoration-none"
                            >
                                <span class="ah-hbar__label text-truncate"><i :class="`bi bi-${c.icon || 'box'}`" class="me-1"></i>{{ c.title }}</span>
                                <span class="ah-hbar__track"><span class="ah-hbar__fill" :style="{ width: (100 * c.count) / maxCategory + '%' }"></span></span>
                                <span class="ah-hbar__value">{{ c.count }}</span>
                            </RouterLink>
                        </div>
                    </div>
                </div>

                <div class="row g-3 mb-3">
                    <div class="col-lg-7">
                        <div class="ah-panel h-100">
                            <h2 class="ah-panel__title">{{ $t("recentAssignments") }}</h2>
                            <div class="entity-list ah-table">
                                <div v-for="a in data.recentAssignments" :key="a.id" class="row border-bottom py-1 align-items-center">
                                    <div class="col-3 col-md-2 text-truncate">
                                        <RouterLink :to="{ name: 'AssetDetails', params: { id: a.assetId } }" class="ah-code">{{ a.assetCode }}</RouterLink>
                                    </div>
                                    <div class="col text-truncate">{{ a.assetTitle }}</div>
                                    <div class="col text-truncate d-none d-sm-block">
                                        <RouterLink :to="{ name: 'EmployeeDetails', params: { id: a.employeeId } }">{{ a.employeeName }}</RouterLink>
                                    </div>
                                    <div class="col-3 col-md-2 text-truncate small text-muted text-end">{{ fmtDate(a.assignedOn) }}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-lg-5">
                        <div class="ah-panel h-100">
                            <h2 class="ah-panel__title">{{ $t("assetsByLocation") }}</h2>
                            <RouterLink
                                v-for="l in data.byLocation"
                                :key="l.id"
                                :to="{ name: 'AssetOverview', query: { locationId: l.id } }"
                                class="d-flex justify-content-between border-bottom py-1 text-decoration-none text-body"
                            >
                                <span class="text-truncate"><Icon name="map" /> {{ l.title }}</span>
                                <span class="fw-semibold">{{ l.count }}</span>
                            </RouterLink>
                            <div class="small text-muted mt-2">{{ data.employees }} {{ $t("activeEmployees") }}</div>
                        </div>
                    </div>
                </div>
            </template>
        </LoadingContainer>

        <h2 class="ah-section-title">{{ $t("manage") }}</h2>
        <Dashboard />
    </section>
</template>
