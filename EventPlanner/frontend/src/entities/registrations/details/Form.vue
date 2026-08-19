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
            <div class="row">
                <div class="col-md-6 mb-2">
                    <EmployeeInputSelector v-model="item.employee" v-model:idValue="item.employeeId" :readonly="readonly" :placeholder="$t('employee')" />
                    <FormLabel :label="$t('employee')" />
                </div>
                <div class="col-md-6 mb-2">
                    <EventItemInputSelector v-model="item.event" v-model:idValue="item.eventId" :readonly="readonly" :placeholder="$t('event')" />
                    <FormLabel :label="$t('event')" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4 mb-2">
                    <select v-model="item.status" class="form-select" :disabled="readonly">
                        <option v-for="s in statuses" :key="s" :value="s">{{ $t(`registrationStatus.${s}`) }}</option>
                    </select>
                    <FormLabel :label="$t('status')" />
                </div>
                <div class="col-md-8 mb-2">
                    <input v-model="item.notes" :readonly="readonly" class="form-control" maxlength="1024" />
                    <FormLabel :label="$t('notes')" />
                </div>
            </div>
        </FormSection>

        <FormSection :title="$t('selectedSessions')" :readonly="readonly">
            <p v-if="!item.eventId" class="italic-muted">{{ $t("pickEventFirst") }}</p>
            <RegistrationSessionOverview v-else v-model="item.selectedSessions" :event-id="item.eventId" />
        </FormSection>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity, { RegistrationStatus } from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import { InputSelector as EventItemInputSelector } from "@/entities/events"
import { RegistrationSessionOverview } from "../registration-sessions"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
const statuses = Object.values(RegistrationStatus)
</script>
