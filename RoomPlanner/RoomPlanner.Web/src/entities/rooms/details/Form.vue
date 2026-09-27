<template>
    <form @submit.prevent="handleSubmit">
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
                <RouterLink v-if="item.id && !isPopup" :to="{ name: 'ReservationDetails', params: { id: 'new' }, query: { roomId: item.id } }" class="btn btn-primary">
                    <i class="bi bi-calendar-plus"></i><span class="d-none d-md-inline ms-1">{{ $t("book") }}</span>
                </RouterLink>
                <RouterLink v-if="isPopup" :to="{ name: `${config.key}Details`, params: { id: item.$id } }" target="_blank" class="btn btn-outline-secondary" :title="$t('popOut')">
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
                    <div class="row g-2">
                        <div class="col-md-6 mb-2">
                            <input v-model="item.title" :readonly="readonly" class="form-control" required maxlength="64" />
                            <FormLabel :label="$t('name')" />
                        </div>
                        <div class="col-6 col-md-3 mb-2">
                            <input v-model="item.code" :readonly="readonly" class="form-control" maxlength="16" />
                            <FormLabel :label="$t('code')" />
                        </div>
                        <div class="col-6 col-md-3 mb-2">
                            <div class="input-group">
                                <input type="color" v-model="item.color" :disabled="readonly" class="form-control form-control-color" />
                                <span class="input-group-text flex-fill"><code>{{ item.color }}</code></span>
                            </div>
                            <FormLabel :label="$t('color')" />
                        </div>
                    </div>
                    <div class="mb-2">
                        <FloorInputSelector v-model="item.floor" v-model:idValue="item.floorId" :readonly="readonly" />
                        <FormLabel :label="$t('floor')" />
                    </div>
                    <div class="row g-2 align-items-start">
                        <div class="col-6 col-md-3 mb-2">
                            <div class="input-group">
                                <span class="input-group-text"><Icon name="people" /></span>
                                <input type="number" min="1" max="1000" v-model.number="item.capacity" :readonly="readonly" class="form-control" required />
                            </div>
                            <FormLabel :label="$t('capacity')" />
                        </div>
                        <div class="col-md-9 mb-2 d-flex flex-wrap gap-4 pt-2">
                            <div class="form-check">
                                <input id="room-approval" type="checkbox" v-model="item.requiresApproval" :disabled="readonly" class="form-check-input" />
                                <label for="room-approval" class="form-check-label"><i class="bi bi-shield-lock me-1"></i>{{ $t("requiresApproval") }}</label>
                            </div>
                            <div class="form-check">
                                <input id="room-active" type="checkbox" v-model="item.isActive" :disabled="readonly" class="form-check-input" />
                                <label for="room-active" class="form-check-label">{{ $t("isActive") }}</label>
                            </div>
                        </div>
                    </div>
                    <div class="mb-2">
                        <textarea v-model="item.description" :readonly="readonly" class="form-control" rows="3" maxlength="1024"></textarea>
                        <FormLabel :label="$t('description')" />
                    </div>
                </FormSection>
            </template>
            <template #equipment>
                <FormSection :title="$t('equipment')" :readonly="readonly">
                    <RoomEquipmentOverview v-model="item.equipment" :readonly="readonly" />
                </FormSection>
            </template>
            <template #schedule>
                <RoomAgenda v-if="item.id" :room="item" />
            </template>
        </TabContainer>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon, TabContainer, Tab } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import { RoomEquipmentOverview } from "../room-equipments"
import { InputSelector as FloorInputSelector } from "@/entities/floors"
import RoomAgenda from "@/components/planning/RoomAgenda.vue"

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
    Tab.create("form", { icon: "form", title: translate("room"), isDefault: true }),
    Tab.create("equipment", { icon: "tools", title: `${translate("equipment")} (${item.value.equipment?.filter((x) => !x._deleted).length ?? 0})` }),
    Tab.create("schedule", { icon: "calendar", title: translate("schedule"), isDisabled: !item.value.id }),
])
</script>
