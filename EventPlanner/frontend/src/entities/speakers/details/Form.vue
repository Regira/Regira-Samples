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

        <div class="row">
            <div class="col-md-3 mb-3 text-center">
                <div class="speaker-avatar mx-auto" :style="item.photoUrl ? { backgroundImage: `url(${item.photoUrl})` } : undefined" />
            </div>
            <div class="col-md-9">
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div class="row">
                        <div class="col-md-6 mb-2">
                            <input v-model="item.title" :readonly="readonly" class="form-control" required maxlength="128" />
                            <FormLabel :label="$t('name')" />
                        </div>
                        <div class="col-md-6 mb-2">
                            <input v-model="item.email" type="email" :readonly="readonly" class="form-control" maxlength="256" />
                            <FormLabel :label="$t('email')" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6 mb-2">
                            <input v-model="item.jobTitle" :readonly="readonly" class="form-control" maxlength="128" />
                            <FormLabel :label="$t('jobTitle')" />
                        </div>
                        <div class="col-md-6 mb-2">
                            <input v-model="item.company" :readonly="readonly" class="form-control" maxlength="128" />
                            <FormLabel :label="$t('company')" />
                        </div>
                    </div>
                    <div class="mb-2">
                        <input v-model="item.photoUrl" :readonly="readonly" class="form-control" placeholder="https://…" maxlength="1024" />
                        <FormLabel :label="$t('photoUrl')" />
                    </div>
                    <div class="mb-2">
                        <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="4" maxlength="2048" />
                        <FormLabel :label="$t('bio')" />
                    </div>
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

<style scoped>
.speaker-avatar {
    width: 140px;
    height: 140px;
    border-radius: 50%;
    background-size: cover;
    background-position: center top;
    background-color: var(--rg-accent, #6d28d9);
    background-image: linear-gradient(135deg, var(--rg-accent, #6d28d9), #ec4899);
}
</style>
