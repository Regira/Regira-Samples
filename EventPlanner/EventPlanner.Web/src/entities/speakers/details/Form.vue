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

        <div class="ep-profile-head mb-3">
            <SpeakerAvatar :speaker="item" size="lg" />
            <div class="min-w-0">
                <h2 class="mb-0 text-truncate">{{ item.$title || $t("speaker") }}</h2>
                <div class="text-muted">{{ item.jobTitle }}<span v-if="item.company"> &middot; <strong class="ep-accent-text">{{ item.company }}</strong></span></div>
                <div class="mt-1 d-flex flex-wrap gap-1">
                    <span v-for="topic in item.$topics" :key="topic" class="badge rounded-pill ep-topic">{{ topic }}</span>
                </div>
            </div>
        </div>

        <div class="row g-3">
            <div class="col-lg-7">
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <input v-model="item.firstName" :readonly="readonly" class="form-control" required maxlength="64" />
                            <FormLabel :label="$t('firstName')" />
                        </div>
                        <div class="col-md-6 mb-3">
                            <input v-model="item.lastName" :readonly="readonly" class="form-control" required maxlength="64" />
                            <FormLabel :label="$t('lastName')" />
                        </div>
                        <div class="col-md-6 mb-3">
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-building"></i></span>
                                <input v-model="item.company" :readonly="readonly" class="form-control" maxlength="128" />
                            </div>
                            <FormLabel :label="$t('company')" />
                        </div>
                        <div class="col-md-6 mb-3">
                            <input v-model="item.jobTitle" :readonly="readonly" class="form-control" maxlength="128" />
                            <FormLabel :label="$t('jobTitle')" />
                        </div>
                        <div class="col-md-6 mb-3">
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                                <input v-model.trim="item.email" type="email" :readonly="readonly" class="form-control" maxlength="256" />
                            </div>
                            <FormLabel :label="$t('email')" />
                        </div>
                        <div class="col-md-6 mb-3">
                            <input v-model.trim="item.photoUrl" type="url" :readonly="readonly" class="form-control" maxlength="512" />
                            <FormLabel :label="$t('photoUrl')" />
                        </div>
                    </div>
                    <div class="mb-3">
                        <input v-model="item.topics" :readonly="readonly" class="form-control" maxlength="256" placeholder="AI, Cloud, Leadership" />
                        <FormLabel :label="$t('topics')" />
                    </div>
                    <div class="mb-2">
                        <textarea v-model="item.bio" :readonly="readonly" class="form-control" rows="5" maxlength="4000"></textarea>
                        <FormLabel :label="$t('bio')" />
                    </div>
                </FormSection>
            </div>
            <div class="col-lg-5" v-if="item.id">
                <FormSection :title="$t('speaksAt')">
                    <LoadingContainer :is-loading="eventsLoading">
                        <p v-if="!speaksAt.length" class="italic-muted mb-0">{{ $t("noResults") }}</p>
                        <RouterLink
                            v-for="ev in speaksAt"
                            :key="ev.id"
                            :to="{ name: 'EventItemDetails', params: { id: ev.id } }"
                            class="ep-mini-event text-decoration-none text-reset"
                        >
                            <span class="ep-date-chip" :style="{ '--ep-cat': ev.category?.color || '#6366f1' }">
                                <span class="day">{{ ev.$startDay }}</span><span class="month">{{ ev.$startMonth }}</span>
                            </span>
                            <span class="min-w-0">
                                <span class="d-block fw-semibold text-truncate">{{ ev.title }}</span>
                                <small class="text-muted text-truncate d-block">{{ ev.location?.title }}</small>
                            </span>
                        </RouterLink>
                    </LoadingContainer>
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
import { ref } from "vue"
import { LoadingContainer } from "@regira/modules/vue/ui"
import { onAuthenticated } from "@regira/modules/vue/auth"
import useEventStore from "@/entities/events/data/store"
import type EventItem from "@/entities/events/data/Entity"
import SpeakerAvatar from "./SpeakerAvatar.vue"
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

// "Speaks at": upcoming events with a session this speaker presents (deep import of the events store — the
// events slice imports this one's barrel, so the reverse edge must not go through the barrel)
const eventStore = useEventStore()
const speaksAt = ref<Array<EventItem>>([])
const eventsLoading = ref(false)
async function loadSpeaksAt() {
    if (!item.value?.id) return
    eventsLoading.value = true
    try {
        speaksAt.value = await eventStore.service.list({ speakerId: item.value.id, upcoming: true, pageSize: 8, sortBy: ["StartDate"] })
    } finally {
        eventsLoading.value = false
    }
}
onAuthenticated(loadSpeaksAt)
</script>
