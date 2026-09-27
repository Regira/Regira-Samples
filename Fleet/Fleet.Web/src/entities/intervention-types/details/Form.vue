<template>
    <form @submit.prevent="handleSubmit" class="entity-form">
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
            <div class="row g-3">
                <div class="col-sm-4 col-lg-3">
                    <FormLabel :label="$t('code')" />
                    <input v-model.trim="item.code" :readonly="readonly" class="form-control text-uppercase" maxlength="16" required />
                </div>
                <div class="col-sm-8 col-lg-5">
                    <FormLabel :label="$t('name')" />
                    <input v-model.trim="item.title" :readonly="readonly" class="form-control" maxlength="64" required />
                </div>
                <div class="col-sm-6 col-lg-4">
                    <FormLabel :label="$t('category')" />
                    <select v-model="item.category" :disabled="readonly" class="form-select">
                        <option v-for="c in InterventionCategories" :key="c" :value="c">{{ c }}</option>
                    </select>
                </div>
                <div class="col-sm-6 col-lg-4">
                    <FormLabel :label="$t('standardCost')" />
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-currency-euro"></i></span>
                        <input v-model.number="item.standardCost" type="number" min="0" step="0.01" :readonly="readonly" class="form-control" />
                    </div>
                </div>
                <div class="col-sm-6 col-lg-4">
                    <FormLabel :label="$t('intervalKm')" />
                    <div class="input-group">
                        <input v-model.number="item.intervalKm" type="number" min="0" step="1000" :readonly="readonly" class="form-control" />
                        <span class="input-group-text">km</span>
                    </div>
                </div>
                <div class="col-sm-6 col-lg-4">
                    <FormLabel :label="$t('intervalMonths')" />
                    <div class="input-group">
                        <input v-model.number="item.intervalMonths" type="number" min="0" max="240" :readonly="readonly" class="form-control" />
                        <span class="input-group-text">months</span>
                    </div>
                </div>
                <div class="col-12">
                    <FormLabel :label="$t('description')" />
                    <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="3" maxlength="512"></textarea>
                </div>
                <div class="col-12">
                    <div class="form-check form-switch">
                        <input id="typeActive" v-model="item.isActive" :disabled="readonly" class="form-check-input" type="checkbox" />
                        <label class="form-check-label" for="typeActive">{{ $t("active") }}</label>
                    </div>
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
import { InterventionCategories } from "@/infrastructure/domain"
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
