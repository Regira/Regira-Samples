<template>
    <div class="row border-bottom py-2 align-items-center">
        <div class="col-auto">
            <RouterLink v-if="config.isComplex" :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
            <FormModalButton v-else v-model="item" :readonly="readonly" @save="$emit('save', $event)" @remove="$emit('remove', $event)" />
        </div>

        <div class="col text-truncate">
            <span v-if="item.finished" class="me-1" :title="$t('isFinished')">🏆</span>{{ item.playerName }}
            <small v-if="item.honorific" class="text-muted ms-1">({{ item.honorific }})</small>
        </div>
        <div class="col-3 d-none d-lg-block text-truncate">{{ item.geniusTitle }}</div>
        <div class="col-2 col-md-1 text-end text-truncate fw-bold">{{ formatNumber(item.score, $culture, 0) }}</div>
        <div class="col-1 d-none d-md-block text-end text-truncate">{{ item.iq ?? "…" }}</div>
        <div class="col-2 d-none d-xl-block text-truncate">{{ formatDate(item.created, $culture) }}</div>

        <div class="col-auto">
            <ConfirmButton v-if="!readonly" icon="delete" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </div>
    </div>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import { formatDate, formatNumber } from "@regira/modules/vue/formatters"
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
