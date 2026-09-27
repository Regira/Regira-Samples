<template>
    <!-- Built-ins are the slice defaults — hand-rolling feedback/buttons/tabs/debug/owned-row editors is a deviation (see entities.card). -->
    <form @submit.prevent="handleSubmit">
        <!-- Action bar: save/delete buttons, the back-to-overview link (a page form must offer the way back), feedback. -->
        <!-- order-*: on md+ the overview / pop-out link moves to the END of the row (order-md-3) and the
             feedback fills the middle — without them both land mid-row, next to the save buttons. -->
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
                <!-- In a modal (isPopup) there is no overview to return to — offer a pop-out to the full page instead. -->
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
            <!-- useForm drives `feedback` (Saving… → Saved / 400 field-map); render it here or the save shows nothing. -->
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <!-- Heavier form? Wrap sections in <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
             with one <template #key> per Tab.create(...) — see entities.advanced.example.md §5. -->
        <div v-if="item.id" class="ws-order-head mb-3">
            <span class="badge fs-6" :class="statusBadge[item.status]">{{ item.status }}</span>
            <span class="text-body-secondary">{{ $t("placed") }} {{ item.created ? formatDateTime(item.created, "dd/MM/yyyy HH:mm") : "" }}</span>
            <span class="ms-auto fw-semibold fs-5">{{ money(item.total) }}</span>
        </div>
        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <div class="row g-3">
                    <div class="col-lg-6">
                        <FormSection :title="$t('customer')" :readonly="readonly">
                            <div class="row g-3">
                                <div class="col-12">
                                    <div class="input-group">
                                        <span class="input-group-text"><i class="bi bi-person"></i></span>
                                        <input v-model.trim="item.customerName" :readonly="readonly" class="form-control" required maxlength="128" />
                                    </div>
                                    <FormLabel :label="$t('customerName')" />
                                </div>
                                <div class="col-md-7">
                                    <div class="input-group">
                                        <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                                        <input type="email" v-model.trim="item.email" :readonly="readonly" class="form-control" required maxlength="256" />
                                    </div>
                                    <FormLabel :label="$t('email')" />
                                </div>
                                <div class="col-md-5">
                                    <div class="input-group">
                                        <span class="input-group-text"><i class="bi bi-telephone"></i></span>
                                        <input v-model.trim="item.phone" :readonly="readonly" class="form-control" maxlength="32" />
                                    </div>
                                    <FormLabel :label="$t('phone')" />
                                </div>
                            </div>
                        </FormSection>
                        <FormSection :title="$t('orderStatus')" :readonly="readonly">
                            <div class="row g-3">
                                <div class="col-md-4">
                                    <select v-model="item.status" :disabled="readonly" class="form-select">
                                        <option v-for="s in OrderStatuses" :key="s" :value="s">{{ s }}</option>
                                    </select>
                                    <FormLabel :label="$t('status')" />
                                </div>
                                <div class="col-md-4">
                                    <select v-model="item.shippingMethod" :disabled="readonly" class="form-select">
                                        <option v-for="s in ShippingMethods" :key="s" :value="s">{{ s }}</option>
                                    </select>
                                    <FormLabel :label="$t('shippingMethod')" />
                                </div>
                                <div class="col-md-4">
                                    <select v-model="item.paymentMethod" :disabled="readonly" class="form-select">
                                        <option v-for="s in PaymentMethods" :key="s" :value="s">{{ s }}</option>
                                    </select>
                                    <FormLabel :label="$t('paymentMethod')" />
                                </div>
                            </div>
                        </FormSection>
                    </div>
                    <div class="col-lg-6">
                        <FormSection :title="$t('shippingAddress')" :readonly="readonly">
                            <div class="row g-3">
                                <div class="col-12">
                                    <input v-model.trim="item.street" :readonly="readonly" class="form-control" required maxlength="256" />
                                    <FormLabel :label="$t('street')" />
                                </div>
                                <div class="col-sm-4">
                                    <input v-model.trim="item.postalCode" :readonly="readonly" class="form-control" required maxlength="16" />
                                    <FormLabel :label="$t('postalCode')" />
                                </div>
                                <div class="col-sm-8">
                                    <input v-model.trim="item.city" :readonly="readonly" class="form-control" required maxlength="128" />
                                    <FormLabel :label="$t('city')" />
                                </div>
                                <div class="col-12">
                                    <select v-model="item.country" :disabled="readonly" class="form-select" required>
                                        <option v-for="c in countries" :key="c" :value="c">{{ c }}</option>
                                    </select>
                                    <FormLabel :label="$t('country')" />
                                </div>
                                <div class="col-12">
                                    <textarea v-model="item.notes" :readonly="readonly" class="form-control" rows="2" maxlength="1024"></textarea>
                                    <FormLabel :label="$t('notes')" />
                                </div>
                            </div>
                        </FormSection>
                    </div>
                </div>
            </template>
            <template #lines>
                <FormSection :title="$t('orderLines')" :readonly="readonly">
                    <OrderLineOverview v-model="item.orderLines" :readonly="readonly" />
                    <div class="ws-order-totals">
                        <div><span>{{ $t("subtotal") }}</span><strong>{{ money(item.subtotal) }}</strong></div>
                        <div><span>{{ $t("shipping") }}</span><strong>{{ money(item.shippingCost) }}</strong></div>
                        <div class="fs-5"><span>{{ $t("total") }}</span><strong>{{ money(item.total) }}</strong></div>
                        <small class="text-body-secondary">{{ $t("totalsComputedOnSave") }}</small>
                    </div>
                </FormSection>
            </template>
        </TabContainer>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
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
import { computed } from "vue"
import { TabContainer, Tab } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { formatDateTime } from "@regira/modules/vue/formatters"
import { OrderStatuses, PaymentMethods, ShippingMethods, statusBadge } from "../data/Entity"
import { OrderLineOverview } from "../order-lines"
import { money, countries } from "@/shop/money"

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
    Tab.create("form", { icon: "form", title: translate("order"), isDefault: true }),
    Tab.create("lines", { icon: "list", title: `${translate("orderLines")} (${item.value.orderLines?.filter((l) => !l._deleted).length ?? 0})` }),
])
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
