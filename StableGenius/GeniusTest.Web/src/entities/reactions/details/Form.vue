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

        <FormSection :title="$t('reaction')" :readonly="readonly" class="mb-3">
            <div class="row g-3">
                <div class="col-sm-6">
                    <FormLabel :label="$t('part')" />
                    <select v-model="item.part" :disabled="readonly" class="form-select">
                        <option v-for="p in Object.values(ReactionPart)" :key="p" :value="p">{{ $t("part" + p) }}</option>
                    </select>
                </div>
                <div class="col-sm-6">
                    <FormLabel :label="$t('strategy')" />
                    <select v-model="item.strategy" :disabled="readonly" class="form-select">
                        <option v-for="s in Object.values(SpinStrategy)" :key="s" :value="s">{{ $t("strategy" + s) }}</option>
                    </select>
                </div>
                <div class="col-12">
                    <div class="d-flex justify-content-between align-items-end mb-1">
                        <FormLabel :label="$t('text')" />
                        <EmojiPicker insert align="end" :readonly="readonly" @pick="insertEmoji" />
                    </div>
                    <textarea ref="textArea" v-model="item.text" :readonly="readonly" maxlength="512" rows="3" class="form-control" required></textarea>
                    <div class="form-text">
                        {{ $t("placeholdersHint") }}
                        <code v-for="p in placeholders" :key="p" class="me-1">{{ p }}</code>
                    </div>
                </div>
                <div class="col-12">
                    <div class="form-check form-switch">
                        <input id="reactionActive" v-model="item.isActive" :disabled="readonly" type="checkbox" class="form-check-input" />
                        <label for="reactionActive" class="form-check-label">{{ $t("isActive") }}</label>
                    </div>
                </div>
            </div>
        </FormSection>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { nextTick, ref } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity, { ReactionPart, SpinStrategy, placeholders } from "../data/Entity"
import useEntityStore from "../data/store"
import EmojiPicker from "@/components/emoji/EmojiPicker.vue"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService, props, emit })

// The emoji picker inserts at the cursor (or replaces the selection) and puts the cursor right after it.
const textArea = ref<HTMLTextAreaElement>()
async function insertEmoji(emoji: string) {
    const text = item.value.text ?? ""
    const start = textArea.value?.selectionStart ?? text.length
    const end = textArea.value?.selectionEnd ?? text.length
    item.value.text = text.slice(0, start) + emoji + text.slice(end)
    await nextTick()
    textArea.value?.focus()
    textArea.value?.setSelectionRange(start + emoji.length, start + emoji.length)
}
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
