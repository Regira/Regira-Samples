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
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div class="row g-3">
                        <div class="col-md-8">
                            <input v-model.trim="item.title" :readonly="readonly" class="form-control" required maxlength="96" />
                            <FormLabel :label="$t('title')" />
                        </div>
                        <div class="col-md-4">
                            <input v-model.trim="item.badge" :readonly="readonly" class="form-control" maxlength="24" placeholder="-30%" />
                            <FormLabel :label="$t('badge')" />
                        </div>
                        <div class="col-12">
                            <textarea v-model="item.subtitle" :readonly="readonly" class="form-control" rows="2" maxlength="256"></textarea>
                            <FormLabel :label="$t('subtitle')" />
                        </div>
                        <div class="col-md-5">
                            <input v-model.trim="item.ctaLabel" :readonly="readonly" class="form-control" maxlength="48" placeholder="Shop now" />
                            <FormLabel :label="$t('ctaLabel')" />
                        </div>
                        <div class="col-md-7">
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-link-45deg"></i></span>
                                <input v-model.trim="item.ctaLink" :readonly="readonly" class="form-control" maxlength="256" placeholder="/shop?onSale=true" />
                            </div>
                            <FormLabel :label="$t('ctaLink')" />
                        </div>
                    </div>
                </FormSection>
            </div>
            <div class="col-lg-5">
                <FormSection :title="$t('presentation')" :readonly="readonly">
                    <div class="row g-3">
                        <div class="col-sm-6">
                            <select v-model="item.theme" :disabled="readonly" class="form-select">
                                <option v-for="t in PromotionThemes" :key="t" :value="t">{{ t }}</option>
                            </select>
                            <FormLabel :label="$t('theme')" />
                        </div>
                        <div class="col-sm-6">
                            <div class="input-group">
                                <span class="input-group-text"><i :class="`bi bi-${item.icon || 'stars'}`"></i></span>
                                <input v-model.trim="item.icon" :readonly="readonly" class="form-control" placeholder="stars" />
                            </div>
                            <FormLabel :label="$t('iconName')" />
                        </div>
                        <div class="col-sm-6">
                            <DateInput v-model="item.startDate" :readonly="readonly" />
                            <FormLabel :label="$t('startDate')" />
                        </div>
                        <div class="col-sm-6">
                            <DateInput v-model="item.endDate" :readonly="readonly" />
                            <FormLabel :label="$t('endDate')" />
                        </div>
                        <div class="col-sm-6">
                            <input type="number" v-model.number="item.sortOrder" :readonly="readonly" class="form-control" />
                            <FormLabel :label="$t('sortOrder')" />
                        </div>
                        <div class="col-sm-6 d-flex align-items-center">
                            <div class="form-check form-switch">
                                <input id="p-active" v-model="item.isActive" :disabled="readonly" class="form-check-input" type="checkbox" />
                                <label class="form-check-label" for="p-active">{{ $t("isActive") }}</label>
                            </div>
                        </div>
                    </div>
                </FormSection>
            </div>
            <div class="col-12">
                <FormSection :title="$t('preview')">
                    <PromoBanner :promotion="item" variant="compact" />
                </FormSection>
            </div>
        </div>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, DateInput } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { PromotionThemes } from "../data/Entity"
import PromoBanner from "@/shop/components/PromoBanner.vue"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
