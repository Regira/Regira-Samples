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

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div class="row g-3 mb-3">
                        <div class="col-3 col-sm-2">
                            <FormLabel :label="$t('icon')" />
                            <input v-model="item.icon" :readonly="readonly" class="form-control form-control-lg text-center" maxlength="16" placeholder="🍎" />
                        </div>
                        <div class="col">
                            <FormLabel :label="$t('title')" />
                            <input v-model="item.title" :readonly="readonly" class="form-control form-control-lg" required maxlength="64" />
                        </div>
                    </div>
                    <div class="mb-3">
                        <FormLabel :label="$t('description')" />
                        <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="2" maxlength="1024"></textarea>
                    </div>
                    <div class="mb-3 d-flex align-items-end gap-3">
                        <div>
                            <FormLabel :label="$t('color')" />
                            <input v-model="item.color" :disabled="readonly" type="color" class="form-control form-control-color" />
                        </div>
                        <span class="sm-chip is-selected" :style="{ '--sm-chip-color': item.color }">{{ item.icon }} {{ item.title || $t("preview") }}</span>
                    </div>
                </FormSection>
            </template>
            <template #hierarchy>
                <FormSection :title="$t('parentCategories')" :readonly="readonly">
                    <p class="small text-muted mb-2">{{ $t("parentCategoriesHelp") }}</p>
                    <ParentCategoryEditor v-model="item.parentEntities" :child-id="item.id" :readonly="readonly" />
                </FormSection>
                <FormSection :title="$t('subCategories')" :readonly="true" class="mt-3">
                    <div v-if="children.length" class="d-flex flex-wrap gap-2">
                        <CategoryButton v-for="c in children" :key="c.id" :model-value="c" class="btn sm-chip" :style="{ '--sm-chip-color': c.color }">
                            {{ c.icon }} {{ c.title }}
                        </CategoryButton>
                    </div>
                    <p v-else class="italic-muted mb-0">{{ $t("noSubCategories") }}</p>
                </FormSection>
            </template>
        </TabContainer>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { computed } from "vue"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import CategoryButton from "./FormModalButton.vue"
import ParentCategoryEditor from "./ParentCategoryEditor.vue"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const { translate } = useLang()
const tabs = [
    Tab.create("form", { title: translate("category"), isDefault: true }),
    Tab.create("hierarchy", { title: translate("hierarchy") }),
]
const { fromPool } = useEntityStore()
// child links are read-only here (they are edited from the child's side)
const children = computed(() => (item.value.childEntities ?? []).filter((x) => x.child).map((x) => fromPool(x.child as Entity)))
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
