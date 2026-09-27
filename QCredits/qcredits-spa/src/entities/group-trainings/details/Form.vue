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

        <div class="alert alert-info small py-2"><i class="bi bi-piggy-bank me-1"></i>{{ $t("groupTrainingFunding") }}</div>
        <TabContainer :tabs="tabs" :active="initialTab" :use-route-nav="!isPopup">
            <template #form>
                <FormSection :title="$t('groupTraining')" :readonly="readonly">
                    <div class="row g-3">
                        <div class="col-md-8">
                            <FormLabel :label="$t('title')" />
                            <input v-model="item.title" :readonly="readonly" class="form-control" required maxlength="150" />
                        </div>
                        <div class="col-md-4">
                            <FormLabel :label="$t('status')" />
                            <select v-model="item.status" class="form-select" :disabled="readonly">
                                <option v-for="s in Object.values(GroupTrainingStatus)" :key="s" :value="s">{{ $t(`gtStatus${s}`) }}</option>
                            </select>
                        </div>
                        <div class="col-md-6">
                            <FormLabel :label="$t('provider')" />
                            <input v-model="item.provider" :readonly="readonly" class="form-control" maxlength="150" />
                        </div>
                        <div class="col-md-6">
                            <FormLabel :label="$t('location')" />
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-geo-alt"></i></span>
                                <input v-model="item.location" :readonly="readonly" class="form-control" maxlength="150" />
                            </div>
                        </div>
                        <div class="col-6 col-md-4">
                            <FormLabel :label="$t('startDate')" />
                            <input v-model="item.startDate" type="date" :readonly="readonly" class="form-control" required />
                        </div>
                        <div class="col-6 col-md-4">
                            <FormLabel :label="$t('durationDays')" />
                            <input v-model.number="item.durationDays" type="number" min="0.5" max="30" step="0.5" :readonly="readonly" class="form-control" required />
                        </div>
                        <div class="col-6 col-md-4">
                            <FormLabel :label="$t('totalCostEur')" />
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-currency-euro"></i></span>
                                <input v-model.number="item.totalCost" type="number" min="0" step="1" :readonly="readonly" class="form-control" />
                            </div>
                        </div>
                        <div class="col-12">
                            <FormLabel :label="$t('description')" />
                            <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="3" maxlength="2000"></textarea>
                        </div>
                    </div>
                </FormSection>
            </template>
            <template #participants>
                <FormSection :title="$t('participants')">
                    <GroupTrainingParticipantOverview v-model="item.participants" :readonly="readonly" />
                </FormSection>
            </template>
        </TabContainer>

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { computed } from "vue"
import { useLang } from "@regira/modules/vue/lang"
import { GroupTrainingStatus } from "@/domain/enums"
import { GroupTrainingParticipantOverview } from "../group-training-participants"
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
const { translate } = useLang()
const tabs = computed(() => [
    Tab.create("form", { title: translate("groupTraining"), icon: "form", isDefault: true }),
    Tab.create("participants", { title: `${translate("participants")} (${item.value.participants?.filter((x) => !x._deleted).length ?? 0})`, icon: "people" }),
])
// the form's handleRemove() takes NO argument (it removes item.value) — unlike the overview's handleRemove(item)
</script>
