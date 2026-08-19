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
            <div class="row">
                <div class="col-md-6 mb-2">
                    <input v-model="item.title" :readonly="readonly" class="form-control" required maxlength="128" />
                    <FormLabel :label="$t('name')" />
                </div>
                <div class="col-md-6 mb-2 d-flex align-items-end">
                    <NullableCheckBox v-model="item.isActive" id="supplier-is-active" :label="$t('isActive')" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6 mb-2">
                    <input v-model="item.contactEmail" type="email" :readonly="readonly" class="form-control" maxlength="256" />
                    <FormLabel :label="$t('contactEmail')" />
                </div>
                <div class="col-md-6 mb-2">
                    <input v-model="item.contactPhone" :readonly="readonly" class="form-control" maxlength="32" />
                    <FormLabel :label="$t('contactPhone')" />
                </div>
            </div>
            <div class="mb-2">
                <input v-model="item.address" :readonly="readonly" class="form-control" maxlength="256" />
                <FormLabel :label="$t('address')" />
            </div>
        </FormSection>

        <FormSection :title="$t('supportedInterventionTypes')" :readonly="readonly">
            <SupplierInterventionTypeOverview v-model="item.supportedInterventionTypes" />
        </FormSection>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, NullableCheckBox } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { SupplierInterventionTypeOverview } from "../supplier-intervention-types"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
</script>
