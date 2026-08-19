<template>
    <form @submit.prevent="handleSubmit">
        <div class="row form-toolbar align-items-center mb-3">
            <div class="col col-md-auto order-1">
                <FormButtonsRow
                    :item="item"
                    :readonly="readonly || !isEditable"
                    :feedback="feedback"
                    :show-delete="item?.id > 0"
                    @cancel="handleCancel"
                    @remove="handleRemove"
                    @restore="handleRestore"
                />
            </div>
            <div class="col-auto order-2 order-md-3">
                <RouterLink
                    v-if="isPopup"
                    :to="{ name: `${config.key}Details`, params: { id: item.$id } }"
                    target="_blank"
                    class="btn btn-outline-secondary"
                    :title="$t('popOut')"
                >
                    <Icon name="popOut" />
                </RouterLink>
                <RouterLink v-else-if="overviewUrl" :to="overviewUrl" class="btn btn-outline-info">
                    <Icon name="list" /> <span class="d-none d-md-inline ms-1">{{ $t("overview") }}</span>
                </RouterLink>
            </div>
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <div class="mb-3">
            <span class="badge fs-6" :class="statusBadgeClass">{{ $t(item.status.toLowerCase()) }}</span>
            <span class="ms-2 text-muted">{{ $t("totalCredits") }}: <strong>{{ item.totalCredits }}</strong> QCredits</span>
        </div>

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly || !isEditable">
                    <div class="row">
                        <div class="col-md mb-2">
                            <EmployeeInputSelector
                                v-model="item.employee"
                                v-model:idValue="item.employeeId"
                                :readonly="readonly || !isEditable"
                                :placeholder="$t('employee')"
                            />
                            <FormLabel :label="$t('employee')" />
                        </div>
                        <div class="col-md mb-2">
                            <input v-model.number="item.year" type="number" min="2000" max="2100" :readonly="readonly || !isEditable" class="form-control" />
                            <FormLabel :label="$t('year')" />
                        </div>
                    </div>

                    <div class="row" v-if="item.id > 0">
                        <div class="col-md mb-2">
                            <div class="form-control-plaintext">{{ formatDateTime(item.submittedDate) }}</div>
                            <FormLabel :label="$t('submittedDate')" />
                        </div>
                        <div class="col-md mb-2" v-if="item.decisionDate">
                            <div class="form-control-plaintext">{{ formatDateTime(item.decisionDate) }}</div>
                            <FormLabel :label="$t('decisionDate')" />
                        </div>
                    </div>
                    <div class="row" v-if="item.approver">
                        <div class="col-md mb-2">
                            <ApproverButton :model-value="item.approver" /> {{ item.approver.$title }}
                            <FormLabel :label="$t('approver')" />
                        </div>
                        <div class="col-md mb-2" v-if="item.decisionNotes">
                            <div class="form-control-plaintext">{{ item.decisionNotes }}</div>
                            <FormLabel :label="$t('decisionNotes')" />
                        </div>
                    </div>
                </FormSection>

                <FormSection v-if="item.status === RequestStatuses.Pending && item.id > 0" :title="$t('approvalDecision')">
                    <div class="row">
                        <div class="col-md mb-2">
                            <EmployeeInputSelector
                                v-model="decisionApprover"
                                v-model:idValue="decisionInput.approverId"
                                :filter-defaults="{ role: 'Admin' }"
                                :placeholder="$t('chooseApprover')"
                            />
                            <FormLabel :label="$t('approver')" />
                        </div>
                        <div class="col-md mb-2">
                            <input v-model="decisionInput.notes" class="form-control" :placeholder="$t('decisionNotes')" />
                            <FormLabel :label="$t('decisionNotes')" />
                        </div>
                    </div>
                    <Feedback :feedback="decisionFeedback" />
                    <div class="mt-2">
                        <button type="button" class="btn btn-success me-2" :disabled="!decisionInput.approverId || isDeciding" @click="approve">
                            <Icon name="selected" /> {{ $t("approve") }}
                        </button>
                        <button type="button" class="btn btn-outline-danger" :disabled="!decisionInput.approverId || isDeciding" @click="reject">
                            <Icon name="clear" /> {{ $t("reject") }}
                        </button>
                    </div>
                </FormSection>
            </template>
            <template #items>
                <FormSection :title="$t('purchasesAndActivities')">
                    <QCreditRequestItemOverview v-model="item.items" :readonly="readonly || !isEditable" />
                </FormSection>
            </template>
        </TabContainer>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab, useFeedback } from "@regira/modules/vue/ui"
import { formatDateTime } from "@regira/modules/vue/formatters"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { get } from "@regira/modules/vue/ioc"
import config from "../config/config"
import Entity, { RequestStatuses } from "../data/Entity"
import useEntityStore from "../data/store"
import EntityService from "../data/EntityService"
import { InputSelector as EmployeeInputSelector, FormModalButton as ApproverButton, type Entity as Employee } from "@/entities/employees"
import { QCreditRequestItemOverview } from "../q-credit-request-items"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
// custom endpoints (approve/reject) live on the raw service, not the pooled store handler
const rawService = get<EntityService>(Entity.name)!

const isEditable = computed(() => item.value.status === RequestStatuses.Pending)
const statusBadgeClass = computed(() => {
    switch (item.value.status) {
        case RequestStatuses.Approved:
            return "bg-success"
        case RequestStatuses.Rejected:
            return "bg-danger"
        default:
            return "bg-warning text-dark"
    }
})

const tabs = computed(() => [
    Tab.create("form", { icon: "form", title: "General", isDefault: true }),
    Tab.create("items", { icon: "list", title: "Purchases & activities" })
])

// Approve / reject panel (no auth in this demo app - the admin picks themselves from the Admin-only picker)
const decisionApprover = ref<Employee>()
const decisionInput = reactive<{ approverId?: number; notes?: string }>({})
const decisionFeedback = useFeedback()
const isDeciding = ref(false)

async function approve() {
    if (!decisionInput.approverId) return
    isDeciding.value = true
    decisionFeedback.pending("Approving...")
    try {
        const result = await rawService.approve(item.value.id, { approverId: decisionInput.approverId, notes: decisionInput.notes })
        item.value.status = result.status as Entity["status"]
        item.value.decisionDate = result.decisionDate ? new Date(result.decisionDate) : undefined
        item.value.approverId = result.approverId
        item.value.approver = decisionApprover.value
        item.value.decisionNotes = result.decisionNotes
        decisionFeedback.success("Request approved.")
    } catch (e: any) {
        decisionFeedback.fail("Could not approve this request.", e?.response?.data)
    } finally {
        isDeciding.value = false
    }
}
async function reject() {
    if (!decisionInput.approverId) return
    isDeciding.value = true
    decisionFeedback.pending("Rejecting...")
    try {
        const result = await rawService.reject(item.value.id, { approverId: decisionInput.approverId, notes: decisionInput.notes })
        item.value.status = result.status as Entity["status"]
        item.value.decisionDate = result.decisionDate ? new Date(result.decisionDate) : undefined
        item.value.approverId = result.approverId
        item.value.approver = decisionApprover.value
        item.value.decisionNotes = result.decisionNotes
        decisionFeedback.success("Request rejected.")
    } catch (e: any) {
        decisionFeedback.fail("Could not reject this request.", e?.response?.data)
    } finally {
        isDeciding.value = false
    }
}
</script>
