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
                <div class="col-md-6 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="building" /></span>
                        <input v-model="item.title" required maxlength="96" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('companyName')" />
                </div>
                <div class="col-md-6 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="contact" /></span>
                        <input v-model="item.contactName" maxlength="96" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('contactName')" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="email" /></span>
                        <input v-model.trim="item.email" type="email" maxlength="128" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('email')" />
                </div>
                <div class="col-md-4 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="phone" /></span>
                        <input v-model.trim="item.phone" maxlength="32" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('phone')" />
                </div>
                <div class="col-md-4 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="website" /></span>
                        <input v-model.trim="item.website" type="url" maxlength="256" :readonly="readonly" class="form-control" />
                        <a v-if="item.website" :href="item.website" target="_blank" rel="noopener" class="btn btn-outline-secondary"><Icon name="popOut" /></a>
                    </div>
                    <FormLabel :label="$t('website')" />
                </div>
            </div>
            <div class="mb-3">
                <div class="input-group">
                    <span class="input-group-text"><Icon name="address" /></span>
                    <input v-model="item.address" maxlength="256" :readonly="readonly" class="form-control" />
                </div>
                <FormLabel :label="$t('address')" />
            </div>
            <div class="mb-3">
                <textarea v-model="item.notes" rows="3" maxlength="1024" :readonly="readonly" class="form-control"></textarea>
                <FormLabel :label="$t('notes')" />
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
