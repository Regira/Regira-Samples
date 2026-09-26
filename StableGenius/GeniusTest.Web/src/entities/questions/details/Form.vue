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

        <FormSection :title="$t('question')" :readonly="readonly" class="mb-3">
            <div class="row g-3">
                <div class="col-auto">
                    <FormLabel :label="$t('emoji')" />
                    <EmojiPicker v-model="item.emoji" :readonly="readonly" class="emoji-large" />
                </div>
                <div class="col">
                    <FormLabel :label="$t('question')" />
                    <input v-model="item.title" :readonly="readonly" maxlength="256" class="form-control fs-5" required />
                </div>
                <!-- column break: emoji + question get the whole first line -->
                <div class="w-100 mt-0"></div>
                <div class="col-sm-6 col-lg-4">
                    <FormLabel :label="$t('type')" />
                    <select v-model="item.type" :disabled="readonly" class="form-select">
                        <option v-for="t in Object.values(QuestionType)" :key="t" :value="t">{{ $t("type" + t) }}</option>
                    </select>
                </div>
                <div class="col-sm-6 col-lg-4">
                    <FormLabel :label="$t('category')" />
                    <select v-model="item.category" :disabled="readonly" class="form-select">
                        <option v-for="c in Object.values(QuestionCategory)" :key="c" :value="c">{{ $t("category" + c) }}</option>
                    </select>
                </div>
                <div class="col-lg-4 d-flex align-items-end">
                    <div class="form-check form-switch mb-2">
                        <input id="questionActive" v-model="item.isActive" :disabled="readonly" type="checkbox" class="form-check-input" />
                        <label for="questionActive" class="form-check-label">{{ $t("isActive") }}</label>
                    </div>
                    <div class="form-check form-switch mb-2 ms-3" :title="$t('alwaysAskedHint')">
                        <input id="questionAlwaysAsked" v-model="item.alwaysAsked" :disabled="readonly" type="checkbox" class="form-check-input" />
                        <label for="questionAlwaysAsked" class="form-check-label">{{ $t("alwaysAsked") }}</label>
                    </div>
                </div>
            </div>
        </FormSection>

        <FormSection v-if="item.type === QuestionType.Choice" :title="$t('options')" :readonly="readonly" class="mb-3">
            <p class="small text-muted mb-2">{{ $t("optionsHint") }}</p>
            <QuestionOptionOverview v-model="item.options" :readonly="readonly" />
        </FormSection>

        <FormSection v-else-if="item.type === QuestionType.Number" :title="$t('officialAnswer')" :readonly="readonly" class="mb-3">
            <div class="row g-3">
                <div class="col-sm-4">
                    <FormLabel :label="$t('correctNumber')" />
                    <input v-model.number="item.correctNumber" :readonly="readonly" type="number" step="any" class="form-control" required />
                </div>
                <div class="col-sm-4">
                    <FormLabel :label="$t('tolerance')" />
                    <input v-model.number="item.tolerance" :readonly="readonly" type="number" step="any" min="0" class="form-control" />
                </div>
                <div class="col-sm-4">
                    <FormLabel :label="$t('unit')" />
                    <input v-model="item.unit" :readonly="readonly" maxlength="32" class="form-control" placeholder="m" />
                </div>
            </div>
        </FormSection>

        <FormSection v-else :title="$t('ratingScale')" :readonly="readonly" class="mb-3">
            <div class="row g-3">
                <div class="col-6 col-sm-3">
                    <FormLabel :label="$t('minValue')" />
                    <input v-model.number="item.minValue" :readonly="readonly" type="number" class="form-control" placeholder="1" />
                </div>
                <div class="col-6 col-sm-3">
                    <FormLabel :label="$t('maxValue')" />
                    <input v-model.number="item.maxValue" :readonly="readonly" type="number" class="form-control" placeholder="10" />
                </div>
            </div>
        </FormSection>

        <FormSection :title="$t('revealNote')" :readonly="readonly" class="mb-3">
            <p class="small text-muted mb-2">{{ $t("revealNoteHint") }}</p>
            <textarea v-model="item.revealNote" :readonly="readonly" maxlength="512" rows="2" class="form-control"></textarea>
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
import Entity, { QuestionCategory, QuestionType } from "../data/Entity"
import useEntityStore from "../data/store"
import { QuestionOptionOverview } from "../question-options"
import EmojiPicker from "@/components/emoji/EmojiPicker.vue"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })
</script>
