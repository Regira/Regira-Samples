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
                    :readonly="readonly"
                    :feedback="feedback"
                    :show-delete="item?.id > 0"
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

        <!-- Heavier form? Wrap sections in <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
             with one <template #key> per Tab.create(...) — see entities.advanced.example.md §5. -->
        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="row g-3">
                <div class="col-md-4">
                    <FormLabel :label="$t('role')" />
                    <select v-model="item.role" :disabled="readonly" class="form-select">
                        <option value="Customer">{{ $t("customer") }}</option>
                        <option value="Employee">{{ $t("employee") }}</option>
                    </select>
                </div>
                <div class="col-md-4">
                    <FormLabel :label="$t('givenName')" />
                    <input v-model.trim="item.givenName" :readonly="readonly" class="form-control" required maxlength="64" />
                </div>
                <div class="col-md-4">
                    <FormLabel :label="$t('familyName')" />
                    <input v-model.trim="item.familyName" :readonly="readonly" class="form-control" required maxlength="64" />
                </div>
                <div class="col-md-6">
                    <FormLabel :label="$t('email')" />
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                        <input v-model.trim="item.email" :readonly="readonly" type="email" class="form-control" maxlength="128" />
                    </div>
                </div>
                <div class="col-md-6">
                    <FormLabel :label="$t('phone')" />
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-telephone"></i></span>
                        <input v-model.trim="item.phone" :readonly="readonly" class="form-control" maxlength="32" />
                    </div>
                </div>
                <template v-if="item.role === 'Employee'">
                    <div class="col-md-6">
                        <FormLabel :label="$t('jobTitle')" />
                        <input v-model.trim="item.jobTitle" :readonly="readonly" class="form-control" maxlength="64" />
                    </div>
                    <div class="col-md-6">
                        <FormLabel :label="$t('supportTeam')" />
                        <SupportTeamInputSelector v-model="item.supportTeam" v-model:idValue="item.supportTeamId" :canEdit="false" />
                    </div>
                </template>
                <div v-else class="col-md-6">
                    <FormLabel :label="$t('company')" />
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-building"></i></span>
                        <input v-model.trim="item.company" :readonly="readonly" class="form-control" maxlength="128" />
                    </div>
                </div>
                <div class="col-12">
                    <div class="form-check">
                        <input id="p-isActive" v-model="item.isActive" :disabled="readonly" type="checkbox" class="form-check-input" />
                        <label for="p-isActive" class="form-check-label">{{ $t("active") }}</label>
                    </div>
                </div>
            </div>
        </FormSection>

        <FormSection v-if="item.id > 0" :title="$t('accountAndTickets')" class="mt-3">
            <div class="d-flex flex-wrap align-items-center gap-3">
                <RouterLink
                    :to="{ name: 'TicketOverview', query: item.role === 'Employee' ? { assignedEmployeeId: String(item.id) } : { customerId: String(item.id) } }"
                    class="btn btn-outline-primary btn-sm"
                >
                    <i class="bi bi-ticket-detailed me-1"></i>{{ $t(item.role === "Employee" ? "assignedTickets" : "reportedTickets") }}
                </RouterLink>
                <span v-if="item.hasAccount" class="text-success small"><i class="bi bi-person-check-fill me-1"></i>{{ $t("hasAccount") }}</span>
                <template v-else-if="isAdmin">
                    <div class="input-group input-group-sm" style="max-width: 360px">
                        <input v-model="newPassword" type="password" autocomplete="new-password" class="form-control" :placeholder="$t('initialPassword')" />
                        <button type="button" class="btn btn-outline-success" :disabled="!newPassword || accountFeedback.isPending" @click="createAccount">
                            <i class="bi bi-key me-1"></i>{{ $t("createLogin") }}
                        </button>
                    </div>
                    <Feedback :feedback="accountFeedback" />
                </template>
                <span v-else class="text-muted small">{{ $t("noAccount") }}</span>
            </div>
        </FormSection>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { ref } from "vue"
import { useAxios } from "@regira/modules/vue/http"
import { useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { InputSelector as SupportTeamInputSelector } from "@/entities/support-teams"
import { useAccess } from "@/infrastructure/access"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)

const { isAdmin } = useAccess()
const { translate } = useLang()
const newPassword = ref("")
const accountFeedback = useFeedback()
// admin action: POST persons/{id}/account links a new Identity login (role follows the person's role)
async function createAccount() {
    accountFeedback.pending(translate("saving"))
    try {
        await useAxios().post(`${config.api}/${item.value.id}/account`, { password: newPassword.value })
        item.value.hasAccount = true
        newPassword.value = ""
        accountFeedback.success(translate("loginCreated"))
    } catch (ex: any) {
        console.error("Creating the login failed", ex)
        accountFeedback.fail(translate("loginFailed"), toFeedbackError(ex) ?? ex?.message)
    }
}
</script>
