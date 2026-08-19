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

        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="mb-2">
                <input v-model="item.title" :readonly="readonly" class="form-control" required maxlength="128" />
                <FormLabel :label="$t('name')" />
            </div>
            <div class="mb-2">
                <EventItemInputSelector v-model="item.event" v-model:idValue="item.eventId" :readonly="readonly" :placeholder="$t('event')" />
                <FormLabel :label="$t('event')" />
            </div>
            <div class="row">
                <div class="col-md-4 mb-2">
                    <input v-model="item.room" :readonly="readonly" class="form-control" maxlength="64" />
                    <FormLabel :label="$t('room')" />
                </div>
                <div class="col-md-4 mb-2">
                    <DateInput v-model="item.startTime" show-time :readonly="readonly" />
                    <FormLabel :label="$t('startTime')" />
                </div>
                <div class="col-md-4 mb-2">
                    <DateInput v-model="item.endTime" show-time :readonly="readonly" />
                    <FormLabel :label="$t('endTime')" />
                </div>
            </div>
            <div class="col-md-4 mb-2">
                <input v-model.number="item.capacity" type="number" min="0" :readonly="readonly" class="form-control" />
                <FormLabel :label="$t('capacity')" />
            </div>
            <div class="mb-2">
                <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="3" maxlength="2048" />
                <FormLabel :label="$t('description')" />
            </div>
        </FormSection>

        <FormSection :title="$t('speakers')" :readonly="readonly">
            <SessionSpeakerOverview v-model="item.sessionSpeakers" />
        </FormSection>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, DateInput } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as EventItemInputSelector } from "@/entities/events"
import { SessionSpeakerOverview } from "../session-speakers"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
</script>
