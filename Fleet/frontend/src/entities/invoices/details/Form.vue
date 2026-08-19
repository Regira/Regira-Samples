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
                <div class="col-md-4 mb-2" v-if="item.id > 0">
                    <input :value="item.code" readonly class="form-control-plaintext fw-semibold" />
                    <FormLabel :label="$t('code')" />
                </div>
                <div class="col-md-4 mb-2">
                    <SupplierInputSelector v-model="item.supplier" v-model:idValue="item.supplierId" :readonly="readonly" />
                    <FormLabel :label="$t('supplier')" />
                </div>
                <div class="col-md-4 mb-2">
                    <select v-model="item.status" :disabled="readonly" class="form-select">
                        <option v-for="s in statusOptions" :key="s" :value="s">{{ $t(`invoiceStatus.${s}`) }}</option>
                    </select>
                    <FormLabel :label="$t('status')" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-4 mb-2">
                    <DateInput v-model="item.issueDate" :readonly="readonly" />
                    <FormLabel :label="$t('issueDate')" />
                </div>
                <div class="col-md-4 mb-2">
                    <DateInput v-model="item.dueDate" :readonly="readonly" />
                    <FormLabel :label="$t('dueDate')" />
                </div>
                <div class="col-md-4 mb-2" v-if="item.id > 0">
                    <input :value="formatCurrency(item.totalAmount, undefined, 'EUR')" readonly class="form-control-plaintext fw-bold fs-5" />
                    <FormLabel :label="$t('totalAmount')" />
                </div>
            </div>
        </FormSection>

        <FormSection :title="$t('interventions')" :readonly="readonly" v-if="item.id > 0">
            <p v-if="!item.interventions?.length" class="text-muted mb-0">{{ $t("noResults") }}</p>
            <div v-else class="entity-list">
                <div class="row fw-bold border-bottom pb-2">
                    <div class="col">{{ $t("vehicle") }}</div>
                    <div class="col d-none d-md-block">{{ $t("status") }}</div>
                    <div class="col d-none d-md-block">{{ $t("scheduledDate") }}</div>
                    <div class="col-2 text-end">{{ $t("cost") }}</div>
                </div>
                <RouterLink
                    v-for="intervention in item.interventions"
                    :key="intervention.id"
                    :to="{ name: 'InterventionDetails', params: { id: intervention.id } }"
                    class="row border-bottom py-2 text-body text-decoration-none"
                >
                    <div class="col text-truncate">{{ intervention.vehicle?.licensePlate }} <span class="text-muted small">{{ intervention.vehicle?.brand }} {{ intervention.vehicle?.model }}</span></div>
                    <div class="col d-none d-md-block"><StatusBadge :status="intervention.status" :variants="interventionStatusVariants" :label="$t(`interventionStatus.${intervention.status}`)" /></div>
                    <div class="col d-none d-md-block text-truncate">{{ formatDate(intervention.scheduledDate) }}</div>
                    <div class="col-2 text-end">{{ formatCurrency(intervention.cost, undefined, "EUR") }}</div>
                </RouterLink>
            </div>
        </FormSection>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, DateInput } from "@regira/modules/vue/ui"
import { formatCurrency, formatDate } from "@regira/modules/vue/formatters"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity, { InvoiceStatus } from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as SupplierInputSelector } from "@/entities/suppliers"
import StatusBadge from "@/components/status/StatusBadge.vue"
import { interventionStatusVariants } from "@/components/status/variants"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const statusOptions = Object.values(InvoiceStatus)
</script>
