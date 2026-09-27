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

        <div v-if="item.id" class="fleet-entity-header mb-3">
            <span class="fleet-plate fleet-plate--lg">{{ item.licensePlate }}</span>
            <div>
                <div class="fs-5 fw-semibold">{{ item.make }} {{ item.model }}</div>
                <div class="text-secondary small">{{ item.year }} &middot; {{ item.vehicleType }} &middot; {{ fmtKm(item.mileage) }}</div>
            </div>
            <StatusBadge :value="item.status" :map="vehicleStatusBadge" class="ms-auto" />
        </div>

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t('identification')" :readonly="readonly" class="mb-3">
                    <div class="row g-3">
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('licensePlate')" />
                            <input v-model.trim="item.licensePlate" :readonly="readonly" class="form-control text-uppercase" maxlength="16" required />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('vin')" />
                            <input v-model.trim="item.vin" :readonly="readonly" class="form-control text-uppercase" maxlength="17" />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('make')" />
                            <input v-model.trim="item.make" :readonly="readonly" class="form-control" maxlength="64" required />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('model')" />
                            <input v-model.trim="item.model" :readonly="readonly" class="form-control" maxlength="64" required />
                        </div>
                        <div class="col-sm-4 col-lg-3">
                            <FormLabel :label="$t('vehicleType')" />
                            <select v-model="item.vehicleType" :disabled="readonly" class="form-select">
                                <option v-for="t in VehicleTypes" :key="t" :value="t">{{ t }}</option>
                            </select>
                        </div>
                        <div class="col-sm-4 col-lg-3">
                            <FormLabel :label="$t('fuelType')" />
                            <select v-model="item.fuelType" :disabled="readonly" class="form-select">
                                <option v-for="t in FuelTypes" :key="t" :value="t">{{ fuelLabel[t] ?? t }}</option>
                            </select>
                        </div>
                        <div class="col-sm-4 col-lg-3">
                            <FormLabel :label="$t('year')" />
                            <input v-model.number="item.year" type="number" min="1950" max="2100" :readonly="readonly" class="form-control" />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('acquisitionDate')" />
                            <input v-model="item.acquisitionDate" type="date" :readonly="readonly" class="form-control" />
                        </div>
                    </div>
                </FormSection>
                <FormSection :title="$t('operation')" :readonly="readonly" class="mb-3">
                    <div class="row g-3">
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('status')" />
                            <select v-model="item.status" :disabled="readonly" class="form-select">
                                <option v-for="s in VehicleStatuses" :key="s" :value="s">{{ vehicleStatusBadge[s]!.label }}</option>
                            </select>
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('mileage')" />
                            <div class="input-group">
                                <input v-model.number="item.mileage" type="number" min="0" :readonly="readonly" class="form-control" />
                                <span class="input-group-text">km</span>
                            </div>
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('nextService')" />
                            <input v-model="item.nextServiceDate" type="date" :readonly="readonly" class="form-control" />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('department')" />
                            <input v-model.trim="item.department" :readonly="readonly" class="form-control" maxlength="64" />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <FormLabel :label="$t('assignedDriver')" />
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-person"></i></span>
                                <input v-model.trim="item.assignedDriver" :readonly="readonly" class="form-control" maxlength="64" />
                            </div>
                        </div>
                        <div class="col-12">
                            <FormLabel :label="$t('notes')" />
                            <textarea v-model="item.notes" :readonly="readonly" class="form-control" rows="2" maxlength="1024"></textarea>
                        </div>
                    </div>
                </FormSection>
            </template>
            <template #history>
                <VehicleInterventions v-if="item.id" :vehicle-id="item.id" />
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
import { FuelTypes, VehicleStatuses, VehicleTypes, fmtKm, fuelLabel, vehicleStatusBadge } from "@/infrastructure/domain"
import StatusBadge from "@/components/StatusBadge.vue"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import VehicleInterventions from "./VehicleInterventions.vue"

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
    Tab.create("form", { title: translate("vehicle"), icon: "bi bi-card-text", isDefault: true }),
    Tab.create("history", { title: translate("maintenanceHistory"), icon: "bi bi-clock-history", isDisabled: !item.value.id }),
])
</script>
