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
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="mb-2">
                <input v-model="item.title" :readonly="readonly" class="form-control" required maxlength="64" />
                <FormLabel :label="$t('name')" />
            </div>
            <div class="row">
                <div class="col-6 mb-2">
                    <input v-model="item.colorHex" type="color" :readonly="readonly" class="form-control form-control-color w-100" />
                    <FormLabel :label="$t('color')" />
                </div>
                <div class="col-6 mb-2">
                    <input v-model="item.icon" :readonly="readonly" class="form-control" placeholder="cpu, briefcase, heart-pulse…" maxlength="64" />
                    <FormLabel :label="$t('icon')" />
                </div>
            </div>
        </FormSection>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import type { RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel } from "@regira/modules/vue/ui"
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
