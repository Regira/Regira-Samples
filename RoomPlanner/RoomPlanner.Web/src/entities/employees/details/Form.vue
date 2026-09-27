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
        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="row g-2">
                <div class="col-md-6 mb-2">
                    <input v-model.trim="item.firstName" :readonly="readonly" class="form-control" required maxlength="64" />
                    <FormLabel :label="$t('firstName')" />
                </div>
                <div class="col-md-6 mb-2">
                    <input v-model.trim="item.lastName" :readonly="readonly" class="form-control" required maxlength="64" />
                    <FormLabel :label="$t('lastName')" />
                </div>
            </div>
            <div class="row g-2">
                <div class="col-md-6 mb-2">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="email" /></span>
                        <input type="email" v-model.trim="item.email" :readonly="readonly" class="form-control" required maxlength="256" />
                    </div>
                    <FormLabel :label="$t('email')" />
                </div>
                <div class="col-md-6 mb-2">
                    <div class="input-group">
                        <span class="input-group-text"><Icon name="phone" /></span>
                        <input v-model.trim="item.phone" :readonly="readonly" class="form-control" maxlength="32" />
                    </div>
                    <FormLabel :label="$t('phone')" />
                </div>
            </div>
            <div class="row g-2">
                <div class="col-md-6 mb-2">
                    <select v-model="item.department" :disabled="readonly" class="form-select">
                        <option :value="undefined">—</option>
                        <option v-for="d in departments" :key="d" :value="d">{{ d }}</option>
                    </select>
                    <FormLabel :label="$t('department')" />
                </div>
                <div class="col-md-6 mb-2">
                    <input v-model.trim="item.jobTitle" :readonly="readonly" class="form-control" maxlength="64" />
                    <FormLabel :label="$t('jobTitle')" />
                </div>
            </div>
            <div class="form-check mb-2">
                <input id="employee-active" type="checkbox" v-model="item.isActive" :disabled="readonly" class="form-check-input" />
                <label for="employee-active" class="form-check-label">{{ $t("isActive") }}</label>
            </div>
            <div v-if="item.id" class="mt-3">
                <RouterLink :to="{ name: 'calendar', query: { employeeId: item.id } }" class="btn btn-outline-primary btn-sm">
                    <i class="bi bi-calendar-week me-1"></i>{{ $t("agenda") }}
                </RouterLink>
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
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { departments } from "../data/Entity"

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
