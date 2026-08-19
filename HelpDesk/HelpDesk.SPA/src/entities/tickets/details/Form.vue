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

        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
                    <div class="mb-2">
                        <input v-model="item.title" required :readonly="readonly" class="form-control" :placeholder="$t('subject')" />
                        <FormLabel :label="$t('subject')" />
                    </div>
                    <div class="mb-2">
                        <DescriptionInput v-model="item.description" :readonly="readonly" :rows="4" />
                        <FormLabel :label="$t('description')" />
                    </div>

                    <div class="row">
                        <div class="col-md-3 mb-2">
                            <PersonInputSelector
                                v-model="item.customer"
                                v-model:idValue="item.customerId as number"
                                :readonly="readonly"
                                :filter-defaults="{ role: ['Customer'] }"
                            />
                            <FormLabel :label="$t('customer')" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <PersonInputSelector
                                v-model="item.assignedEmployee"
                                v-model:idValue="item.assignedEmployeeId as number"
                                :readonly="readonly"
                                :canEdit="false"
                                :filter-defaults="{ role: ['Agent'] }"
                            />
                            <FormLabel :label="$t('assignedEmployee')" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <SupportTeamInputSelector
                                v-model="item.supportTeam"
                                v-model:idValue="item.supportTeamId as number"
                                :readonly="readonly"
                                :canEdit="false"
                            />
                            <FormLabel :label="$t('supportTeam')" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-3 mb-2">
                            <PriorityInputSelector
                                v-model="item.priority"
                                v-model:idValue="item.priorityId as number"
                                :readonly="readonly"
                                :canEdit="false"
                            />
                            <FormLabel :label="$t('priority')" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <StatusInputSelector v-model="item.status" v-model:idValue="item.statusId as number" :readonly="readonly" :canEdit="false" />
                            <FormLabel :label="$t('status')" />
                        </div>
                        <div v-if="item.closedAt" class="col-md-3 mb-2">
                            <input class="form-control" readonly :value="format(item.closedAt, 'dd/MM/yyyy HH:mm')" />
                            <FormLabel :label="$t('closedAt')" />
                        </div>
                    </div>
                </FormSection>

                <FormSection :title="$t('categories')" :readonly="readonly">
                    <InputSelectorInline v-model="item.categories" :row-key="(r) => r.categoryId" :exclude-key="(r) => r.categoryId">
                        <template #chip="{ row }">
                            <CategoryButton :modelValue="hydrateCategory(row.category)" />
                            {{ hydrateCategory(row.category)?.$title }}
                        </template>
                        <template #selector="{ add, exclude }">
                            <CategorySelector
                                :filter-defaults="{ exclude }"
                                @select="(c?: Category) => c && add({ categoryId: c.id!, category: c })"
                            />
                        </template>
                    </InputSelectorInline>
                </FormSection>
            </template>

            <template #comments>
                <FormSection :title="$t('comments')">
                    <CommentsPanel :ticket-id="item.id > 0 ? item.id : undefined" :readonly="readonly" />
                </FormSection>
            </template>

            <template #files>
                <EntityAttachments v-model="item.attachments" :readonly="readonly" />
            </template>
        </TabContainer>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab, DescriptionInput } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults, InputSelectorInline } from "@regira/modules/vue/entities"
import { useLang } from "@regira/modules/vue/lang"
import { format } from "date-fns"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { InputSelector as PersonInputSelector } from "@/entities/people"
import { InputSelector as PriorityInputSelector } from "@/entities/priorities"
import { InputSelector as StatusInputSelector } from "@/entities/statuses"
import { InputSelector as SupportTeamInputSelector } from "@/entities/support-teams"
import {
    InputSelector as CategorySelector,
    FormModalButton as CategoryButton,
    useEntityStore as useCategoryStore,
    type Entity as Category,
} from "@/entities/categories"
import { Overview as EntityAttachments } from "../../entity-attachments"
import CommentsPanel from "../comments/CommentsPanel.vue"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

const { fromPool: hydrateCategory } = useCategoryStore()

const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { title: translate("form"), icon: "form", isDefault: true }),
    Tab.create("comments", { title: translate("comments"), icon: "bi bi-chat-dots", isDisabled: !item.value?.id }),
    Tab.create("files", { title: translate("files"), icon: "attachment" }),
])
</script>
