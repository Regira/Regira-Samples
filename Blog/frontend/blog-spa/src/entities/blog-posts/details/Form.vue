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
                    @restore="handleRestore"
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

        <!-- Heavier form? Wrap sections in <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
             with one <template #key> per Tab.create(...) — see entities.advanced.example.md §5. -->
        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="mb-2">
                <input v-model="item.title" :readonly="readonly" class="form-control" />
                <FormLabel :label="$t('name')" />
            </div>
            <div class="mb-2">
                <input v-model="item.slug" :readonly="readonly" class="form-control" />
                <FormLabel :label="$t('slug')" />
            </div>
            <div class="row">
                <div class="col-md-6 mb-2">
                    <CategoryInputSelector v-model="item.category" v-model:idValue="item.categoryId" :readonly="readonly" />
                    <FormLabel :label="$t('category')" />
                </div>
                <div class="col-md-6 mb-2">
                    <input v-model="item.coverImageUrl" :readonly="readonly" class="form-control" placeholder="https://..." />
                    <FormLabel :label="$t('coverImageUrl')" />
                </div>
            </div>
            <div class="mb-2">
                <textarea v-model="item.summary" :readonly="readonly" class="form-control" rows="2" />
                <FormLabel :label="$t('summary')" />
            </div>
            <div class="mb-2">
                <textarea v-model="item.content" :readonly="readonly" class="form-control" rows="10" />
                <FormLabel :label="$t('content')" />
            </div>
            <div class="row align-items-center">
                <div class="col-md-4 mb-2">
                    <NullableCheckBox v-model="item.isPublished" id="isPublished" :label="$t('isPublished')" />
                </div>
                <div class="col-md-4 mb-2">
                    <input v-model="publishedAtLocal" type="datetime-local" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('publishedAt')" />
                </div>
            </div>
        </FormSection>

        <FormSection :title="$t('tags')" :readonly="readonly">
            <BlogPostTagOverview v-model="item.tags" />
        </FormSection>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, NullableCheckBox } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import { BlogPostTagOverview } from "../blog-post-tags"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)

// datetime-local wants "YYYY-MM-DDTHH:mm" without a timezone suffix
const publishedAtLocal = computed<string>({
    get: () => {
        const d = item.value.publishedAt
        if (!d) return ""
        const pad = (n: number) => String(n).padStart(2, "0")
        return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
    },
    set: (v) => {
        item.value.publishedAt = v ? new Date(v) : undefined
    },
})
</script>
