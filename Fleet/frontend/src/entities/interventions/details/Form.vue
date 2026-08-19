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
                    <VehicleInputSelector v-model="item.vehicle" v-model:idValue="item.vehicleId" :readonly="readonly" />
                    <FormLabel :label="$t('vehicle')" />
                </div>
                <div class="col-md-4 mb-2">
                    <SupplierInputSelector v-model="item.supplier" v-model:idValue="item.supplierId" :readonly="readonly" />
                    <FormLabel :label="$t('supplier')" />
                </div>
                <div class="col-md-4 mb-2">
                    <select v-model="item.status" :disabled="readonly" class="form-select">
                        <option v-for="s in statusOptions" :key="s" :value="s">{{ $t(`interventionStatus.${s}`) }}</option>
                    </select>
                    <FormLabel :label="$t('status')" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-3 mb-2">
                    <DateInput v-model="item.scheduledDate" :readonly="readonly" />
                    <FormLabel :label="$t('scheduledDate')" />
                </div>
                <div class="col-md-3 mb-2">
                    <DateInput v-model="item.completedDate" :readonly="readonly" />
                    <FormLabel :label="$t('completedDate')" />
                </div>
                <div class="col-md-3 mb-2">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="bi bi-currency-euro" /></span>
                        <input v-model.number="item.cost" type="number" min="0" step="0.01" :readonly="readonly" class="form-control" />
                    </div>
                    <FormLabel :label="$t('cost')" />
                </div>
                <div class="col-md-3 mb-2">
                    <InvoiceInputSelector v-model="item.invoice" v-model:idValue="item.invoiceId" :readonly="readonly" :filter-defaults="item.supplierId ? { supplierId: item.supplierId } : undefined" />
                    <FormLabel :label="$t('invoice')" />
                </div>
            </div>

            <div class="mb-2">
                <textarea v-model="item.notes" :readonly="readonly" class="form-control" rows="3" maxlength="2048" />
                <FormLabel :label="$t('notes')" />
            </div>
        </FormSection>

        <FormSection :title="$t('interventionTypes')" :readonly="readonly">
            <InterventionInterventionTypeOverview v-model="item.interventionTypes" />
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
import Entity, { InterventionStatus } from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as VehicleInputSelector } from "@/entities/vehicles"
import { InputSelector as SupplierInputSelector } from "@/entities/suppliers"
import { InputSelector as InvoiceInputSelector } from "@/entities/invoices"
import { InterventionInterventionTypeOverview } from "../intervention-intervention-types"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const statusOptions = Object.values(InterventionStatus)
</script>
