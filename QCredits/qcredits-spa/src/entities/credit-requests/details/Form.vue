<template>
    <!-- Built-ins are the slice defaults — hand-rolling feedback/buttons/tabs/debug/owned-row editors is a deviation (see entities.card). -->
    <form @submit.prevent="handleSubmit">
        <!-- Action bar: save/delete buttons, the back-to-overview link (a page form must offer the way back), feedback. -->
        <!-- order-*: on md+ the overview / pop-out link moves to the END of the row (order-md-3) and the
             feedback fills the middle — without them both land mid-row, next to the save buttons. -->
        <div class="row form-toolbar align-items-center mb-3">
            <div class="col col-md-auto order-1">
                <FormButtonsRow
                    :item="item"
                    :readonly="locked"
                    :feedback="feedback"
                    :show-delete="item?.id > 0 && item.isDraft"
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
                <!-- In a modal (isPopup) there is no overview to return to — offer a pop-out to the full page instead. -->
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
            <!-- useForm drives `feedback` (Saving… → Saved / 400 field-map); render it here or the save shows nothing. -->
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <div v-if="item.id > 0 && !item.isDraft && !readonly" class="alert alert-light border small py-2">
            <i class="bi bi-lock me-1"></i>{{ $t("lockedExplanation") }}
        </div>
        <div class="row g-3">
            <div class="col-xl-8">
                <FormSection :title="$t('request')" :readonly="locked">
                    <div class="row g-3">
                        <div class="col-md-8" v-if="isAdmin()">
                            <FormLabel :label="$t('employee')" />
                            <EmployeeInputSelector v-model="item.employee" v-model:idValue="item.employeeId" :readonly="locked || item.id > 0" :canEdit="false" />
                        </div>
                        <div class="col-md-8" v-else-if="item.employee">
                            <FormLabel :label="$t('employee')" />
                            <input :value="`${item.employee.firstName} ${item.employee.lastName}`" class="form-control" readonly />
                        </div>
                        <div class="col-md-4">
                            <FormLabel :label="$t('year')" />
                            <select v-model.number="item.year" class="form-select" :disabled="locked">
                                <option v-for="y in yearOptions()" :key="y" :value="y">{{ y }}</option>
                            </select>
                        </div>
                        <div class="col-12">
                            <FormLabel :label="$t('title')" />
                            <input v-model="item.title" :readonly="locked" class="form-control" required maxlength="150" :placeholder="$t('titlePlaceholder')" />
                        </div>
                        <div class="col-12">
                            <FormLabel :label="$t('motivation')" />
                            <textarea v-model="item.description" :readonly="locked" class="form-control" rows="3" maxlength="2000" :placeholder="$t('motivationPlaceholder')"></textarea>
                        </div>
                    </div>
                </FormSection>
                <FormSection :title="$t('purchasesAndActivities')" class="mt-3">
                    <CreditRequestItemOverview v-model="item.items" :readonly="locked" />
                </FormSection>
            </div>
            <div class="col-xl-4">
                <WorkflowPanel :item="item" :is-dirty="isDirty" @changed="handleChanged" />
            </div>
        </div>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { computed, ref, watch } from "vue"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import { yearOptions } from "@/domain/enums"
import { useAccess } from "@/access"
import { CreditRequestItemOverview } from "../credit-request-items"
import WorkflowPanel from "./WorkflowPanel.vue"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
const { isAdmin } = useAccess()
// only drafts are editable (the API enforces the same rule in CreditRequestGuard)
const locked = computed(() => props.readonly || (item.value.id > 0 && !item.value.isDraft))

// unsaved edits block a submit: the workflow acts on the stored request
const snapshot = ref("")
const serialize = () => JSON.stringify({ t: item.value.title, d: item.value.description, y: item.value.year, i: item.value.items })
watch(() => [item.value.id, item.value.lastModified], () => (snapshot.value = serialize()), { immediate: true })
const isDirty = computed(() => serialize() !== snapshot.value)

function handleChanged(updated: Entity) {
    // copy the workflow state onto the edited instance (the pool was updated by the panel)
    item.value.status = updated.status
    item.value.submittedAt = updated.submittedAt
    item.value.decidedAt = updated.decidedAt
    item.value.decidedBy = updated.decidedBy
    item.value.decisionComment = updated.decisionComment
    item.value.lastModified = updated.lastModified
}
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
