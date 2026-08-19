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
                <div class="col-md-4 mb-2">
                    <input v-model="item.licensePlate" :readonly="readonly" class="form-control" required maxlength="20" />
                    <FormLabel :label="$t('licensePlate')" />
                </div>
                <div class="col-md-4 mb-2">
                    <input v-model="item.brand" :readonly="readonly" class="form-control" required maxlength="64" />
                    <FormLabel :label="$t('brand')" />
                </div>
                <div class="col-md-4 mb-2">
                    <input v-model="item.model" :readonly="readonly" class="form-control" required maxlength="64" />
                    <FormLabel :label="$t('model')" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-3 mb-2">
                    <select v-model="item.type" :disabled="readonly" class="form-select">
                        <option v-for="t in typeOptions" :key="t" :value="t">{{ $t(`vehicleType.${t}`) }}</option>
                    </select>
                    <FormLabel :label="$t('type')" />
                </div>
                <div class="col-md-3 mb-2">
                    <select v-model="item.status" :disabled="readonly" class="form-select">
                        <option v-for="s in statusOptions" :key="s" :value="s">{{ $t(`vehicleStatus.${s}`) }}</option>
                    </select>
                    <FormLabel :label="$t('status')" />
                </div>
                <div class="col-md-3 mb-2">
                    <input v-model.number="item.year" type="number" min="1980" :max="new Date().getFullYear() + 1" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('year')" />
                </div>
                <div class="col-md-3 mb-2">
                    <div class="input-group">
                        <input v-model.number="item.mileage" type="number" min="0" :readonly="readonly" class="form-control" />
                        <span class="input-group-text">km</span>
                    </div>
                    <FormLabel :label="$t('mileage')" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-2">
                    <input v-model="item.vin" :readonly="readonly" class="form-control" maxlength="32" />
                    <FormLabel :label="$t('vin')" />
                </div>
                <div class="col-md-6 mb-2">
                    <DateInput v-model="item.lastServiceDate" :readonly="readonly" />
                    <FormLabel :label="$t('lastServiceDate')" />
                </div>
            </div>
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
import Entity, { VehicleType, VehicleStatus } from "../data/Entity"
import useEntityStore from "../data/store"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const typeOptions = Object.values(VehicleType)
const statusOptions = Object.values(VehicleStatus)
</script>
