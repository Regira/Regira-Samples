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
                    @cancel="handleCancel"
                    @remove="handleRemove"
                />
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

        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="mb-3">
                <input v-model="item.title" :readonly="readonly" class="form-control" required />
                <FormLabel :label="$t('name')" />
            </div>

            <div class="mb-3">
                <button type="button" class="btn" :class="item.isActive ? 'btn-outline-secondary' : 'btn-success'" :disabled="readonly" @click="item.isActive = !item.isActive">
                    <i class="bi" :class="item.isActive ? 'bi-circle' : 'bi-check-circle-fill'"></i>
                    {{ item.isActive ? $t("toBuy") : $t("bought") }}
                </button>
            </div>

            <div class="row">
                <div class="col-6 col-sm-4 mb-3">
                    <input v-model.number="item.quantity" type="number" min="0" step="0.1" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('quantity')" />
                </div>
                <div class="col-6 col-sm-4 mb-3">
                    <input v-model="item.unit" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('unit')" />
                </div>
            </div>

            <div class="mb-3">
                <input v-model="item.notes" :readonly="readonly" class="form-control" />
                <FormLabel :label="$t('notes')" />
            </div>

            <div class="mb-3">
                <ShoppingListInputSelector v-model="item.shoppingList" v-model:idValue="item.shoppingListId as number" :canEdit="false" />
                <FormLabel :label="$t('shoppingList')" />
            </div>

            <div class="mb-3">
                <ArticleCategoryOverview v-model="item.categories" />
                <FormLabel :label="$t('categories')" />
            </div>
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
import { InputSelector as ShoppingListInputSelector } from "@/entities/shopping-lists"
import { ArticleCategoryOverview } from "../article-categories"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove } = useForm<Entity>({ entityService, props, emit })
</script>
