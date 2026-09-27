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
        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="mb-3">
                <FormLabel :label="$t('title')" />
                <input v-model="item.title" :readonly="readonly" class="form-control form-control-lg" required maxlength="64" :placeholder="$t('listTitlePlaceholder')" />
            </div>
            <div class="mb-3">
                <FormLabel :label="$t('shopper')" />
                <ShopperInputSelector v-model="item.shopper" v-model:idValue="item.shopperId" :readonly="readonly" :placeholder="$t('shopper')" />
            </div>
            <div class="mb-3">
                <FormLabel :label="$t('description')" />
                <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="2" maxlength="1024"></textarea>
            </div>
            <div class="row g-3 mb-3 align-items-end">
                <div class="col-auto">
                    <FormLabel :label="$t('color')" />
                    <input v-model="item.color" :disabled="readonly" type="color" class="form-control form-control-color" />
                </div>
                <div class="col">
                    <div class="form-check form-switch sm-switch">
                        <input id="listIsPinned" v-model="item.isPinned" :disabled="readonly" class="form-check-input" type="checkbox" role="switch" />
                        <label class="form-check-label" for="listIsPinned"><i class="bi bi-pin-angle me-1"></i>{{ $t("pinned") }}</label>
                    </div>
                </div>
            </div>
            <RouterLink v-if="item.id" :to="{ name: 'ShoppingListBoard', params: { id: item.id } }" class="btn btn-success btn-lg w-100">
                <i class="bi bi-basket2 me-2"></i>{{ $t("openList") }}
                <span v-if="item.articleCount != null" class="badge text-bg-light ms-2">{{ item.activeCount }} / {{ item.articleCount }}</span>
            </RouterLink>
            <!-- single relation (FK) → the related entity's InputSelector, e.g. <BarInputSelector v-model="item.bar" v-model:idValue="item.barId" /> -->
            <!-- many-to-many / owned rows → InputSelectorInline (@regira/modules/vue/entities): chips that mark
                 _deleted (undoable until save) with the related entity's FormModalButton inside, adds via its
                 InputSelector + exclude; filter _deleted rows in EntityService.prepareItem. The multi-Selector
                 hard-removes — don't use it here. See entities.patterns.md → owned-m2m recipe. -->
            <!-- child collections go here, e.g. <ChildOverview v-model="item" /> (see entities.advanced.example.md) -->
            <!-- ⚠️ but NOT a component that brings its own <FormSection>: it would render a titled panel
                 inside this one. The attachments overview is exactly that — it owns the "files" section, so
                 place it after </FormSection> below, or in its own <template #files> in a tabbed form. -->
        </FormSection>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { onMounted } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as ShopperInputSelector } from "@/entities/shoppers"
import useCurrentShopper from "@/infrastructure/current-shopper"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

// a new list belongs to the shopper this device shops as
const current = useCurrentShopper()
onMounted(async () => {
    if (item.value.id || item.value.shopperId) return
    if (!current.isLoaded) await current.load()
    if (current.shopper) {
        item.value.shopperId = current.shopper.id
        item.value.shopper = current.shopper
    }
})
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
