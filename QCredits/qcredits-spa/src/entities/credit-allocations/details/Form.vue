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
        <div class="row g-3">
            <div class="col-lg-7">
                <FormSection :title="$t('allocation')" :readonly="readonly">
                    <div class="row g-3">
                        <div class="col-md-8">
                            <FormLabel :label="$t('employee')" />
                            <EmployeeInputSelector v-model="item.employee" v-model:idValue="item.employeeId" :readonly="readonly || item.id > 0" :canEdit="false" />
                        </div>
                        <div class="col-md-4">
                            <FormLabel :label="$t('year')" />
                            <input v-model.number="item.year" type="number" min="2000" max="2100" :readonly="readonly || item.id > 0" class="form-control" required />
                        </div>
                        <div class="col-6 col-md-4">
                            <FormLabel :label="$t('annualCredits')" />
                            <input v-model.number="item.annualCredits" type="number" step="0.5" min="0" :readonly="readonly" class="form-control" />
                        </div>
                        <div class="col-6 col-md-4">
                            <FormLabel :label="$t('reservedCredits')" />
                            <input v-model.number="item.reservedCredits" type="number" step="0.5" min="0" :readonly="readonly" class="form-control" />
                        </div>
                        <div class="col-6 col-md-4">
                            <FormLabel :label="$t('reservedUsed')" />
                            <input v-model.number="item.reservedUsed" type="number" step="0.5" min="0" :max="item.reservedCredits" :readonly="readonly" class="form-control" />
                        </div>
                        <div class="col-6 col-md-4">
                            <FormLabel :label="$t('carriedOver')" />
                            <input v-model.number="item.carriedOver" type="number" step="0.5" :readonly="readonly" class="form-control" />
                        </div>
                        <div class="col-6 col-md-4">
                            <FormLabel :label="$t('minBalance')" />
                            <input v-model.number="item.minBalance" type="number" step="0.5" max="0" :readonly="readonly" class="form-control" />
                        </div>
                        <div class="col-12">
                            <FormLabel :label="$t('notes')" />
                            <textarea v-model="item.notes" :readonly="readonly" class="form-control" rows="2" maxlength="1000"></textarea>
                        </div>
                    </div>
                </FormSection>
            </div>
            <div class="col-lg-5">
                <FormSection :title="$t('balance')">
                    <div class="row g-2 text-center mb-3">
                        <div class="col-6 col-xl-3"><div class="qc-stat"><div class="qc-stat-value">{{ formatCredits(free) }}</div><div class="qc-stat-label">{{ $t("freeCredits") }}</div></div></div>
                        <div class="col-6 col-xl-3"><div class="qc-stat"><div class="qc-stat-value text-primary">{{ formatCredits(item.usedCredits) }}</div><div class="qc-stat-label">{{ $t("usedCredits") }}</div></div></div>
                        <div class="col-6 col-xl-3"><div class="qc-stat"><div class="qc-stat-value text-warning">{{ formatCredits(item.pendingCredits) }}</div><div class="qc-stat-label">{{ $t("pendingCredits") }}</div></div></div>
                        <div class="col-6 col-xl-3"><div class="qc-stat"><div class="qc-stat-value" :class="remaining < 0 ? 'text-danger' : 'text-success'">{{ formatCredits(remaining) }}</div><div class="qc-stat-label">{{ $t("remainingCredits") }}</div></div></div>
                    </div>
                    <CreditBar :free="free" :used="item.usedCredits" :pending="item.pendingCredits" :min-balance="item.minBalance" show-legend />
                    <hr />
                    <div class="small text-muted mb-1">{{ $t("companyDays") }}</div>
                    <div class="progress qc-reserved" role="progressbar" :aria-valuenow="item.reservedUsed">
                        <div class="progress-bar bg-secondary" :style="{ width: reservedPct }">{{ formatCredits(item.reservedUsed) }} / {{ formatCredits(item.reservedCredits) }}</div>
                    </div>
                    <p class="small text-muted mt-3 mb-0"><i class="bi bi-info-circle me-1"></i>{{ $t("balanceExplanation") }}</p>
                </FormSection>
            </div>
        </div>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { computed } from "vue"
import { InputSelector as EmployeeInputSelector } from "@/entities/employees"
import { CreditBar } from "@/components/credits"
import { formatCredits } from "@/domain/enums"
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
// balance preview follows the edited figures (used/pending come from the API)
const free = computed(() => item.value.annualCredits - item.value.reservedCredits + item.value.carriedOver)
const remaining = computed(() => free.value - item.value.usedCredits)
const reservedPct = computed(() => `${item.value.reservedCredits > 0 ? Math.min(100, (item.value.reservedUsed / item.value.reservedCredits) * 100) : 0}%`)
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
