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

        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="mb-3">
                <FormLabel :label="$t('title')" />
                <input v-model="item.title" :readonly="readonly" class="form-control form-control-lg" required maxlength="128" :placeholder="$t('articlePlaceholder')" />
            </div>
            <div class="row g-2 mb-3">
                <div class="col-6">
                    <FormLabel :label="$t('quantity')" />
                    <div class="input-group sm-stepper">
                        <button type="button" class="btn btn-outline-secondary" :disabled="readonly" :aria-label="$t('less')" @click="step(-1)"><i class="bi bi-dash-lg"></i></button>
                        <input v-model.number="item.quantity" :readonly="readonly" type="number" inputmode="decimal" min="0" step="any" class="form-control text-center" />
                        <button type="button" class="btn btn-outline-secondary" :disabled="readonly" :aria-label="$t('more')" @click="step(1)"><i class="bi bi-plus-lg"></i></button>
                    </div>
                </div>
                <div class="col-6">
                    <FormLabel :label="$t('unit')" />
                    <input v-model.trim="item.unit" :readonly="readonly" class="form-control" maxlength="16" list="sm-units" />
                    <datalist id="sm-units">
                        <option v-for="u in units" :key="u" :value="u" />
                    </datalist>
                </div>
            </div>
            <div class="mb-3">
                <FormLabel :label="$t('shoppingList')" />
                <ShoppingListInputSelector v-model="item.shoppingList" v-model:idValue="item.shoppingListId" :readonly="readonly" :placeholder="$t('shoppingList')" />
            </div>
            <div class="mb-3">
                <FormLabel :label="$t('note')" />
                <input v-model="item.description" :readonly="readonly" class="form-control" maxlength="1024" :placeholder="$t('notePlaceholder')" />
            </div>
            <div class="form-check form-switch sm-switch mb-2">
                <input id="articleIsActive" v-model="item.isActive" :disabled="readonly" class="form-check-input" type="checkbox" role="switch" />
                <label class="form-check-label" for="articleIsActive">{{ item.isActive ? $t("stillToBuy") : $t("alreadyBought") }}</label>
            </div>
        </FormSection>
        <FormSection :title="$t('categories')" :readonly="readonly" class="mt-3">
            <ArticleCategoryOverview v-model="item.categories" />
        </FormSection>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
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
import { ArticleCategoryOverview } from "../article-categories"
import { InputSelector as ShoppingListInputSelector } from "@/entities/shopping-lists"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const units = ["pcs", "kg", "g", "l", "ml", "pack", "box", "bag", "bottle", "can", "jar", "bunch"]
function step(delta: number) {
    const next = Math.max(0, (Number(item.value.quantity) || 0) + delta)
    item.value.quantity = next || undefined
}
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
