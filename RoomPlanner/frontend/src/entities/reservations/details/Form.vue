<template>
    <!-- Built-ins are the slice defaults — hand-rolling feedback/buttons/tabs/debug/owned-row editors is a deviation (see entities.card). -->
    <form @submit.prevent="handleSubmit">
        <!-- Action bar: save/delete buttons, the back-to-overview link (a page form must offer the way back), feedback. -->
        <div class="row form-toolbar align-items-center mb-3">
            <div class="col col-md-auto order-1">
                <FormButtonsRow
                    :item="item"
                    :readonly="readonly"
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

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div class="row">
                        <div class="col-md mb-2">
                            <input v-model="item.title" :readonly="readonly" class="form-control" required />
                            <FormLabel :label="$t('subject')" />
                        </div>
                        <div class="col-md mb-2">
                            <EmployeeInputSelector v-model="item.organizer" v-model:idValue="item.organizerId" :readonly="readonly" />
                            <FormLabel :label="$t('organizer')" />
                        </div>
                    </div>
                    <div class="mb-2">
                        <DescriptionInput v-model="item.description" :readonly="readonly" />
                    </div>
                    <div class="row">
                        <div class="col-md mb-2">
                            <input type="datetime-local" :value="startTimeInput" @change="onStartTimeChange" :readonly="readonly" class="form-control" required />
                            <FormLabel :label="$t('startTime')" />
                        </div>
                        <div class="col-md mb-2">
                            <input type="datetime-local" :value="endTimeInput" @change="onEndTimeChange" :readonly="readonly" class="form-control" required />
                            <FormLabel :label="$t('endTime')" />
                        </div>
                    </div>
                    <div class="mb-2" v-if="item.id > 0">
                        <select v-model="item.status" :disabled="readonly" class="form-select w-auto">
                            <option v-for="s in ReservationStatuses" :key="s" :value="s">{{ s }}</option>
                        </select>
                        <FormLabel :label="$t('status')" />
                        <div v-if="anyRoomRequiresApproval" class="form-text">{{ $t("roomRequiresApprovalHint") }}</div>
                    </div>
                    <div v-else class="form-text mb-2">{{ $t("statusAutoHint") }}</div>
                </FormSection>
            </template>
            <template #rooms>
                <FormSection :title="$t('rooms')" :readonly="readonly">
                    <ReservationRoomOverview v-model="item.rooms" />
                </FormSection>
            </template>
            <template #attendees>
                <FormSection :title="$t('attendees')" :readonly="readonly">
                    <ReservationAttendeeOverview v-model="item.attendees" :readonly="readonly" />
                </FormSection>
            </template>
        </TabContainer>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab, DescriptionInput } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useLang } from "@regira/modules/vue/lang"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { dateTimeInputString } from "@regira/modules/vue/formatters"
import config from "../config/config"
import Entity, { ReservationStatuses } from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import { ReservationRoomOverview } from "../reservation-rooms"
import { ReservationAttendeeOverview } from "../reservation-attendees"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)

const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { icon: "form", title: translate("form"), isDefault: true }),
    Tab.create("rooms", { icon: "MeetingRoom", title: translate("rooms") }),
    Tab.create("attendees", { icon: "Employee", title: translate("attendees") }),
])

const startTimeInput = computed(() => dateTimeInputString(item.value.startTime))
const endTimeInput = computed(() => dateTimeInputString(item.value.endTime))
function onStartTimeChange(e: Event) {
    const value = (e.target as HTMLInputElement).value
    if (value) item.value.startTime = new Date(value)
}
function onEndTimeChange(e: Event) {
    const value = (e.target as HTMLInputElement).value
    if (value) item.value.endTime = new Date(value)
}

const anyRoomRequiresApproval = computed(() => (item.value.rooms ?? []).some((r) => r.room?.requiresApproval))
</script>
