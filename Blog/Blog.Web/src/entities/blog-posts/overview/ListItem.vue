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

        <div class="col text-truncate">
            <span v-if="item.isFeatured" class="text-warning me-1" :title="$t('featured')"><i class="bi bi-star-fill"></i></span>
            <span class="fw-semibold">{{ item.$title }}</span>
            <div class="small text-muted text-truncate d-md-none">{{ getCategory(item.category)?.$title }}</div>
        </div>
        <div class="col-2 d-none d-md-block text-truncate">
            <CategoryButton v-if="item.category" :model-value="getCategory(item.category)" /> {{ getCategory(item.category)?.$title }}
        </div>
        <div class="col-auto post-status-col d-none d-sm-block">
            <span class="badge" :class="statusClass[item.$status]">{{ $t("status" + item.$status) }}</span>
        </div>
        <div class="col-2 d-none d-lg-block text-truncate">{{ item.publishedAt ? formatDate(item.publishedAt, culture) : "-" }}</div>
        <div class="col-2 d-none d-xl-block text-truncate">{{ item.authorName }}</div>

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
import { formatDate } from "@regira/modules/vue/formatters"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity, { type PostStatus } from "../data/Entity"
import { useConfig } from "@/app-config"
import FormModalButton from "../details/FormModalButton.vue"
import { FormModalButton as CategoryButton, useEntityStore as useCategoryStore } from "@/entities/categories"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const culture = useConfig().culture
const statusClass: Record<PostStatus, string> = {
    Draft: "text-bg-secondary",
    Scheduled: "text-bg-info",
    Published: "text-bg-success",
}
const { fromPool: getCategory } = useCategoryStore()
</script>
