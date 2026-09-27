<!-- Approval workflow of a credit request: status timeline, balance preview and the transitions
     the signed-in user may perform (submit / withdraw / cancel; approve / reject for administrators). -->
<script setup lang="ts">
import { computed, ref, watch } from "vue"
import type { AxiosInstance } from "axios"
import { get } from "@regira/modules/vue/ioc"
import { Feedback, FormSection, ConfirmButton, ModalType, injectModal, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { formatDateTime } from "@regira/modules/vue/formatters"
import { useLang } from "@regira/modules/vue/lang"
import { CreditBar, StatusBadge } from "@/components/credits"
import { RequestStatus, formatCredits } from "@/domain/enums"
import { useAccess } from "@/access"
import type Entity from "../data/Entity"
import type { EntityService, WorkflowAction } from "../data/EntityService"
import useEntityStore from "../data/store"

interface Balance {
    freeCredits: number
    usedCredits: number
    pendingCredits: number
    remainingCredits: number
    minBalance: number
}

const props = defineProps<{ item: Entity; isDirty?: boolean }>()
const emit = defineEmits<{ (e: "changed", value: Entity): void; (e: "request-save"): void }>()

const Modal = injectModal()
const { translate } = useLang()
const { isAdmin } = useAccess()
const store = useEntityStore()
const feedback = useFeedback()
const balance = ref<Balance>()

const status = computed(() => props.item.status)
const isSaved = computed(() => props.item.id > 0)
const canSubmit = computed(() => isSaved.value && status.value === RequestStatus.Draft)
const canWithdraw = computed(() => status.value === RequestStatus.Submitted)
const canDecide = computed(() => isAdmin() && status.value === RequestStatus.Submitted)
const canCancel = computed(
    () => isSaved.value && (status.value === RequestStatus.Draft || status.value === RequestStatus.Submitted || (isAdmin() && status.value === RequestStatus.Approved))
)

// the balance of the request's employee and year (an employee's own allocation is row-scoped server-side)
async function loadBalance() {
    balance.value = undefined
    const params: Record<string, any> = { year: props.item.year, pageSize: 1 }
    if (props.item.employeeId) params.employeeId = props.item.employeeId
    else if (isAdmin()) return
    try {
        const axios = get<AxiosInstance>("axios")!
        const { data } = await axios.get("/credit-allocations/search", { params })
        balance.value = data.items?.[0]
    } catch {
        balance.value = undefined
    }
}
watch(() => [props.item.employeeId, props.item.year, props.item.status], loadBalance, { immediate: true })

const requestCredits = computed(() => (props.item.items ?? []).filter((x) => !x._deleted).reduce((s, x) => s + (Number(x.credits) || 0), 0))
// what the balance would be once this request is approved (pending requests of others excluded)
const projected = computed(() => {
    if (!balance.value) return undefined
    const alreadyCounted = status.value === RequestStatus.Approved
    return balance.value.remainingCredits - (alreadyCounted ? 0 : requestCredits.value)
})
const exceeds = computed(() => balance.value != null && projected.value != null && projected.value < balance.value.minBalance)

// decision modal (approve / reject with a comment)
const decision = ref<"approve" | "reject">()
const comment = ref("")
function openDecision(kind: "approve" | "reject") {
    comment.value = ""
    decision.value = kind
}

async function run(action: WorkflowAction, withComment?: string) {
    if (action === "submit" && props.isDirty) {
        feedback.fail(translate("saveBeforeSubmit"))
        return
    }
    feedback.pending(translate("processing"))
    try {
        const service = get<EntityService>("CreditRequest")!
        const updated = await service.transition(props.item.id, action, withComment)
        store.set(updated) // write the fresh row into the shared pool
        emit("changed", updated)
        decision.value = undefined
        feedback.success(translate(`workflowDone${action}`))
    } catch (ex) {
        console.error(ex)
        feedback.fail(translate("workflowFailed"), toFeedbackError(ex))
    }
}
function submitDecision() {
    if (!decision.value) return
    run(decision.value, comment.value)
}
</script>

<template>
    <FormSection :title="$t('approvalWorkflow')" class="qc-workflow">
        <div class="d-flex align-items-center gap-2 mb-3">
            <StatusBadge :status="status" class="fs-6" />
            <span class="text-muted small" v-if="item.totalCredits">{{ formatCredits(item.totalCredits) }} {{ $t("credits") }}</span>
        </div>

        <!-- timeline -->
        <ul class="list-unstyled qc-timeline small mb-3">
            <li v-if="item.created"><i class="bi bi-circle-fill text-secondary"></i>{{ $t("createdOn") }} {{ formatDateTime(item.created, "dd/MM/yyyy HH:mm") }}</li>
            <li v-if="item.submittedAt"><i class="bi bi-circle-fill text-warning"></i>{{ $t("submittedOn") }} {{ formatDateTime(new Date(item.submittedAt), "dd/MM/yyyy HH:mm") }}</li>
            <li v-if="item.decidedAt">
                <i class="bi bi-circle-fill" :class="status === 'Approved' ? 'text-success' : 'text-danger'"></i>{{ $t(`decidedAs${status}`) }}
                {{ formatDateTime(new Date(item.decidedAt), "dd/MM/yyyy HH:mm") }} <span v-if="item.decidedBy">{{ $t("by") }} {{ item.decidedBy }}</span>
                <div v-if="item.decisionComment" class="fst-italic text-muted mt-1">"{{ item.decisionComment }}"</div>
            </li>
        </ul>

        <!-- balance preview -->
        <div v-if="balance" class="mb-3">
            <div class="d-flex justify-content-between small mb-1">
                <span class="text-muted">{{ $t("balanceYear", { year: item.year }) }}</span>
                <span>{{ formatCredits(balance.remainingCredits) }} {{ $t("left") }}</span>
            </div>
            <CreditBar :free="balance.freeCredits" :used="balance.usedCredits" :pending="balance.pendingCredits" :min-balance="balance.minBalance" compact />
            <div class="small mt-2" :class="exceeds ? 'text-danger fw-semibold' : 'text-muted'">
                <i :class="exceeds ? 'bi bi-exclamation-triangle' : 'bi bi-calculator'" class="me-1"></i>
                {{ $t("afterApproval") }}: {{ formatCredits(projected) }}
                <span v-if="exceeds">({{ $t("belowMinimum", { min: balance.minBalance }) }})</span>
            </div>
        </div>
        <p v-else-if="item.employeeId || !isAdmin()" class="small text-muted"><i class="bi bi-info-circle me-1"></i>{{ $t("noAllocation", { year: item.year }) }}</p>

        <!-- actions -->
        <div class="d-flex flex-wrap gap-2">
            <button v-if="canSubmit" type="button" class="btn btn-primary" :disabled="feedback.isPending || !item.items?.length" @click="run('submit')">
                <i class="bi bi-send me-1"></i>{{ $t("submitForApproval") }}
            </button>
            <button v-if="canDecide" type="button" class="btn btn-success" :disabled="feedback.isPending" @click="openDecision('approve')"><i class="bi bi-check-lg me-1"></i>{{ $t("approve") }}</button>
            <button v-if="canDecide" type="button" class="btn btn-danger" :disabled="feedback.isPending" @click="openDecision('reject')"><i class="bi bi-x-lg me-1"></i>{{ $t("reject") }}</button>
            <button v-if="canWithdraw" type="button" class="btn btn-outline-secondary" :disabled="feedback.isPending" @click="run('withdraw')">
                <i class="bi bi-arrow-counterclockwise me-1"></i>{{ $t("withdraw") }}
            </button>
            <ConfirmButton
                v-if="canCancel"
                icon="bi bi-slash-circle"
                class="btn btn-outline-danger"
                :button-label="$t('cancelRequest')"
                :modal-title="$t('cancelRequest')"
                :modal-type="ModalType.warning"
                :modal-labels="{ cancel: $t('no'), submit: $t('yesCancel') }"
                @confirm="run('cancel')"
            >
                {{ status === "Approved" ? $t("cancelApprovedConfirm") : $t("cancelConfirm") }}
            </ConfirmButton>
        </div>
        <p v-if="canSubmit && !item.items?.length" class="small text-muted mt-2 mb-0">{{ $t("addItemsFirst") }}</p>
        <Feedback :feedback="feedback" class="mt-2" />

        <Teleport to="#modals">
            <component
                :is="Modal"
                :is-visible="decision != null"
                :title="decision === 'approve' ? $t('approveRequest') : $t('rejectRequest')"
                :type="decision === 'approve' ? ModalType.success : ModalType.danger"
                :labels="{ cancel: $t('cancel'), submit: decision === 'approve' ? $t('approve') : $t('reject') }"
                @close="decision = undefined"
                @cancel="decision = undefined"
                @submit="submitDecision"
            >
                <p class="mb-2">
                    <strong>{{ item.title }}</strong> &mdash; {{ formatCredits(item.totalCredits) }} {{ $t("credits") }}
                </p>
                <p v-if="exceeds && decision === 'approve'" class="text-danger small">{{ $t("belowMinimum", { min: balance?.minBalance }) }}</p>
                <label class="form-label" for="decisionComment">{{ decision === "reject" ? $t("rejectReason") : $t("commentOptional") }}</label>
                <textarea id="decisionComment" v-model="comment" class="form-control" rows="3" maxlength="1000"></textarea>
                <Feedback :feedback="feedback" class="mt-2" />
            </component>
        </Teleport>
    </FormSection>
</template>

<style scoped>
.qc-timeline li {
    position: relative;
    padding-left: 1.25rem;
    margin-bottom: 0.4rem;
}
.qc-timeline li > i {
    position: absolute;
    left: 0;
    top: 0.2rem;
    font-size: 0.55rem;
}
</style>
