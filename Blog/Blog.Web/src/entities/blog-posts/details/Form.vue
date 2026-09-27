<template>
    <form @submit.prevent="handleSubmit">
        <!-- Action bar: save/delete buttons, the way back (overview / public page), feedback. -->
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
            <div class="col-auto order-2 order-md-3 d-flex gap-2">
                <RouterLink
                    v-if="item.id > 0 && item.slug && item.$status === 'Published'"
                    :to="{ name: 'post', params: { slug: item.slug } }"
                    target="_blank"
                    class="btn btn-outline-secondary"
                    :title="$t('viewOnBlog')"
                >
                    <Icon name="website" /> <span class="d-none d-lg-inline ms-1">{{ $t("viewOnBlog") }}</span>
                </RouterLink>
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

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <!-- main tab: the article's metadata -->
            <template #form>
                <FormSection :title="$t('article')" :readonly="readonly" class="mb-3">
                    <div class="mb-3">
                        <input v-model="item.title" :readonly="readonly" class="form-control form-control-lg post-title-input" required maxlength="160" />
                        <FormLabel :label="$t('title')" />
                    </div>
                    <div class="mb-3">
                        <textarea v-model="item.summary" :readonly="readonly" class="form-control" rows="2" maxlength="512"></textarea>
                        <FormLabel :label="$t('summary')" />
                    </div>
                    <div class="row g-3 mb-2">
                        <div class="col-md-6">
                            <CategoryInputSelector v-model="item.category" v-model:idValue="item.categoryId" :readonly="readonly" :placeholder="$t('category')" />
                            <FormLabel :label="$t('category')" />
                        </div>
                        <div class="col-md-6">
                            <div class="input-group">
                                <span class="input-group-text"><Icon name="user" /></span>
                                <input v-model.trim="item.authorName" :readonly="readonly" class="form-control" maxlength="96" />
                            </div>
                            <FormLabel :label="$t('author')" />
                        </div>
                    </div>
                    <div class="mb-2">
                        <BlogPostTagOverview v-model="item.tags" />
                        <FormLabel :label="$t('tags')" />
                    </div>
                </FormSection>

                <div class="row g-3">
                    <div class="col-lg-6">
                        <FormSection :title="$t('publication')" :readonly="readonly" class="mb-3 h-100">
                            <div class="d-flex flex-wrap gap-4 mb-3">
                                <div class="form-check form-switch">
                                    <input id="isPublished" v-model="item.isPublished" :disabled="readonly" class="form-check-input" type="checkbox" role="switch" />
                                    <label class="form-check-label" for="isPublished">{{ $t("isPublished") }}</label>
                                </div>
                                <div class="form-check form-switch">
                                    <input id="isFeatured" v-model="item.isFeatured" :disabled="readonly" class="form-check-input" type="checkbox" role="switch" />
                                    <label class="form-check-label" for="isFeatured">{{ $t("featured") }}</label>
                                </div>
                            </div>
                            <div class="mb-2">
                                <DateInput v-model="item.publishedAt" :show-time="true" :readonly="readonly" />
                                <FormLabel :label="$t('publishedAtHint')" />
                            </div>
                            <p class="small mb-0">
                                <span class="badge me-2" :class="statusClass[item.$status]">{{ $t("status" + item.$status) }}</span>
                                <span class="text-muted">{{ $t("statusHint" + item.$status) }}</span>
                            </p>
                        </FormSection>
                    </div>
                    <div class="col-lg-6">
                        <FormSection :title="$t('presentation')" :readonly="readonly" class="mb-3 h-100">
                            <div class="mb-3">
                                <div class="input-group">
                                    <span class="input-group-text"><Icon name="website" /></span>
                                    <input v-model.trim="item.slug" :readonly="readonly" class="form-control" maxlength="128" :placeholder="$t('slugAuto')" />
                                </div>
                                <FormLabel :label="$t('slug')" />
                            </div>
                            <div class="mb-2">
                                <input v-model.trim="item.coverImageUrl" :readonly="readonly" type="url" class="form-control" maxlength="512" placeholder="https://" />
                                <FormLabel :label="$t('coverImageUrl')" />
                            </div>
                            <img v-if="item.coverImageUrl" :src="item.coverImageUrl" alt="" class="cover-preview rounded mt-2" loading="lazy" />
                        </FormSection>
                    </div>
                </div>
            </template>

            <!-- content tab: Markdown editor with a live preview -->
            <template #content>
                <FormSection :title="$t('content')" :readonly="readonly">
                    <div class="row g-3">
                        <div class="col-lg-6">
                            <textarea v-model="item.content" :readonly="readonly" class="form-control content-editor" rows="24"></textarea>
                            <FormLabel :label="$t('contentHint')" />
                            <div class="small text-muted mt-1">
                                <Icon name="timespan" /> {{ $t("readingTimeMin", { min: estimateReadingTime(item.content) }) }}
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="content-preview border rounded p-3">
                                <h1 class="article-title h2 mb-3">{{ item.title }}</h1>
                                <div class="article-body" v-html="renderMarkdown(item.content)"></div>
                            </div>
                        </div>
                    </div>
                </FormSection>
            </template>
        </TabContainer>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab, DateInput } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { InputSelector as CategoryInputSelector } from "@/entities/categories"
import { renderMarkdown, estimateReadingTime } from "@/utilities/markdown"
import config from "../config/config"
import Entity, { type PostStatus } from "../data/Entity"
import useEntityStore from "../data/store"
import { BlogPostTagOverview } from "../blog-post-tags"

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
    Tab.create("form", { icon: "form", title: translate("article"), isDefault: true }),
    Tab.create("content", { icon: "markdown", title: translate("content") }),
])

const statusClass: Record<PostStatus, string> = {
    Draft: "text-bg-secondary",
    Scheduled: "text-bg-info",
    Published: "text-bg-success",
}
</script>
