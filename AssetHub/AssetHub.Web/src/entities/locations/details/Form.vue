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
                <RouterLink v-if="!isPopup && overviewUrl" :to="overviewUrl" class="btn btn-outline-info">
                    <Icon name="list" /> <span class="d-none d-md-inline ms-1">{{ $t("overview") }}</span>
                </RouterLink>
            </div>
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="row">
                <div class="col-md-8 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="building" /></span>
                        <input v-model="item.title" required maxlength="64" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('title')" />
                </div>
                <div class="col-md-4 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="code" /></span>
                        <input v-model.trim="item.code" maxlength="16" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('code')" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="address" /></span>
                        <input v-model="item.address" maxlength="128" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('address')" />
                </div>
                <div class="col-md-3 mb-3">
                    <input v-model="item.city" maxlength="64" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('city')" />
                </div>
                <div class="col-md-3 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="country" /></span>
                        <input v-model="item.country" maxlength="64" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('country')" />
                </div>
            </div>
            <div class="mb-3">
                <textarea v-model="item.description" rows="2" maxlength="512" :readonly="readonly" class="form-control"></textarea>
                <FormLabel :label="$t('description')" />
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
