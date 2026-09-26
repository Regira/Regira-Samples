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

        <FormSection :title="$t('player')" :readonly="readonly" class="mb-3">
            <div class="row g-3">
                <div class="col-sm-8">
                    <FormLabel :label="$t('playerName')" />
                    <input v-model="item.playerName" :readonly="readonly" maxlength="64" class="form-control" required />
                </div>
                <div class="col-sm-4">
                    <FormLabel :label="$t('honorific')" />
                    <input v-model="item.honorific" :readonly="readonly" maxlength="32" class="form-control" />
                </div>
            </div>
        </FormSection>

        <FormSection v-if="item.id > 0" :title="$t('achievements')" :readonly="true" class="mb-3">
            <p class="small text-muted mb-2">{{ $t("scoresAreSacred") }}</p>
            <div class="row row-cols-2 row-cols-md-4 g-2 text-center">
                <div class="col"><div class="border rounded p-2"><div class="small text-muted">{{ $t("score") }}</div><div class="fs-4 fw-bold">{{ formatNumber(item.score, $culture, 0) }}</div></div></div>
                <div class="col"><div class="border rounded p-2"><div class="small text-muted">{{ $t("iq") }}</div><div class="fs-4 fw-bold">{{ item.iq ?? "…" }}</div></div></div>
                <div class="col"><div class="border rounded p-2"><div class="small text-muted">{{ $t("factsViewed") }}</div><div class="fs-4 fw-bold">{{ item.factsViewed }}</div></div></div>
                <div class="col"><div class="border rounded p-2"><div class="small text-muted">{{ $t("finished") }}</div><div class="fw-bold pt-2">{{ item.finished ? formatDate(item.finished, $culture) : "…" }}</div></div></div>
            </div>
            <div class="mt-2 text-center fw-bold">👑 {{ item.geniusTitle }}</div>
        </FormSection>

        <FormSection v-if="item.answers?.length" :title="$t('answers')" :readonly="true" class="mb-3">
            <div class="entity-list">
                <div class="row fw-bold border-bottom pb-2 small">
                    <div class="col-auto" style="width: 2.5rem">#</div>
                    <div class="col">{{ $t("question") }}</div>
                    <div class="col-3">{{ $t("answer") }}</div>
                    <div class="col-2 d-none d-md-block">{{ $t("strategy") }}</div>
                    <div class="col-1 text-end">{{ $t("points") }}</div>
                </div>
                <div v-for="a in item.answers" :key="a.id" class="row border-bottom py-2 small">
                    <div class="col-auto" style="width: 2.5rem">{{ a.sortOrder + 1 }}</div>
                    <div class="col text-truncate" :title="a.reactionText">{{ a.questionText }}</div>
                    <div class="col-3 text-truncate">
                        <span v-if="a.isCorrect === true" title="official">✅</span>
                        <span v-else-if="a.isCorrect === false" title="original">🦄</span>
                        {{ a.answerText }}
                        <span v-if="a.revealed" :title="$t('revealed')">🔍</span>
                    </div>
                    <div class="col-2 d-none d-md-block text-truncate">{{ $t("strategy" + a.strategy) }}</div>
                    <div class="col-1 text-end">{{ a.points }}</div>
                </div>
            </div>
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
import { formatDate, formatNumber } from "@regira/modules/vue/formatters"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"

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
