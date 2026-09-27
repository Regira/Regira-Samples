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

        <div class="row g-3">
            <div class="col-lg-7">
                <FormSection :title="$t('supplier')" :readonly="readonly" class="mb-3">
                    <div class="row g-3">
                        <div class="col-sm-8">
                            <FormLabel :label="$t('name')" />
                            <input v-model.trim="item.title" :readonly="readonly" class="form-control" maxlength="128" required />
                        </div>
                        <div class="col-sm-4">
                            <FormLabel :label="$t('vatNumber')" />
                            <input v-model.trim="item.vatNumber" :readonly="readonly" class="form-control" maxlength="32" />
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('contactPerson')" />
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-person"></i></span>
                                <input v-model.trim="item.contactPerson" :readonly="readonly" class="form-control" maxlength="128" />
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('phone')" />
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-telephone"></i></span>
                                <input v-model.trim="item.phone" :readonly="readonly" class="form-control" maxlength="32" />
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('email')" />
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                                <input v-model.trim="item.email" type="email" :readonly="readonly" class="form-control" maxlength="128" />
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('rating')" />
                            <select v-model="item.rating" :disabled="readonly" class="form-select">
                                <option :value="undefined">not rated</option>
                                <option v-for="n in 5" :key="n" :value="n">{{ n }} / 5</option>
                            </select>
                        </div>
                        <div class="col-sm-6">
                            <FormLabel :label="$t('street')" />
                            <input v-model.trim="item.street" :readonly="readonly" class="form-control" maxlength="128" />
                        </div>
                        <div class="col-4 col-sm-2">
                            <FormLabel :label="$t('postalCode')" />
                            <input v-model.trim="item.postalCode" :readonly="readonly" class="form-control" maxlength="16" />
                        </div>
                        <div class="col-8 col-sm-4">
                            <FormLabel :label="$t('city')" />
                            <input v-model.trim="item.city" :readonly="readonly" class="form-control" maxlength="64" />
                        </div>
                        <div class="col-12">
                            <FormLabel :label="$t('notes')" />
                            <textarea v-model="item.notes" :readonly="readonly" class="form-control" rows="2" maxlength="1024"></textarea>
                        </div>
                        <div class="col-12">
                            <div class="form-check form-switch">
                                <input id="supplierActive" v-model="item.isActive" :disabled="readonly" class="form-check-input" type="checkbox" />
                                <label class="form-check-label" for="supplierActive">{{ $t("active") }}</label>
                            </div>
                        </div>
                    </div>
                </FormSection>
            </div>
            <div class="col-lg-5">
                <FormSection :title="$t('capabilities')" :readonly="readonly" class="mb-3">
                    <p class="text-secondary small mb-2">{{ $t("capabilitiesHelp") }}</p>
                    <SupplierInterventionTypeOverview v-model="item.interventionTypes" />
                </FormSection>
            </div>
        </div>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { SupplierInterventionTypeOverview } from "../supplier-intervention-types"
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
</script>
