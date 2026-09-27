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
        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <div class="row g-3">
                    <div class="col-lg-8">
                        <FormSection :title="$t('product')" :readonly="readonly">
                            <div class="row g-3">
                                <div class="col-md-8">
                                    <input v-model.trim="item.title" :readonly="readonly" class="form-control" required maxlength="128" />
                                    <FormLabel :label="$t('title')" />
                                </div>
                                <div class="col-md-4">
                                    <input v-model.trim="item.sku" :readonly="readonly" class="form-control" required maxlength="32" />
                                    <FormLabel :label="$t('sku')" />
                                </div>
                                <div class="col-md-6">
                                    <CategoryInputSelector v-model="item.category" v-model:idValue="item.categoryId" :readonly="readonly" />
                                    <FormLabel :label="$t('category')" />
                                </div>
                                <div class="col-md-6">
                                    <BrandInputSelector v-model="item.brand" v-model:idValue="item.brandId" :readonly="readonly" />
                                    <FormLabel :label="$t('brand')" />
                                </div>
                                <div class="col-12">
                                    <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="5" maxlength="2048"></textarea>
                                    <FormLabel :label="$t('description')" />
                                </div>
                            </div>
                        </FormSection>
                    </div>
                    <div class="col-lg-4">
                        <FormSection :title="$t('storefront')">
                            <div class="ws-admin-preview mb-3">
                                <ProductVisual :seed="item.id" :icon="item.category?.icon" :color="item.category?.color" :image-url="item.imageUrl" :label="item.brand?.title" />
                            </div>
                            <div class="form-check form-switch mb-2">
                                <input id="p-active" v-model="item.isActive" :disabled="readonly" class="form-check-input" type="checkbox" />
                                <label class="form-check-label" for="p-active">{{ $t("published") }}</label>
                            </div>
                            <div class="form-check form-switch">
                                <input id="p-featured" v-model="item.isFeatured" :disabled="readonly" class="form-check-input" type="checkbox" />
                                <label class="form-check-label" for="p-featured">{{ $t("featured") }}</label>
                            </div>
                            <RouterLink v-if="item.id" :to="{ name: 'product', params: { id: item.id } }" target="_blank" class="btn btn-sm btn-outline-secondary mt-3">
                                <i class="bi bi-box-arrow-up-right me-1"></i>{{ $t("viewInStore") }}
                            </RouterLink>
                        </FormSection>
                    </div>
                </div>
            </template>
            <template #pricing>
                <FormSection :title="$t('pricingAndStock')" :readonly="readonly">
                    <div class="row g-3">
                        <div class="col-sm-6 col-lg-3">
                            <div class="input-group">
                                <span class="input-group-text">&euro;</span>
                                <input type="number" step="0.01" min="0" v-model.number="item.price" :readonly="readonly" class="form-control" required />
                            </div>
                            <FormLabel :label="$t('price')" />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <div class="input-group">
                                <span class="input-group-text">&euro;</span>
                                <input type="number" step="0.01" min="0" v-model.number="item.compareAtPrice" :readonly="readonly" class="form-control" />
                            </div>
                            <FormLabel :label="$t('compareAtPrice')" />
                            <small v-if="item.$isOnSale" class="text-success">-{{ item.$discountPercentage }}% {{ $t("onSale") }}</small>
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <input type="number" min="0" v-model.number="item.stock" :readonly="readonly" class="form-control" required />
                            <FormLabel :label="$t('stock')" />
                        </div>
                        <div class="col-sm-6 col-lg-3">
                            <div class="input-group">
                                <input type="number" step="0.1" min="0" max="5" v-model.number="item.rating" :readonly="readonly" class="form-control" />
                                <input type="number" min="0" v-model.number="item.reviewCount" :readonly="readonly" class="form-control" />
                            </div>
                            <FormLabel :label="$t('ratingAndReviews')" />
                        </div>
                        <div class="col-12">
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-image"></i></span>
                                <input type="url" v-model.trim="item.imageUrl" :readonly="readonly" class="form-control" maxlength="512" placeholder="https://..." />
                            </div>
                            <FormLabel :label="$t('imageUrl')" />
                        </div>
                    </div>
                </FormSection>
            </template>
        </TabContainer>

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
import { computed } from "vue"
import { TabContainer, Tab } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import { InputSelector as BrandInputSelector } from "@/entities/brands"
import ProductVisual from "@/shop/components/ProductVisual.vue"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { icon: "form", title: translate("product"), isDefault: true }),
    Tab.create("pricing", { icon: "bi bi-currency-euro", title: translate("pricingAndStock") }),
])
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
