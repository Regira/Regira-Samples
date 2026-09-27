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
        <FormSection :title="$t('personalDetails')" :readonly="readonly">
            <div class="row g-3">
                <div class="col-md-6">
                    <FormLabel :label="$t('firstName')" />
                    <input v-model="item.firstName" :readonly="readonly" class="form-control" required maxlength="64" />
                </div>
                <div class="col-md-6">
                    <FormLabel :label="$t('lastName')" />
                    <input v-model="item.lastName" :readonly="readonly" class="form-control" required maxlength="64" />
                </div>
                <div class="col-md-6">
                    <FormLabel :label="$t('email')" />
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                        <input v-model="item.email" type="email" :readonly="readonly" class="form-control" required maxlength="256" />
                    </div>
                </div>
                <div class="col-md-6">
                    <FormLabel :label="$t('hireDate')" />
                    <input v-model="item.hireDate" type="date" :readonly="readonly" class="form-control" required />
                </div>
            </div>
        </FormSection>
        <FormSection :title="$t('position')" :readonly="readonly" class="mt-3">
            <div class="row g-3">
                <div class="col-md-6">
                    <FormLabel :label="$t('department')" />
                    <DepartmentInputSelector v-model="item.department" v-model:idValue="item.departmentId" :readonly="readonly" :canEdit="false" />
                </div>
                <div class="col-md-6">
                    <FormLabel :label="$t('jobTitle')" />
                    <input v-model="item.jobTitle" :readonly="readonly" class="form-control" maxlength="100" />
                </div>
                <div class="col-12">
                    <div class="form-check form-switch">
                        <input id="employeeActive" v-model="item.isActive" type="checkbox" class="form-check-input" :disabled="readonly" />
                        <label for="employeeActive" class="form-check-label">{{ $t("isActive") }}</label>
                    </div>
                </div>
            </div>
        </FormSection>
        <EmployeeBalances v-if="item.id > 0" :employee-id="item.id" class="mt-3" />

        <!-- <Debug> dumps the live payload, self-gated on $isDebug (?debug=1) — inert in production; curate the payload. -->
        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import { InputSelector as DepartmentInputSelector } from "@/entities/departments"
import EmployeeBalances from "./EmployeeBalances.vue"
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
