<!-- Current holder + assign / return actions + complete assignment history.
     The actions are domain endpoints (POST assets/{id}/assign | return), not part of the form's save. -->
<template>
    <div>
        <FormSection :title="$t('currentAssignment')">
            <div class="row g-3 align-items-stretch">
                <div class="col-lg-5">
                    <div class="ah-holder h-100" :class="{ 'ah-holder--empty': !item.currentEmployeeId }">
                        <template v-if="item.currentEmployeeId">
                            <span class="ah-avatar ah-avatar--lg">{{ holder?.$initials }}</span>
                            <div class="min-w-0">
                                <div class="fw-semibold text-truncate">
                                    <EmployeeButton :model-value="holder" class="p-0 me-1" />{{ holder?.$title }}
                                </div>
                                <div class="small text-muted text-truncate">{{ holder?.department }} &middot; {{ holder?.email }}</div>
                                <div class="small">{{ $t("since") }} {{ fmtDate(item.assignedOn) }} ({{ durationDays(item.assignedOn) }} {{ $t("days") }})</div>
                            </div>
                        </template>
                        <template v-else>
                            <i class="bi bi-inbox fs-3 text-muted"></i>
                            <div class="text-muted">{{ $t("notAssigned") }}</div>
                        </template>
                    </div>
                </div>
                <div v-if="canAct" class="col-lg-7">
                    <div class="border rounded p-2 h-100">
                        <div class="row g-2">
                            <div class="col-12">
                                <EmployeeSelector
                                    v-model="selectedEmployee"
                                    v-model:idValue="selectedEmployeeId as number"
                                    :canEdit="false"
                                    :filter-defaults="{ isActive: true, exclude: item.currentEmployeeId ? [item.currentEmployeeId] : [] }"
                                    :placeholder="$t('selectEmployee')"
                                />
                            </div>
                            <div class="col-12">
                                <input v-model="notes" maxlength="512" class="form-control form-control-sm" :placeholder="$t('handoverNotes')" />
                            </div>
                            <div class="col-12 d-flex flex-wrap gap-2 align-items-center">
                                <button type="button" class="btn btn-primary btn-sm" :disabled="!selectedEmployeeId || feedback.isPending || !assignable" @click="assign">
                                    <Icon name="user" /> {{ item.currentEmployeeId ? $t("reassign") : $t("assign") }}
                                </button>
                                <ConfirmButton
                                    v-if="item.currentEmployeeId"
                                    icon="restore"
                                    class="btn-outline-secondary btn-sm"
                                    :button-label="$t('returnAsset')"
                                    :modal-title="$t('returnAsset')"
                                    :modal-labels="{ cancel: $t('cancel'), submit: $t('returnAsset') }"
                                    @confirm="returnAsset"
                                >
                                    {{ $t("returnConfirm", { title: item.$title, holder: holder?.$title }) }}
                                </ConfirmButton>
                                <span v-if="!assignable" class="small text-warning-emphasis">{{ $t("notAssignableHint") }}</span>
                            </div>
                        </div>
                        <Feedback :feedback="feedback" class="mt-2" />
                    </div>
                </div>
            </div>
        </FormSection>

        <FormSection :title="$t('assignmentHistory')">
            <div class="entity-list ah-table">
                <div class="row fw-bold border-bottom pb-1">
                    <div class="col">{{ $t("employee") }}</div>
                    <div class="col-3 col-md-2">{{ $t("assignedOn") }}</div>
                    <div class="col-3 col-md-2">{{ $t("returnedOn") }}</div>
                    <div class="col-1 d-none d-md-block text-end">{{ $t("days") }}</div>
                    <div class="col d-none d-lg-block">{{ $t("notes") }}</div>
                </div>
                <div v-for="a in history" :key="a.id" class="row border-bottom py-1 align-items-center" :class="{ 'ah-row--current': !a.returnedOn }">
                    <div class="col text-truncate">
                        <EmployeeButton v-if="a.employee" :model-value="getEmployee(a.employee)" class="p-0 me-1" />{{ getEmployee(a.employee)?.$title }}
                    </div>
                    <div class="col-3 col-md-2 text-truncate">{{ fmtDate(a.assignedOn) }}</div>
                    <div class="col-3 col-md-2 text-truncate">
                        <span v-if="a.returnedOn">{{ fmtDate(a.returnedOn) }}</span>
                        <span v-else class="badge text-bg-primary">{{ $t("current") }}</span>
                    </div>
                    <div class="col-1 d-none d-md-block text-end">{{ durationDays(a.assignedOn, a.returnedOn) }}</div>
                    <div class="col d-none d-lg-block text-truncate text-muted small">{{ [a.notes, a.returnNotes].filter(Boolean).join(" / ") }}</div>
                </div>
                <p v-if="!history.length" class="italic-muted my-2">{{ $t("neverAssigned") }}</p>
            </div>
        </FormSection>
    </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue"
import { get } from "@regira/modules/vue/ioc"
import { useLang } from "@regira/modules/vue/lang"
import { Feedback, FormSection, Icon, ConfirmButton, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { InputSelector as EmployeeSelector, FormModalButton as EmployeeButton, useEntityStore as useEmployeeStore } from "@/entities/employees"
import type { Entity as Employee } from "@/entities/employees"
import { fmtDate, durationDays } from "@/utilities/format"
import Entity from "../data/Entity"
import type EntityService from "../data/EntityService"
import useEntityStore from "../data/store"

const props = defineProps<{ item: Entity; readonly?: boolean }>()
const emit = defineEmits<{ (e: "changed", fresh: Entity): void }>()

const { translate } = useLang()
const { fromPool: getEmployee } = useEmployeeStore()
const { set: setPooled } = useEntityStore()
const service = get<EntityService>(Entity.name)! // custom endpoints live on the raw service, not the pooled store
const feedback = useFeedback()

const holder = computed(() => (props.item.currentEmployee ? getEmployee(props.item.currentEmployee) : undefined))
const history = computed(() => [...(props.item.assignments ?? [])].sort((a, b) => (b.assignedOn || "").localeCompare(a.assignedOn || "")))
const canAct = computed(() => !props.readonly && props.item.id > 0)
const assignable = computed(() => !["Inactive", "Maintenance"].includes(props.item.status?.kind ?? ""))

const selectedEmployee = ref<Employee>()
const selectedEmployeeId = ref<number>()
const notes = ref<string>()

async function run(action: () => Promise<Entity>, message: string) {
    feedback.pending(translate("saving"))
    try {
        const fresh = await action()
        setPooled(fresh) // refresh the shared cache so lists relabel
        emit("changed", fresh)
        selectedEmployee.value = undefined
        selectedEmployeeId.value = undefined
        notes.value = undefined
        feedback.success(message)
    } catch (ex: any) {
        console.error(ex)
        feedback.fail(translate("actionFailed"), toFeedbackError(ex) ?? ex?.message)
    }
}
const assign = () => run(() => service.assign(props.item.id, selectedEmployeeId.value!, notes.value), translate("assetAssigned"))
const returnAsset = () => run(() => service.returnAsset(props.item.id, notes.value), translate("assetReturned"))
</script>
