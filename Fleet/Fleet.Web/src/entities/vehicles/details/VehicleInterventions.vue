<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { RouterLink } from "vue-router"
import { LoadingContainer, Paging, useFeedback, Feedback, toFeedbackError } from "@regira/modules/vue/ui"
import { PagingInfo } from "@regira/modules/vue/entities"
// deep imports (not the barrel): the interventions slice already imports the vehicles barrel (--rel Vehicle),
// so the second edge goes straight to the module to avoid a barrel cycle
import useInterventionStore from "@/entities/interventions/data/store"
import type { Entity as Intervention } from "@/entities/interventions"
import StatusBadge from "@/components/StatusBadge.vue"
import { fmtDay, fmtKm, fmtMoney, interventionStatusBadge, priorityBadge } from "@/infrastructure/domain"

const props = defineProps<{ vehicleId: number }>()

const { service } = useInterventionStore()
const feedback = useFeedback()
const items = ref<Array<Intervention>>([])
const count = ref(0)
const isLoading = ref(false)
const pagingInfo = ref(new PagingInfo(15, 1))
const completedCost = computed(() => items.value.filter((x) => x.status === "Completed").reduce((sum, x) => sum + (x.totalCost || 0), 0))

async function load() {
    if (!props.vehicleId) return
    isLoading.value = true
    try {
        const result = await service.search({
            vehicleId: props.vehicleId,
            includes: ["Lines"],
            sortBy: ["ScheduledDateDesc"],
            pageSize: pagingInfo.value.pageSize,
            page: pagingInfo.value.page,
        })
        items.value = result.items
        count.value = result.count ?? result.items.length
    } catch (ex) {
        console.error(ex)
        feedback.fail("Loading the intervention history failed", toFeedbackError(ex))
    } finally {
        isLoading.value = false
    }
}
watch(() => props.vehicleId, load, { immediate: true })
</script>

<template>
    <section>
        <div class="d-flex flex-wrap align-items-center gap-2 mb-3">
            <h5 class="mb-0 me-auto">{{ count }} {{ $t("interventions") }}</h5>
            <span class="text-secondary small">{{ $t("completedCostOnPage") }}: <strong class="fleet-num">{{ fmtMoney(completedCost) }}</strong></span>
            <RouterLink :to="{ name: 'InterventionDetails', params: { id: 'new' }, query: { vehicleId: vehicleId } }" class="btn btn-sm btn-primary">
                <i class="bi bi-plus-lg me-1"></i>{{ $t("planIntervention") }}
            </RouterLink>
        </div>
        <Feedback :feedback="feedback" />
        <LoadingContainer :is-loading="isLoading">
            <div v-if="items.length" class="entity-list fleet-table">
                <div class="row fleet-table__head">
                    <div class="col-3 col-md-2">{{ $t("date") }}</div>
                    <div class="col">{{ $t("interventionTypes") }}</div>
                    <div class="col-3 d-none d-md-block">{{ $t("supplier") }}</div>
                    <div class="col-2 d-none d-lg-block text-end">{{ $t("mileage") }}</div>
                    <div class="col-auto fleet-col-status">{{ $t("status") }}</div>
                    <div class="col-3 col-md-2 text-end">{{ $t("cost") }}</div>
                </div>
                <div v-for="row in items" :key="row.id" class="row fleet-table__row align-items-center">
                    <div class="col-3 col-md-2">
                        <RouterLink :to="{ name: 'InterventionDetails', params: { id: row.id } }">{{ fmtDay(row.scheduledDate) }}</RouterLink>
                    </div>
                    <div class="col text-truncate">
                        <StatusBadge v-if="row.priority === 'Urgent' || row.priority === 'High'" :value="row.priority" :map="priorityBadge" :compact="true" class="me-1" />
                        {{ (row.lines ?? []).map((l) => l.interventionType?.title).join(", ") }}
                    </div>
                    <div class="col-3 d-none d-md-block text-truncate">{{ row.supplier?.title }}</div>
                    <div class="col-2 d-none d-lg-block text-end fleet-num">{{ fmtKm(row.mileage) }}</div>
                    <div class="col-auto fleet-col-status"><StatusBadge :value="row.status" :map="interventionStatusBadge" :compact="true" /></div>
                    <div class="col-3 col-md-2 text-end fleet-num">{{ fmtMoney(row.totalCost) }}</div>
                </div>
            </div>
            <p v-else-if="!isLoading" class="italic-muted">{{ $t("noResults") }}</p>
        </LoadingContainer>
        <Paging v-if="count > pagingInfo.pageSize!" class="mt-2" v-model="pagingInfo" :count="count" @change="load" />
    </section>
</template>
