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

        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="row">
                <div class="col-md mb-2">
                    <input v-model="item.title" :readonly="readonly" class="form-control" maxlength="128" />
                    <FormLabel :label="$t('name')" />
                </div>
                <div class="col-md mb-2">
                    <DateInput v-model="item.trainingDate" :readonly="readonly" />
                    <FormLabel :label="$t('trainingDate')" />
                </div>
            </div>
            <div class="mb-2">
                <DescriptionInput v-model="item.description" :readonly="readonly" />
                <FormLabel :label="$t('description')" />
            </div>
            <div class="row">
                <div class="col-md mb-2">
                    <input v-model="item.location" :readonly="readonly" class="form-control" maxlength="128" />
                    <FormLabel :label="$t('location')" />
                </div>
                <div class="col-md mb-2">
                    <input v-model="item.facilitator" :readonly="readonly" class="form-control" maxlength="128" />
                    <FormLabel :label="$t('facilitator')" />
                </div>
                <div class="col-md mb-2">
                    <select v-model="item.department" :disabled="readonly" class="form-select">
                        <option :value="undefined">{{ $t("allDepartments") }}</option>
                        <option v-for="dep in departments" :key="dep" :value="dep">{{ dep }}</option>
                    </select>
                    <FormLabel :label="$t('department')" />
                </div>
            </div>
            <div class="row">
                <div class="col-md mb-2">
                    <input v-model.number="item.cost" type="number" step="0.01" min="0" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('cost')" />
                </div>
                <div class="col-md mb-2">
                    <input v-model.number="item.maxParticipants" type="number" min="1" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('maxParticipants')" />
                </div>
            </div>
        </FormSection>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, DateInput, DescriptionInput } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const departments = ["Engineering", "Sales", "Marketing", "Human Resources", "Finance", "Operations", "Customer Support", "Product", "Legal", "IT"]

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
</script>
