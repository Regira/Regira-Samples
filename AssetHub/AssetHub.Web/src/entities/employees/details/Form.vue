<template>
    <form @submit.prevent="handleSubmit">
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

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t('employee')" :readonly="readonly">
                    <div class="row">
                        <div class="col-md-3 mb-3">
                            <div class="input-group">
                                <span class="input-group-text"><Icon name="vCard" /></span>
                                <input v-model.trim="item.code" required maxlength="16" :readonly="readonly" class="form-control" placeholder="EMP-0001" />
                            </div>
                            <FormLabel :label="$t('employeeNumber')" />
                        </div>
                        <div class="col-md-4 mb-3">
                            <input v-model="item.firstName" required maxlength="64" :readonly="readonly" class="form-control" />
                            <FormLabel :label="$t('firstName')" />
                        </div>
                        <div class="col-md-5 mb-3">
                            <input v-model="item.lastName" required maxlength="64" :readonly="readonly" class="form-control" />
                            <FormLabel :label="$t('lastName')" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6 mb-3">
                            <div class="input-group">
                                <span class="input-group-text"><Icon name="email" /></span>
                                <input v-model.trim="item.email" type="email" required maxlength="128" :readonly="readonly" class="form-control" />
                            </div>
                            <FormLabel :label="$t('email')" />
                        </div>
                        <div class="col-md-6 mb-3">
                            <div class="input-group">
                                <span class="input-group-text"><Icon name="mobilePhone" /></span>
                                <input v-model.trim="item.phone" maxlength="32" :readonly="readonly" class="form-control" />
                            </div>
                            <FormLabel :label="$t('phone')" />
                        </div>
                    </div>
                </FormSection>
                <FormSection :title="$t('organisation')" :readonly="readonly">
                    <div class="row">
                        <div class="col-md-4 mb-3">
                            <input v-model.trim="item.department" list="ah-form-departments" maxlength="64" :readonly="readonly" class="form-control" />
                            <datalist id="ah-form-departments">
                                <option v-for="d in departments" :key="d" :value="d" />
                            </datalist>
                            <FormLabel :label="$t('department')" />
                        </div>
                        <div class="col-md-4 mb-3">
                            <input v-model="item.jobTitle" maxlength="64" :readonly="readonly" class="form-control" />
                            <FormLabel :label="$t('jobTitle')" />
                        </div>
                        <div class="col-md-4 mb-3">
                            <input v-model="item.hireDate" type="date" :readonly="readonly" class="form-control" />
                            <FormLabel :label="$t('hireDate')" />
                        </div>
                    </div>
                    <div class="row align-items-center">
                        <div class="col-md-8 mb-3">
                            <LocationSelector v-model="item.location" v-model:idValue="item.locationId as number" :readonly="readonly" :placeholder="$t('location')" />
                            <FormLabel :label="$t('location')" />
                        </div>
                        <div class="col-md-4 mb-3">
                            <div class="form-check form-switch">
                                <input id="empActive" v-model="item.isActive" type="checkbox" class="form-check-input" :disabled="readonly" />
                                <label for="empActive" class="form-check-label">{{ $t("active") }}</label>
                            </div>
                        </div>
                    </div>
                </FormSection>
            </template>

            <template #assets>
                <FormSection :title="$t('currentAssets')">
                    <p v-if="!current.length" class="italic-muted mb-2">{{ $t("noCurrentAssets") }}</p>
                    <div class="row g-2 mb-2">
                        <div v-for="a in current" :key="a.id" class="col-sm-6 col-xl-4">
                            <RouterLink :to="{ name: 'AssetDetails', params: { id: a.assetId } }" class="ah-mini-card text-decoration-none">
                                <i :class="`bi bi-${a.asset?.category?.icon || 'box'}`" class="ah-mini-card__icon"></i>
                                <div class="flex-grow-1 min-w-0">
                                    <div class="text-truncate fw-semibold">{{ a.asset?.title }}</div>
                                    <div class="small text-muted"><span class="ah-code">{{ a.asset?.code }}</span> &middot; {{ $t("since") }} {{ fmtDate(a.assignedOn) }}</div>
                                </div>
                                <StatusBadge :status="a.asset?.status" small />
                            </RouterLink>
                        </div>
                    </div>
                </FormSection>
                <FormSection :title="$t('assignmentHistory')">
                    <div class="entity-list ah-table">
                        <div class="row fw-bold border-bottom pb-1">
                            <div class="col-3 col-md-2">{{ $t("assetTag") }}</div>
                            <div class="col">{{ $t("asset") }}</div>
                            <div class="col-3 col-md-2">{{ $t("assignedOn") }}</div>
                            <div class="col-2 d-none d-md-block">{{ $t("returnedOn") }}</div>
                            <div class="col d-none d-lg-block">{{ $t("notes") }}</div>
                        </div>
                        <div v-for="a in history" :key="a.id" class="row border-bottom py-1">
                            <div class="col-3 col-md-2 text-truncate">
                                <RouterLink :to="{ name: 'AssetDetails', params: { id: a.assetId } }" class="ah-code">{{ a.asset?.code }}</RouterLink>
                            </div>
                            <div class="col text-truncate">{{ a.asset?.title }}</div>
                            <div class="col-3 col-md-2 text-truncate">{{ fmtDate(a.assignedOn) }}</div>
                            <div class="col-2 d-none d-md-block text-truncate">
                                <span v-if="a.returnedOn">{{ fmtDate(a.returnedOn) }}</span>
                                <span v-else class="badge text-bg-primary">{{ $t("current") }}</span>
                            </div>
                            <div class="col d-none d-lg-block text-truncate text-muted">{{ [a.notes, a.returnNotes].filter(Boolean).join(" / ") }}</div>
                        </div>
                        <p v-if="!history.length" class="italic-muted my-2">{{ $t("noResults") }}</p>
                    </div>
                </FormSection>
            </template>
        </TabContainer>
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useLang } from "@regira/modules/vue/lang"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { InputSelector as LocationSelector } from "@/entities/locations"
import StatusBadge from "@/components/StatusBadge.vue"
import { fmtDate } from "@/utilities/format"
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

const departments = ["Engineering", "Sales", "Marketing", "Finance", "HR", "IT", "Operations", "Support", "Legal", "Facilities"]

const history = computed(() => [...(item.value?.assignments ?? [])].sort((a, b) => (b.assignedOn || "").localeCompare(a.assignedOn || "")))
const current = computed(() => history.value.filter((a) => !a.returnedOn))

const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { icon: "form", title: translate("employee"), isDefault: true }),
    Tab.create("assets", { icon: "boxes", title: `${translate("assets")} (${current.value.length})`, isDisabled: !item.value?.id }),
])
</script>
