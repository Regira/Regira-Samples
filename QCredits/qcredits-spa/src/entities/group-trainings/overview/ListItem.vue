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
            <span class="fw-semibold">{{ item.$title }}</span>
            <div class="small text-muted text-truncate">{{ item.provider }}<span v-if="item.location"> &middot; {{ item.location }}</span></div>
        </div>
        <div class="col-2 small">{{ item.startDate }}<div class="text-muted">{{ item.durationDays }} {{ $t("daysShort") }}</div></div>
        <div class="col-2 d-none d-md-block"><span class="badge" :class="groupTrainingStatusBadge[item.status]">{{ $t(`gtStatus${item.status}`) }}</span></div>
        <div class="col-1 d-none d-md-block text-end">{{ item.participantCount }}</div>
        <div class="col-2 d-none d-lg-block text-end">{{ formatEuro(item.totalCost) }}</div>

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
import { formatEuro, groupTrainingStatusBadge } from "@/domain/enums"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import FormModalButton from "../details/FormModalButton.vue"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
</script>
