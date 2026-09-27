<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { RouterLink } from "vue-router"
import { Feedback, LoadingContainer, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useAxios } from "@regira/modules/vue/http"
// deep imports (not the barrels): the interventions slice already imports the invoices barrel (--rel Invoice)
import useInterventionStore from "@/entities/interventions/data/store"
import interventionConfig from "@/entities/interventions/config/config"
import type { Entity as Intervention } from "@/entities/interventions"
import { FormModalButton as VehicleButton, useEntityStore as useVehicleStore } from "@/entities/vehicles"
import { fmtDay, fmtMoney } from "@/infrastructure/domain"
import type Invoice from "../data/Entity"
import useEntityStore from "../data/store"

// Interventions are NOT owned by the invoice: each intervention writes its own invoiceId (one writer).
// Linking/unlinking PATCHes the intervention; the API's reactor then re-derives the invoice amounts,
// which are re-read here.
const props = defineProps<{ readonly?: boolean }>()
const item = defineModel<Invoice>({ required: true })

const axios = useAxios()
const feedback = useFeedback()
const { service: invoiceService } = useEntityStore()
const { service: interventionService, set: setIntervention } = useInterventionStore()
const { fromPool: getVehicle } = useVehicleStore()

const candidates = ref<Array<Intervention>>([])
const isLoading = ref(false)
const linked = computed(() => item.value.interventions ?? [])

async function loadCandidates() {
    if (!item.value.id || !item.value.supplierId || props.readonly) {
        candidates.value = []
        return
    }
    isLoading.value = true
    try {
        const result = await interventionService.search({
            supplierId: item.value.supplierId,
            status: "Completed",
            isInvoiced: false,
            sortBy: ["ScheduledDateDesc"],
            pageSize: 50,
        })
        candidates.value = result.items
    } catch (ex) {
        console.error(ex)
        feedback.fail("Loading billable interventions failed", toFeedbackError(ex))
    } finally {
        isLoading.value = false
    }
}

async function refreshInvoice() {
    const fresh = await invoiceService.details(item.value.id)
    if (fresh) {
        item.value.subTotal = fresh.subTotal
        item.value.vatAmount = fresh.vatAmount
        item.value.totalAmount = fresh.totalAmount
        item.value.interventions = fresh.interventions
        item.value.status = fresh.status
    }
}

async function setInvoice(interventionId: number, invoiceId: number | null, msg: string) {
    feedback.pending(msg)
    try {
        const { data } = await axios.patch(`${interventionConfig.api}/${interventionId}`, { invoiceId })
        if (data?.item) setIntervention(data.item) // write-through: keep the pooled intervention current
        await refreshInvoice()
        await loadCandidates()
        feedback.success(invoiceId ? "Intervention added to the invoice" : "Intervention removed from the invoice")
    } catch (ex) {
        console.error(ex)
        feedback.fail("Updating the intervention failed", toFeedbackError(ex))
    }
}

watch(() => [item.value.id, item.value.supplierId], loadCandidates, { immediate: true })
</script>

<template>
    <section>
        <Feedback :feedback="feedback" />
        <h6 class="fleet-section-title">{{ $t("billedInterventions") }} ({{ linked.length }})</h6>
        <div v-if="linked.length" class="entity-list fleet-table mb-4">
            <div class="row fleet-table__head">
                <div class="col-3 col-md-2">{{ $t("date") }}</div>
                <div class="col-3">{{ $t("vehicle") }}</div>
                <div class="col d-none d-md-block">{{ $t("interventionTypes") }}</div>
                <div class="col-3 col-md-2 text-end">{{ $t("cost") }}</div>
                <div class="col-auto" style="width: 3rem"></div>
            </div>
            <div v-for="row in linked" :key="row.id" class="row fleet-table__row align-items-center">
                <div class="col-3 col-md-2">
                    <RouterLink :to="{ name: 'InterventionDetails', params: { id: row.id } }">{{ fmtDay(row.completedDate ?? row.scheduledDate) }}</RouterLink>
                </div>
                <div class="col-3 text-truncate">
                    <VehicleButton v-if="row.vehicle" :model-value="getVehicle(row.vehicle)" />
                    <span class="fleet-plate">{{ row.vehicle?.licensePlate }}</span>
                </div>
                <div class="col d-none d-md-block text-truncate">{{ (row.lines ?? []).map((l) => l.interventionType?.title).join(", ") }}</div>
                <div class="col-3 col-md-2 text-end fleet-num">{{ fmtMoney(row.totalCost) }}</div>
                <div class="col-auto" style="width: 3rem">
                    <button v-if="!readonly" type="button" class="btn btn-sm btn-outline-danger" :title="$t('removeFromInvoice')" :disabled="feedback.isPending" @click="setInvoice(row.id, null, 'Removing...')">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>
            </div>
        </div>
        <p v-else class="italic-muted">{{ $t("noInterventionsOnInvoice") }}</p>

        <template v-if="!readonly && item.id">
            <h6 class="fleet-section-title">{{ $t("billableInterventions") }}</h6>
            <p class="text-secondary small">{{ $t("billableHelp") }}</p>
            <LoadingContainer :is-loading="isLoading">
                <div v-if="candidates.length" class="entity-list fleet-table">
                    <div v-for="row in candidates" :key="row.id" class="row fleet-table__row align-items-center">
                        <div class="col-3 col-md-2">{{ fmtDay(row.completedDate ?? row.scheduledDate) }}</div>
                        <div class="col-3"><span class="fleet-plate">{{ row.vehicle?.licensePlate }}</span></div>
                        <div class="col d-none d-md-block text-truncate">{{ (row.lines ?? []).map((l) => l.interventionType?.title).join(", ") }}</div>
                        <div class="col-3 col-md-2 text-end fleet-num">{{ fmtMoney(row.totalCost) }}</div>
                        <div class="col-auto" style="width: 3rem">
                            <button type="button" class="btn btn-sm btn-outline-primary" :title="$t('addToInvoice')" :disabled="feedback.isPending" @click="setInvoice(row.id, item.id, 'Adding...')">
                                <i class="bi bi-plus-lg"></i>
                            </button>
                        </div>
                    </div>
                </div>
                <p v-else-if="!isLoading" class="italic-muted">{{ $t("noBillableInterventions") }}</p>
            </LoadingContainer>
        </template>
    </section>
</template>
