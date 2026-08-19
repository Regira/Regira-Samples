<template>
    <form @submit.prevent="handleSubmit">
        <div class="row form-toolbar align-items-center mb-3">
            <div class="col col-md-auto order-1">
                <FormButtonsRow
                    :item="item"
                    :readonly="readonly"
                    :feedback="feedback"
                    :show-delete="item?.id > 0"
                    @cancel="handleCancel"
                    @remove="handleRemove"
                    @restore="handleRestore"
                />
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

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div v-if="item.code" class="row">
                        <div class="col-md-auto mb-2">
                            <span class="badge text-bg-light border font-monospace">{{ item.code }}</span>
                            <FormLabel :label="$t('code')" />
                        </div>
                        <div class="col-md mb-2">
                            <span v-if="item.currentEmployeeName" class="badge text-bg-success">
                                {{ $t("asset.currentHolder") }}: {{ item.currentEmployeeName }}
                            </span>
                            <span v-else class="badge text-bg-secondary">{{ $t("asset.unassigned") }}</span>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md mb-2">
                            <input v-model="item.title" maxlength="150" :readonly="readonly" class="form-control" />
                            <FormLabel :label="$t('name')" />
                        </div>
                        <div class="col-md mb-2">
                            <input v-model="item.serialNumber" maxlength="100" :readonly="readonly" class="form-control" />
                            <FormLabel :label="$t('asset.serialNumber')" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md mb-2">
                            <CategoryInputSelector v-model="item.category" v-model:idValue="item.categoryId" :readonly="readonly" />
                            <FormLabel :label="$t('category')" />
                        </div>
                        <div class="col-md mb-2">
                            <AssetStatusInputSelector v-model="item.status" v-model:idValue="item.statusId" :readonly="readonly" />
                            <FormLabel :label="$t('status')" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md mb-2">
                            <LocationItemInputSelector v-model="item.location" v-model:idValue="item.locationId" :readonly="readonly" />
                            <FormLabel :label="$t('location')" />
                        </div>
                        <div class="col-md mb-2">
                            <SupplierInputSelector v-model="item.supplier" v-model:idValue="item.supplierId" :readonly="readonly" />
                            <FormLabel :label="$t('supplier')" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md mb-2">
                            <DateInput v-model="item.purchaseDate" :readonly="readonly" />
                            <FormLabel :label="$t('asset.purchaseDate')" />
                        </div>
                        <div class="col-md mb-2">
                            <input type="number" step="0.01" v-model.number="item.purchasePrice" :readonly="readonly" class="form-control" />
                            <FormLabel :label="$t('asset.purchasePrice')" />
                        </div>
                    </div>

                    <div class="mb-2">
                        <DescriptionInput v-model="item.description" :label="$t('description')" :readonly="readonly" />
                    </div>
                    <div class="mb-2">
                        <DescriptionInput v-model="item.notes" :label="$t('asset.notes')" :readonly="readonly" />
                    </div>
                </FormSection>
            </template>

            <template #attachments>
                <FormSection :title="$t('asset.attachments')">
                    <AssetAttachmentOverview v-model="item.attachments" :readonly="readonly" />
                </FormSection>
            </template>

            <template #warranties>
                <FormSection :title="$t('asset.warranties')">
                    <AssetWarrantyOverview v-model="item.warranties" :readonly="readonly" />
                </FormSection>
            </template>

            <template #maintenance>
                <FormSection :title="$t('asset.maintenanceRecords')">
                    <AssetMaintenanceRecordOverview v-model="item.maintenanceRecords" :readonly="readonly" />
                </FormSection>
            </template>

            <template #assignments>
                <FormSection :title="$t('asset.assignments')">
                    <div class="mb-2 text-end" v-if="item.id > 0">
                        <AssetAssignmentFormModalButton
                            class="btn btn-info"
                            :item-defaults="{ assetId: item.id, asset: item, assignedDate: new Date() }"
                            @save="handleAssignmentSaved"
                        >
                            <Icon name="new" /> {{ $t("asset.assignAsset") }}
                        </AssetAssignmentFormModalButton>
                    </div>
                    <div v-if="item.assignments?.length" class="assignment-history">
                        <div class="row fw-bold small text-muted border-bottom pb-1">
                            <div class="col">{{ $t("employee") }}</div>
                            <div class="col-2">{{ $t("assetAssignment.assignedDate") }}</div>
                            <div class="col-2">{{ $t("assetAssignment.returnedDate") }}</div>
                        </div>
                        <div v-for="assignment in item.assignments" :key="assignment.id" class="row py-1 border-bottom">
                            <div class="col">
                                <AssetAssignmentFormModalButton class="btn btn-link p-0" :model-value="assignment" @save="handleAssignmentSaved">
                                    {{ assignment.employee?.firstName }} {{ assignment.employee?.lastName }}
                                </AssetAssignmentFormModalButton>
                            </div>
                            <div class="col-2">{{ formatDate(assignment.assignedDate) }}</div>
                            <div class="col-2">
                                <span v-if="assignment.returnedDate">{{ formatDate(assignment.returnedDate) }}</span>
                                <span v-else class="badge text-bg-success">{{ $t("assetAssignment.active") }}</span>
                            </div>
                        </div>
                    </div>
                    <p v-else class="italic-muted">{{ $t("noResults") }}</p>
                </FormSection>
            </template>
        </TabContainer>

        <Debug
            :modelValue="{
                item: {
                    ...item,
                    category: item.category?.title,
                    status: item.status?.title,
                    location: item.location?.title,
                    supplier: item.supplier?.title,
                },
            }"
        />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { useLang } from "@regira/modules/vue/lang"
import { Feedback, FormButtonsRow, FormSection, FormLabel, DescriptionInput, DateInput, TabContainer, Tab, Icon } from "@regira/modules/vue/ui"
import { formatDate } from "@regira/modules/vue/formatters"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults, type SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import { InputSelector as AssetStatusInputSelector } from "@/entities/asset-statuses"
import { InputSelector as LocationItemInputSelector } from "@/entities/locations"
import { InputSelector as SupplierInputSelector } from "@/entities/suppliers"
import { AssetAttachmentOverview } from "../asset-attachments"
import { AssetWarrantyOverview } from "../asset-warranties"
import { AssetMaintenanceRecordOverview } from "../asset-maintenance-records"
// Deep import (not the barrel): asset-assignments already imports Asset's barrel as a VALUE
// (FormModalButton/useEntityStore in its ListItem/FilterAdv), so this edge must bypass
// "@/entities/asset-assignments" to avoid a two-directional store.ts cycle (entities.card → Cross-slice model imports).
import AssetAssignmentFormModalButton from "@/entities/asset-assignments/details/FormModalButton.vue"
import type { Entity as AssetAssignment } from "@/entities/asset-assignments"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

// Re-fetch the asset after an assignment is created/edited so the current-holder badge and history refresh
// with the server's authoritative state (AssetProcessor recomputes the current holder).
async function handleAssignmentSaved(_result: SaveResult<AssetAssignment>) {
    if (item.value?.id > 0) {
        const reloaded = await entityService.details(item.value.id)
        if (reloaded) Object.assign(item.value, reloaded)
    }
}

const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { icon: "form", title: translate("form"), isDefault: true }),
    Tab.create("assignments", { icon: "bi bi-arrow-left-right", title: translate("asset.assignments") }),
    Tab.create("attachments", { icon: "bi bi-paperclip", title: translate("asset.attachments") }),
    Tab.create("warranties", { icon: "bi bi-shield-check", title: translate("asset.warranties") }),
    Tab.create("maintenance", { icon: "bi bi-tools", title: translate("asset.maintenanceRecords") }),
])
</script>
