<template>
    <form @submit.prevent="handleSubmit">
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
            <div class="col-auto order-2 order-md-3 d-flex gap-2">
                <RouterLink
                    v-if="isPopup"
                    :to="{ name: `${config.key}Details`, params: { id: item.$id } }"
                    target="_blank"
                    class="btn btn-outline-secondary"
                    :title="$t('popOut')"
                >
                    <Icon name="popOut" />
                </RouterLink>
                <template v-else>
                    <RouterLink v-if="item.id" :to="{ name: `${config.key}Fiche`, params: { id: item.$id } }" class="btn btn-outline-primary">
                        <i class="bi bi-eye"></i> <span class="d-none d-md-inline ms-1">{{ $t("viewEvent") }}</span>
                    </RouterLink>
                    <RouterLink v-if="overviewUrl" :to="overviewUrl" class="btn btn-outline-info">
                        <Icon name="list" /> <span class="d-none d-md-inline ms-1">{{ $t("overview") }}</span>
                    </RouterLink>
                </template>
            </div>
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <div class="ep-form-banner mb-3" :style="item.$bannerStyle">
            <div>
                <span class="badge ep-status" :class="`ep-status-${item.status}`">{{ $t(item.status) }}</span>
                <h3 class="mb-0 mt-1">{{ item.title || $t("event") }}</h3>
                <small>{{ item.$dateRange }}</small>
            </div>
        </div>

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div class="row">
                        <div class="col-lg-8 mb-3">
                            <input v-model="item.title" :readonly="readonly" class="form-control form-control-lg" required maxlength="160" />
                            <FormLabel :label="$t('title')" />
                        </div>
                        <div class="col-6 col-lg-2 mb-3">
                            <select v-model="item.status" :disabled="readonly" class="form-select">
                                <option v-for="s in statuses" :key="s" :value="s">{{ $t(s) }}</option>
                            </select>
                            <FormLabel :label="$t('status')" />
                        </div>
                        <div class="col-6 col-lg-2 mb-3 d-flex flex-column justify-content-center">
                            <div class="form-check form-switch">
                                <input id="isFeatured" v-model="item.isFeatured" type="checkbox" :disabled="readonly" class="form-check-input" />
                                <label for="isFeatured" class="form-check-label">{{ $t("featured") }}</label>
                            </div>
                        </div>
                    </div>
                    <div class="mb-3">
                        <input v-model="item.summary" :readonly="readonly" class="form-control" maxlength="320" />
                        <FormLabel :label="$t('summary')" />
                    </div>
                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <EventCategoryInputSelector v-model="item.category" v-model:idValue="item.categoryId" :readonly="readonly" />
                            <FormLabel :label="$t('category')" />
                        </div>
                        <div class="col-md-6 mb-3">
                            <LocationInputSelector v-model="item.location" v-model:idValue="item.locationId" :readonly="readonly" />
                            <FormLabel :label="$t('location')" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-6 col-md-4 mb-3">
                            <input v-model="item.startDate" type="date" :readonly="readonly" class="form-control" required />
                            <FormLabel :label="$t('startDate')" />
                        </div>
                        <div class="col-6 col-md-4 mb-3">
                            <input v-model="item.endDate" type="date" :min="item.startDate" :readonly="readonly" class="form-control" required />
                            <FormLabel :label="$t('endDate')" />
                        </div>
                        <div class="col-md-4 mb-3">
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-people"></i></span>
                                <input v-model.number="item.maxParticipants" type="number" min="1" :readonly="readonly" class="form-control" />
                            </div>
                            <FormLabel :label="$t('maxParticipants')" />
                        </div>
                    </div>
                    <div class="mb-3">
                        <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="6" maxlength="8000"></textarea>
                        <FormLabel :label="$t('description')" />
                    </div>
                    <div class="mb-2">
                        <input v-model.trim="item.bannerUrl" type="url" :readonly="readonly" class="form-control" maxlength="512" />
                        <FormLabel :label="$t('bannerUrl')" />
                    </div>
                </FormSection>
            </template>
            <template #sessions>
                <FormSection :title="$t('agenda')" :readonly="readonly">
                    <SessionOverview
                        v-model="sessions"
                        :readonly="readonly"
                        :default-date="item.startDate"
                        :default-capacity="item.maxParticipants"
                    />
                </FormSection>
            </template>
            <template #participants>
                <EventParticipants v-if="item.id" :event-id="item.id" />
            </template>
        </TabContainer>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useLang } from "@regira/modules/vue/lang"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { InputSelector as EventCategoryInputSelector } from "@/entities/event-categories"
import { InputSelector as LocationInputSelector } from "@/entities/locations"
import config from "../config/config"
import Entity, { EventStatus } from "../data/Entity"
import useEntityStore from "../data/store"
import { SessionOverview, type Entity as Session } from "../sessions"
import EventParticipants from "./EventParticipants.vue"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const statuses = Object.values(EventStatus)
// a new event starts with an empty agenda; SessionOverview binds the array itself
const sessions = computed<Array<Session>>({
    get: () => item.value.sessions ?? [],
    set: (value) => (item.value.sessions = value),
})

const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { icon: "form", title: translate("form"), isDefault: true }),
    Tab.create("sessions", { icon: "bi bi-calendar-week", title: `${translate("agenda")} (${sessions.value.filter((s) => !s._deleted).length})` }),
    Tab.create("participants", { icon: "people", title: translate("participants"), isDisabled: !item.value.id }),
])
</script>
