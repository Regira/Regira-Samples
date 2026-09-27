<!-- Administrator actions on yearly allocations: generate a year for all active employees,
     or close a year and carry the remaining balances over to the next one. -->
<script setup lang="ts">
import { ref } from "vue"
import type { AxiosInstance } from "axios"
import { get } from "@regira/modules/vue/ioc"
import { ConfirmButton, Feedback, ModalType, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { yearOptions } from "@/domain/enums"

const emit = defineEmits<{ (e: "done"): void }>()
const year = ref(new Date().getFullYear())
const feedback = useFeedback()

async function run(action: "generate" | "rollover") {
    feedback.pending(action === "generate" ? "Generating allocations..." : "Rolling balances over...")
    try {
        const axios = get<AxiosInstance>("axios")!
        const { data } = await axios.post(`/credit-allocations/${action}`, { year: year.value })
        feedback.success(data.message)
        emit("done")
    } catch (ex) {
        console.error(ex)
        feedback.fail(action === "generate" ? "Generating allocations failed" : "Roll-over failed", toFeedbackError(ex))
    }
}
</script>

<template>
    <div class="qc-admin-actions card card-body bg-body-tertiary border-0 mb-3 py-2">
        <div class="row g-2 align-items-center">
            <div class="col-auto fw-semibold small text-uppercase text-muted"><i class="bi bi-gear me-1"></i>{{ $t("yearActions") }}</div>
            <div class="col-auto">
                <select v-model.number="year" class="form-select form-select-sm">
                    <option v-for="y in yearOptions()" :key="y" :value="y">{{ y }}</option>
                </select>
            </div>
            <div class="col-auto">
                <ConfirmButton
                    icon="new"
                    class="btn btn-sm btn-outline-primary"
                    :button-label="$t('generateAllocations')"
                    :modal-title="$t('generateAllocations')"
                    :modal-labels="{ cancel: $t('cancel'), submit: $t('generate') }"
                    :disabled="feedback.isPending"
                    @confirm="run('generate')"
                >
                    {{ $t("generateAllocationsConfirm", { year }) }}
                </ConfirmButton>
            </div>
            <div class="col-auto">
                <ConfirmButton
                    icon="bi bi-arrow-right-circle"
                    class="btn btn-sm btn-outline-warning"
                    :button-label="$t('rollover')"
                    :modal-title="$t('rollover')"
                    :modal-type="ModalType.warning"
                    :modal-labels="{ cancel: $t('cancel'), submit: $t('rollover') }"
                    :disabled="feedback.isPending"
                    @confirm="run('rollover')"
                >
                    {{ $t("rolloverConfirm", { year, next: year + 1 }) }}
                </ConfirmButton>
            </div>
            <div class="col"><Feedback :feedback="feedback" /></div>
        </div>
    </div>
</template>
