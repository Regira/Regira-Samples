<template>
    <form @submit.prevent="handleSubmit">
        <div class="row form-toolbar align-items-center mb-3">
            <div class="col col-md-auto order-1">
                <FormButtonsRow :item="item" :readonly="readonly" :feedback="feedback" :show-delete="item?.id > 0" @cancel="handleCancel" @remove="handleRemove" @restore="handleRestore" />
            </div>
            <div class="col-auto order-2 order-md-3">
                <RouterLink v-if="isPopup" :to="{ name: `${config.key}Details`, params: { id: item.$id } }" target="_blank" class="btn btn-outline-secondary" :title="$t('popOut')">
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
                <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="2" maxlength="1024" />
                <FormLabel :label="$t('description')" />
            </div>
            <div class="row">
                <div class="col-6 mb-2">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="bi bi-currency-euro" /></span>
                        <input v-model.number="item.estimatedCost" type="number" min="0" step="0.01" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('estimatedCost')" />
                </div>
                <div class="col-6 mb-2">
                    <div class="input-group">
                        <input v-model.number="item.estimatedDurationHours" type="number" min="0" step="0.25" :readonly="readonly" class="form-control" />
                        <span class="input-group-text">{{ $t("hours") }}</span>
                    </div>
                    <FormLabel :label="$t('estimatedDurationHours')" />
                </div>
            </div>
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
</script>
