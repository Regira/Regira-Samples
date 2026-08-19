<template>
    <form @submit.prevent="handleSubmit">
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

        <div v-if="item.bannerImageUrl" class="event-banner-preview mb-3" :style="{ backgroundImage: `url(${item.bannerImageUrl})` }">
            <div class="event-banner-preview-overlay">
                <h2 class="h4 text-white mb-0">{{ item.title || $t("event") }}</h2>
            </div>
        </div>

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div class="row">
                        <div class="col-md-8 mb-2">
                            <input v-model="item.title" :readonly="readonly" class="form-control" required maxlength="160" />
                            <FormLabel :label="$t('name')" />
                        </div>
                        <div class="col-md-4 mb-2 pt-2">
                            <div class="form-check form-switch pt-1">
                                <input v-model="item.isFeatured" type="checkbox" class="form-check-input" id="isFeatured" :disabled="readonly" />
                                <label class="form-check-label" for="isFeatured">{{ $t("featured") }}</label>
                            </div>
                        </div>
                    </div>
                    <div class="mb-2">
                        <input v-model="item.bannerImageUrl" :readonly="readonly" class="form-control" placeholder="https://…" maxlength="1024" />
                        <FormLabel :label="$t('bannerImageUrl')" />
                    </div>
                    <div class="row">
                        <div class="col-md-6 mb-2">
                            <VenueInputSelector v-model="item.location" v-model:idValue="item.locationId" :readonly="readonly" :placeholder="$t('location')" />
                            <FormLabel :label="$t('location')" />
                        </div>
                        <div class="col-md-6 mb-2">
                            <EventCategoryInputSelector
                                v-model="item.eventCategory"
                                v-model:idValue="item.eventCategoryId"
                                :readonly="readonly"
                                :placeholder="$t('eventCategory')"
                            />
                            <FormLabel :label="$t('eventCategory')" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6 mb-2">
                            <DateInput v-model="item.startDate" show-time :readonly="readonly" />
                            <FormLabel :label="$t('startDate')" />
                        </div>
                        <div class="col-md-6 mb-2">
                            <DateInput v-model="item.endDate" show-time :readonly="readonly" />
                            <FormLabel :label="$t('endDate')" />
                        </div>
                    </div>
                    <div class="mb-2">
                        <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="4" maxlength="4096" />
                        <FormLabel :label="$t('description')" />
                    </div>
                </FormSection>
            </template>
            <template #sessions>
                <FormSection :title="$t('sessions')" :readonly="readonly">
                    <div class="d-flex justify-content-between align-items-center mb-2">
                        <span class="text-muted small">{{ item.sessions?.length ?? 0 }} {{ $t("sessions") }}</span>
                        <div class="d-flex gap-2">
                            <RouterLink :to="{ name: 'SessionOverview', query: { eventId: item.id } }" class="btn btn-sm btn-outline-secondary">
                                <Icon name="list" /> {{ $t("manageSessions") }}
                            </RouterLink>
                            <RouterLink :to="{ name: 'SessionDetails', params: { id: 'new' } }" class="btn btn-sm btn-info">
                                <Icon name="new" /> {{ $t("newSession") }}
                            </RouterLink>
                        </div>
                    </div>
                    <p v-if="!item.sessions?.length" class="italic-muted">{{ $t("noSessionsYet") }}</p>
                    <div v-for="session in sortedSessions" :key="session.id" class="row border-bottom py-2 align-items-center">
                        <div class="col-auto agenda-time text-center">
                            <div class="fw-bold small">{{ formatDateTime(session.startTime, "dd/MM HH:mm") }}</div>
                        </div>
                        <div class="col">
                            {{ session.title }}
                            <span v-if="session.room" class="badge text-bg-light border ms-1">{{ session.room }}</span>
                        </div>
                        <div class="col-auto small text-muted">
                            <!-- seatsTaken is filled by SessionProcessor, which runs only in Session's own read
                                 pipeline — nested here (via Event.Sessions) it is always null/undefined, not 0.
                                 Render it as unknown rather than implying zero registrations. -->
                            {{ session.seatsTaken ?? "—" }}/{{ session.capacity }} {{ $t("seats") }}
                        </div>
                    </div>
                </FormSection>
            </template>
        </TabContainer>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, DateInput, TabContainer, Tab } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { formatDateTime } from "@regira/modules/vue/formatters"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as VenueInputSelector } from "@/entities/locations"
import { InputSelector as EventCategoryInputSelector } from "@/entities/event-categories"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { icon: "form", title: translate("form"), isDefault: true }),
    Tab.create("sessions", { icon: "bi bi-calendar-event-fill", title: translate("sessions"), isDisabled: !item.value.id }),
])

const sortedSessions = computed(() => [...(item.value.sessions || [])].sort((a, b) => (a.startTime?.getTime() ?? 0) - (b.startTime?.getTime() ?? 0)))
</script>

<style scoped>
.event-banner-preview {
    height: 220px;
    border-radius: 0.75rem;
    background-size: cover;
    background-position: center;
    position: relative;
    overflow: hidden;
}
.event-banner-preview-overlay {
    position: absolute;
    inset: 0;
    display: flex;
    align-items: flex-end;
    padding: 1rem;
    background: linear-gradient(to top, rgba(0, 0, 0, 0.65), rgba(0, 0, 0, 0));
}
.agenda-time {
    min-width: 90px;
}
</style>
