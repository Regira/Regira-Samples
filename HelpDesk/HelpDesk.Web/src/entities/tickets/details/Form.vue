<template>
    <form @submit.prevent="handleSubmit">
        <div class="row form-toolbar align-items-center mb-2">
            <div class="col col-md-auto order-1">
                <FormButtonsRow
                    :item="item"
                    :readonly="readonly"
                    :feedback="feedback"
                    :show-delete="item?.id > 0 && canDelete(config.key)"
                    :labels="{ save: $t('save'), cancel: $t('cancel'), delete: $t('delete'), restore: $t('restore') }"
                    :modal-title="$t('delete')"
                    @cancel="handleCancel"
                    @remove="handleRemove"
                    @restore="handleRestore"
                >
                    <template #delete>{{ $t("deleteItem", { title: item?.$title }) }}</template>
                </FormButtonsRow>
            </div>
            <div class="col-auto order-2 order-md-3">
                <RouterLink v-if="isPopup" :to="{ name: `${config.key}Details`, params: { id: item.$id } }" target="_blank" class="btn btn-outline-secondary" :title="$t('popOut')">
                    <Icon name="popOut" />
                </RouterLink>
                <RouterLink v-else :to="overviewUrl || { name: 'TicketOverview' }" class="btn btn-outline-info">
                    <Icon name="list" /> <span class="d-none d-md-inline ms-1">{{ $t("overview") }}</span>
                </RouterLink>
            </div>
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <!-- ticket header -->
        <div class="mb-3">
            <div class="d-flex align-items-center flex-wrap gap-2 mb-1">
                <span v-if="item.code" class="hd-code">{{ item.code }}</span>
                <ColorBadge :title="item.status?.title" :color="item.status?.color" solid />
                <ColorBadge :title="item.priority?.title" :color="item.priority?.color" />
                <SlaIndicator :due-date="item.dueDate" :closed-at="item.closedAt" :is-closed="item.$isClosed" />
            </div>
            <h1 class="hd-page-title">{{ item.id ? item.title : $t("newTicket") }}</h1>
        </div>

        <div class="row g-3">
            <div :class="item.id ? 'col-lg-8' : 'col-12 col-xl-9'">
                <div class="hd-card p-3">
                    <TabContainer :tabs="tabs" :active="initialTab ?? (item.id ? 'conversation' : 'details')" :use-route-nav="!isPopup">
                        <template #conversation>
                            <div class="pt-3">
                                <Conversation v-model="item" :readonly="readonly" />
                            </div>
                        </template>

                        <template #details>
                            <div class="pt-3">
                                <FormSection :title="$t('request')" :readonly="readonly">
                                    <div class="mb-3">
                                        <FormLabel :label="$t('subject')" />
                                        <input v-model="item.title" :readonly="readonly" class="form-control" required maxlength="200" :placeholder="$t('subjectPlaceholder')" />
                                    </div>
                                    <div class="mb-3">
                                        <FormLabel :label="$t('description')" />
                                        <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="6" maxlength="8000" :placeholder="$t('descriptionPlaceholder')"></textarea>
                                    </div>
                                    <div class="mb-3">
                                        <FormLabel :label="$t('categories')" />
                                        <TicketCategoryOverview v-model="item.categories" />
                                    </div>
                                    <div class="mb-2" v-if="isStaff || !item.id">
                                        <FormLabel :label="$t('priority')" />
                                        <PrioritySelectorDropdown v-model="item.priority" v-model:idValue="item.priorityId" :disabled="readonly" />
                                        <small v-if="!isStaff" class="text-muted">{{ $t("priorityHintCustomer") }}</small>
                                    </div>
                                </FormSection>

                                <FormSection v-if="isStaff" :title="$t('handling')" :readonly="readonly" class="mt-3">
                                    <div class="row g-3">
                                        <div class="col-md-6">
                                            <FormLabel :label="$t('customer')" />
                                            <PersonInputSelector v-model="item.customer" v-model:idValue="item.customerId" :filter-defaults="{ role: 'Customer' }" :canEdit="canWrite('Person')" />
                                        </div>
                                        <div class="col-md-6">
                                            <FormLabel :label="$t('status')" />
                                            <StatusSelectorDropdown v-model="item.status" v-model:idValue="item.statusId" :disabled="readonly" />
                                        </div>
                                        <div class="col-md-6">
                                            <FormLabel :label="$t('supportTeam')" />
                                            <SupportTeamSelectorDropdown v-model="item.supportTeam" v-model:idValue="item.supportTeamId" :disabled="readonly" />
                                        </div>
                                        <div class="col-md-6">
                                            <FormLabel :label="$t('assignedEmployee')" />
                                            <PersonInputSelector
                                                v-model="item.assignedEmployee"
                                                v-model:idValue="item.assignedEmployeeId"
                                                :filter-defaults="assigneeFilter"
                                                :canEdit="false"
                                            />
                                        </div>
                                        <div class="col-md-6">
                                            <FormLabel :label="$t('dueDate')" />
                                            <DateInput v-model="item.dueDate" :readonly="readonly" show-time />
                                            <small class="text-muted">{{ $t("dueDateHint") }}</small>
                                        </div>
                                    </div>
                                </FormSection>
                            </div>
                        </template>

                        <template #files>
                            <div class="pt-3">
                                <EntityAttachments v-model="item.attachments" :readonly="readonly" />
                            </div>
                        </template>
                    </TabContainer>
                </div>
            </div>

            <!-- properties panel -->
            <div v-if="item.id" class="col-lg-4">
                <div class="hd-card p-3 mb-3">
                    <h2 class="h6 text-uppercase text-muted mb-3">{{ $t("properties") }}</h2>
                    <dl class="row small mb-0">
                        <dt class="col-5 fw-normal text-muted">{{ $t("status") }}</dt>
                        <dd class="col-7"><ColorBadge :title="item.status?.title" :color="item.status?.color" solid /></dd>
                        <dt class="col-5 fw-normal text-muted">{{ $t("priority") }}</dt>
                        <dd class="col-7"><ColorBadge :title="item.priority?.title" :color="item.priority?.color" /></dd>
                        <dt class="col-5 fw-normal text-muted">{{ $t("supportTeam") }}</dt>
                        <dd class="col-7"><ColorBadge :title="item.supportTeam?.title" :color="item.supportTeam?.color" /><span v-if="!item.supportTeam" class="text-muted">-</span></dd>
                        <dt class="col-5 fw-normal text-muted">{{ $t("assignedEmployee") }}</dt>
                        <dd class="col-7"><PersonChip :person="item.assignedEmployee" :show-button="isStaff" :muted="$t('unassigned')" /></dd>
                        <dt class="col-5 fw-normal text-muted">{{ $t("created") }}</dt>
                        <dd class="col-7">{{ formatDateTime(item.created, dateMask) }}</dd>
                        <dt class="col-5 fw-normal text-muted">{{ $t("dueDate") }}</dt>
                        <dd class="col-7">{{ item.dueDate ? formatDateTime(item.dueDate, dateMask) : "-" }}</dd>
                        <template v-if="item.firstResponseAt">
                            <dt class="col-5 fw-normal text-muted">{{ $t("firstResponse") }}</dt>
                            <dd class="col-7">{{ formatDateTime(item.firstResponseAt, dateMask) }}</dd>
                        </template>
                        <template v-if="item.closedAt">
                            <dt class="col-5 fw-normal text-muted">{{ $t("closedAt") }}</dt>
                            <dd class="col-7">{{ formatDateTime(item.closedAt, dateMask) }}</dd>
                        </template>
                    </dl>

                    <div v-if="isStaff && !readonly" class="border-top pt-3 mt-2">
                        <FormLabel :label="$t('quickStatus')" />
                        <div class="d-flex flex-wrap gap-1 mb-2">
                            <button
                                v-for="s in statuses"
                                :key="s.id"
                                type="button"
                                class="btn btn-sm"
                                :class="s.id === item.statusId ? 'btn-dark' : 'btn-outline-secondary'"
                                :disabled="feedback.isPending || s.id === item.statusId"
                                @click="quickStatus(s)"
                            >
                                {{ s.title }}
                            </button>
                        </div>
                        <button v-if="myPersonId && item.assignedEmployeeId !== myPersonId" type="button" class="btn btn-sm btn-outline-primary" :disabled="feedback.isPending" @click="assignToMe">
                            <i class="bi bi-person-check me-1"></i>{{ $t("assignToMe") }}
                        </button>
                    </div>
                </div>

                <div class="hd-card p-3">
                    <h2 class="h6 text-uppercase text-muted mb-3">{{ $t("customer") }}</h2>
                    <PersonChip :person="item.customer" :show-button="isStaff" />
                    <div class="small mt-2">
                        <div v-if="item.customer?.company"><i class="bi bi-building me-2 text-muted"></i>{{ item.customer.company }}</div>
                        <div v-if="item.customer?.email">
                            <i class="bi bi-envelope me-2 text-muted"></i><a :href="`mailto:${item.customer.email}`">{{ item.customer.email }}</a>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed, ref } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab, DateInput } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useLang } from "@regira/modules/vue/lang"
import { formatDateTime } from "@regira/modules/vue/formatters"
import { onAuthenticated } from "@regira/modules/vue/auth"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import Conversation from "./Conversation.vue"
import { TicketCategoryOverview } from "../ticket-categories"
import { Overview as EntityAttachments } from "../../entity-attachments"
import { InputSelector as PersonInputSelector } from "@/entities/persons"
import { SelectorDropdown as StatusSelectorDropdown, useEntityStore as useStatusStore, type Entity as Status } from "@/entities/statuses"
import { SelectorDropdown as PrioritySelectorDropdown } from "@/entities/priorities"
import { SelectorDropdown as SupportTeamSelectorDropdown } from "@/entities/support-teams"
import ColorBadge from "@/components/tickets/ColorBadge.vue"
import SlaIndicator from "@/components/tickets/SlaIndicator.vue"
import PersonChip from "@/components/tickets/PersonChip.vue"
import { useAccess } from "@/infrastructure/access"
import { useMeStore } from "@/infrastructure/me"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const { translate } = useLang()
const { isStaff, canWrite, canDelete } = useAccess()
const meStore = useMeStore()
const myPersonId = computed(() => meStore.me?.person?.id)
const dateMask = "dd/MM/yyyy HH:mm"

// employees of the ticket's team first; any employee when no team is set
const assigneeFilter = computed(() => (item.value.supportTeamId ? { role: "Employee", supportTeamId: item.value.supportTeamId } : { role: "Employee" }))

const tabs = computed(() => [
    item.value.id ? Tab.create("conversation", { icon: "chat", title: `${translate("conversation")} (${item.value.comments?.length ?? item.value.commentCount ?? 0})` }) : undefined,
    Tab.create("details", { icon: "form", title: translate("details"), isDefault: !item.value.id }),
    Tab.create("files", { icon: "attachment", title: `${translate("files")}${item.value.attachments?.length ? ` (${item.value.attachments.length})` : ""}` }),
])

// quick actions (staff): change the status / take the ticket, then save through the form (same feedback)
const statuses = ref<Array<Status>>([])
const statusStore = useStatusStore()
onAuthenticated(async () => {
    if (isStaff.value) statuses.value = statusStore.fromPool(await statusStore.list({ pageSize: 0 }))
})
async function quickStatus(status: Status) {
    item.value.statusId = status.id
    item.value.status = status
    await handleSubmit()
}
async function assignToMe() {
    if (!myPersonId.value) return
    item.value.assignedEmployeeId = myPersonId.value
    item.value.assignedEmployee = meStore.me?.person
    await handleSubmit()
}
</script>
