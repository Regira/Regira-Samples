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

        <div class="row g-3 mb-3" v-if="item.id">
            <div class="col-6 col-md-3"><div class="fleet-mini-stat"><span>{{ $t("subTotal") }}</span><strong>{{ fmtMoney(item.subTotal) }}</strong></div></div>
            <div class="col-6 col-md-3"><div class="fleet-mini-stat"><span>{{ $t("vat") }} ({{ item.vatRate }}%)</span><strong>{{ fmtMoney(item.vatAmount) }}</strong></div></div>
            <div class="col-6 col-md-3"><div class="fleet-mini-stat fleet-mini-stat--accent"><span>{{ $t("total") }}</span><strong>{{ fmtMoney(item.totalAmount) }}</strong></div></div>
            <div class="col-6 col-md-3">
                <div class="fleet-mini-stat">
                    <span>{{ $t("status") }}</span>
                    <strong><StatusBadge :value="item.status" :map="invoiceStatusBadge" /></strong>
                </div>
            </div>
        </div>

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t('invoice')" :readonly="readonly" class="mb-3">
                    <div class="row g-3">
                        <div class="col-sm-6 col-lg-4">
                            <FormLabel :label="$t('invoiceNumber')" />
                            <input v-model.trim="item.invoiceNumber" :readonly="readonly" class="form-control" maxlength="32" required />
                        </div>
                        <div class="col-sm-6 col-lg-8">
                            <FormLabel :label="$t('supplier')" />
                            <SupplierInputSelector v-model="item.supplier" v-model:idValue="item.supplierId" :readonly="readonly || (item.interventions?.length ?? 0) > 0" />
                            <small v-if="(item.interventions?.length ?? 0) > 0" class="text-secondary">{{ $t("supplierLockedHint") }}</small>
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('invoiceDate')" />
                            <input v-model="item.invoiceDate" type="date" :readonly="readonly" class="form-control" required />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('dueDate')" />
                            <input v-model="item.dueDate" type="date" :readonly="readonly" class="form-control" :placeholder="$t('defaults30Days')" />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('status')" />
                            <select v-model="item.status" :disabled="readonly" class="form-select">
                                <option v-for="s in InvoiceStatuses" :key="s" :value="s">{{ invoiceStatusBadge[s]!.label }}</option>
                            </select>
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('paidDate')" />
                            <input v-model="item.paidDate" type="date" :readonly="readonly || item.status !== 'Paid'" class="form-control" />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('vatRate')" />
                            <div class="input-group">
                                <input v-model.number="item.vatRate" type="number" min="0" max="100" step="0.5" :readonly="readonly" class="form-control" />
                                <span class="input-group-text">%</span>
                            </div>
                        </div>
                        <div class="col-12">
                            <FormLabel :label="$t('notes')" />
                            <textarea v-model="item.notes" :readonly="readonly" class="form-control" rows="2" maxlength="1024"></textarea>
                        </div>
                    </div>
                </FormSection>
                <p v-if="!item.id" class="text-secondary small"><i class="bi bi-info-circle me-1"></i>{{ $t("saveInvoiceFirst") }}</p>
            </template>
            <template #interventions>
                <InvoiceInterventions v-if="item.id" v-model="item" :readonly="readonly" />
            </template>
        </TabContainer>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { InputSelector as SupplierInputSelector } from "@/entities/suppliers"
import { InvoiceStatuses, fmtMoney, invoiceStatusBadge } from "@/infrastructure/domain"
import StatusBadge from "@/components/StatusBadge.vue"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import InvoiceInterventions from "./InvoiceInterventions.vue"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { title: translate("invoice"), icon: "bi bi-receipt", isDefault: true }),
    Tab.create("interventions", {
        title: `${translate("interventions")} (${item.value.interventions?.length ?? 0})`,
        icon: "bi bi-tools",
        isDisabled: !item.value.id,
    }),
])
</script>
