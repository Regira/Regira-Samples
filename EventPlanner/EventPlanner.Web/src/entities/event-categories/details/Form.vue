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
                <RouterLink v-if="overviewUrl && !isPopup" :to="overviewUrl" class="btn btn-outline-info">
                    <Icon name="list" /> <span class="d-none d-md-inline ms-1">{{ $t("overview") }}</span>
                </RouterLink>
            </div>
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="row">
                <div class="col-md-6 mb-3">
                    <input v-model="item.title" :readonly="readonly" class="form-control" required maxlength="64" />
                    <FormLabel :label="$t('title')" />
                </div>
                <div class="col-6 col-md-3 mb-3">
                    <input v-model="item.color" type="color" :disabled="readonly" class="form-control form-control-color w-100" />
                    <FormLabel :label="$t('color')" />
                </div>
                <div class="col-6 col-md-3 mb-3">
                    <div class="input-group">
                        <span class="input-group-text"><i :class="item.$iconClass"></i></span>
                        <input v-model.trim="item.icon" :readonly="readonly" class="form-control" placeholder="mic" maxlength="48" />
                    </div>
                    <FormLabel :label="$t('icon')" />
                </div>
            </div>
            <div class="mb-3">
                <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="2" maxlength="1024"></textarea>
                <FormLabel :label="$t('description')" />
            </div>
            <div class="mb-2">
                <span class="ep-category-badge" :style="{ '--ep-cat': item.$color }"><i :class="item.$iconClass" class="me-1"></i>{{ item.title || "..." }}</span>
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
