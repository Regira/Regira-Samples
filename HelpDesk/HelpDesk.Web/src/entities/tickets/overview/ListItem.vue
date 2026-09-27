<template>
    <div class="row border-bottom py-2 align-items-center hd-queue-row" :style="{ '--hd-prio-color': item.priority?.color }">
        <div class="col-auto">
            <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1" :title="$t('open')">
                <Icon name="edit" />
            </RouterLink>
        </div>

        <div class="col text-truncate">
            <div class="d-flex align-items-center gap-2">
                <span class="hd-code">{{ item.code }}</span>
                <span v-if="item.commentCount" class="small text-muted"><i class="bi bi-chat-dots me-1"></i>{{ item.commentCount }}</span>
                <span v-if="item.attachmentCount" class="small text-muted"><i class="bi bi-paperclip"></i>{{ item.attachmentCount }}</span>
            </div>
            <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="fw-semibold text-body text-decoration-none d-block text-truncate">
                {{ item.title }}
            </RouterLink>
            <div class="d-flex flex-wrap gap-1 mt-1">
                <ColorBadge v-for="c in item.categories ?? []" :key="c.categoryId" :title="c.category?.title" :color="c.category?.color" :icon="c.category?.icon" />
                <span class="d-md-none"><ColorBadge :title="item.status?.title" :color="item.status?.color" solid /></span>
            </div>
        </div>
        <div class="col-2 d-none d-md-block">
            <ColorBadge :title="item.status?.title" :color="item.status?.color" solid />
        </div>
        <div class="col-2 d-none d-xl-block">
            <ColorBadge :title="item.priority?.title" :color="item.priority?.color" />
        </div>
        <div v-if="isStaff" class="col-2 d-none d-lg-block text-truncate small">
            <PersonChip :person="item.customer" show-button />
            <div class="text-muted text-truncate" style="font-size: 0.75rem">{{ item.customer?.company }}</div>
        </div>
        <div class="col-2 d-none d-xxl-block text-truncate small">
            <PersonChip :person="item.assignedEmployee" :show-button="isStaff" :muted="$t('unassigned')" />
        </div>
        <div class="col-2 col-lg-1 d-none d-sm-block text-end">
            <SlaIndicator :due-date="item.dueDate" :closed-at="item.closedAt" :is-closed="item.$isClosed" compact />
            <div class="text-muted" style="font-size: 0.72rem">{{ formatDate(item.lastModified ?? item.created) }}</div>
        </div>

        <div class="col-auto">
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
import Entity from "../data/Entity"
import ColorBadge from "@/components/tickets/ColorBadge.vue"
import SlaIndicator from "@/components/tickets/SlaIndicator.vue"
import PersonChip from "@/components/tickets/PersonChip.vue"
import { useAccess } from "@/infrastructure/access"

defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { isStaff } = useAccess()
</script>
