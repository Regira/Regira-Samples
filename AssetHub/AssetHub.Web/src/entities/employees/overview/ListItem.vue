<template>
    <div class="row border-bottom py-2">
        <div class="col-auto">
            <!-- Row-edit affordance follows config.isComplex: a real entity (page) links to its Details route;
                 a very basic entity (modal) opens FormModalButton. Forward @remove either way so a delete from
                 inside the modal refreshes the pooled overview — without it the deleted row lingers until reload. -->
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" :readonly="readonly" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col-3 col-md-2 col-xxl-1 text-truncate"><span class="ah-code">{{ item.code }}</span></div>
        <div class="col text-truncate">
            <span class="ah-avatar me-2" :class="{ 'ah-avatar--inactive': !item.isActive }">{{ item.$initials }}</span>
            <span :class="{ 'text-decoration-line-through text-muted': !item.isActive }">{{ item.$title }}</span>
            <span v-if="!item.isActive" class="badge text-bg-secondary ms-2">{{ $t("inactive") }}</span>
            <div class="small text-muted text-truncate d-none d-sm-block ah-indent">{{ item.jobTitle }}</div>
        </div>
        <div class="col d-none d-md-block text-truncate">{{ item.department }}</div>
        <div class="col d-none d-xl-block text-truncate">
            <LocationButton v-if="item.location" :model-value="getLocation(item.location)" /> {{ getLocation(item.location)?.$title }}
        </div>
        <div class="col-2 col-lg-1 text-center">
            <RouterLink
                v-if="item.currentAssetCount"
                :to="{ name: 'AssetOverview', query: { employeeId: item.id } }"
                class="badge rounded-pill text-bg-primary text-decoration-none"
                :title="$t('currentAssets')"
                >{{ item.currentAssetCount }}</RouterLink
            >
            <span v-else class="text-muted">-</span>
        </div>

        <div class="col-auto">
            <!-- readonly comes from List.vue; it is the hook for permission-gating (entities.patterns.md -> Permission-gated UI) -->
            <ConfirmButton
                v-if="!readonly"
                icon="delete"
                :modal-type="ModalType.danger"
                :modal-title="$t('delete')"
                :modal-labels="{ cancel: $t('cancel'), submit: $t('delete') }"
                @confirm="$emit('request-remove', item)"
            >
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </div>
    </div>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import FormModalButton from "../details/FormModalButton.vue"
import { FormModalButton as LocationButton, useEntityStore as useLocationStore } from "@/entities/locations"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getLocation } = useLocationStore()
</script>
