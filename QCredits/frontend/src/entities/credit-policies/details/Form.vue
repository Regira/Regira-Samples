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
                    <input v-model.number="item.year" type="number" min="2000" max="2100" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('year')" />
                </div>
            </div>
            <div class="row">
                <div class="col-md mb-2">
                    <input v-model.number="item.annualCredits" type="number" step="0.5" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('annualCredits')" />
                </div>
                <div class="col-md mb-2">
                    <input v-model.number="item.reservedCredits" type="number" step="0.5" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('reservedCredits')" />
                </div>
            </div>
            <div class="row">
                <div class="col-md mb-2">
                    <input v-model.number="item.maxCarryOver" type="number" step="0.5" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('maxCarryOver')" />
                </div>
                <div class="col-md mb-2">
                    <input v-model.number="item.minBalance" type="number" step="0.5" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('minBalance')" />
                </div>
            </div>
            <p class="text-muted small mb-0">{{ $t("freelyAvailableCreditsHint") }}: {{ item.annualCredits - item.reservedCredits }}</p>
        </FormSection>

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

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
</script>
