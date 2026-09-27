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

        <div class="row g-3">
            <div class="col-lg-6">
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div class="mb-3">
                        <!-- the event is immutable once registered (server-owned) -->
                        <EventItemInputSelector v-model="item.event" v-model:idValue="item.eventId" :readonly="readonly || !!item.id" :can-edit="false" />
                        <FormLabel :label="$t('event')" />
                    </div>
                    <div class="mb-3">
                        <EmployeeInputSelector
                            v-if="isAdmin() && !item.id"
                            v-model="item.user"
                            v-model:idValue="item.userId"
                            :readonly="readonly"
                            :can-edit="false"
                        />
                        <div v-else class="form-control bg-body-tertiary">
                            <i class="bi bi-person me-1"></i>{{ employeeLabel }}
                        </div>
                        <FormLabel :label="$t('employee')" />
                    </div>
                    <div class="mb-3">
                        <select v-if="isAdmin()" v-model="item.status" :disabled="readonly" class="form-select">
                            <option v-for="s in statuses" :key="s" :value="s">{{ $t(s) }}</option>
                        </select>
                        <div v-else class="d-flex align-items-center gap-2 flex-wrap">
                            <span class="badge ep-status fs-6" :class="`ep-status-${item.status}`">{{ $t(item.status) }}</span>
                            <button v-if="!readonly && item.id && item.status !== 'Cancelled'" type="button" class="btn btn-sm btn-outline-danger" @click="setStatus('Cancelled')">
                                <i class="bi bi-x-circle me-1"></i>{{ $t("cancelRegistration") }}
                            </button>
                            <button v-if="!readonly && item.id && item.status === 'Cancelled'" type="button" class="btn btn-sm btn-outline-success" @click="setStatus('Confirmed')">
                                <i class="bi bi-arrow-counterclockwise me-1"></i>{{ $t("register") }}
                            </button>
                        </div>
                        <FormLabel :label="$t('status')" />
                    </div>
                    <div class="mb-2">
                        <textarea v-model="item.notes" :readonly="readonly" class="form-control" rows="3" maxlength="1000"></textarea>
                        <FormLabel :label="$t('notes')" />
                    </div>
                </FormSection>
            </div>
            <div class="col-lg-6">
                <FormSection :title="$t('sessions')">
                    <SessionPicker v-model="item.sessions" :event-id="item.eventId" :readonly="readonly" />
                </FormSection>
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
import { computed } from "vue"
import { getAccountName } from "@regira/modules/vue/auth"
import { InputSelector as EventItemInputSelector } from "@/entities/events"
import { InputSelector as EmployeeInputSelector, useEntityStore as useEmployeeStore } from "@/entities/employees"
import { useAccess } from "@/access"
import { RegistrationStatus } from "../data/Entity"
import SessionPicker from "./SessionPicker.vue"
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
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)

const { isAdmin } = useAccess()
const statuses = Object.values(RegistrationStatus)
const { fromPool: getEmployee } = useEmployeeStore()
// employees always register themselves (the API stamps the user from the token)
const employeeLabel = computed(() => getEmployee(item.value.user)?.$title ?? (item.value.id ? "" : getAccountName() ?? ""))
async function setStatus(status: RegistrationStatus) {
    item.value.status = status
    await handleSubmit()
}
</script>
