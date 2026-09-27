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
            <span class="room-dot me-2" :style="{ backgroundColor: item.$color }"></span>{{ item.$title }}
            <small class="text-muted ms-1">{{ item.code }}</small>
            <span v-if="item.requiresApproval" class="badge text-bg-warning ms-1" :title="$t('requiresApproval')"><i class="bi bi-shield-lock"></i></span>
            <span v-if="!item.isActive" class="badge text-bg-secondary ms-1">{{ $t("outOfService") }}</span>
        </div>
        <div class="col d-none d-md-block text-truncate">
            <FloorButton v-if="item.floor" :model-value="getFloor(item.floor)" /> {{ getFloor(item.floor)?.$title }}
        </div>
        <div class="col-1 d-none d-sm-block text-end text-truncate"><i class="bi bi-people me-1 text-muted"></i>{{ item.capacity }}</div>
        <div class="col-3 d-none d-lg-block text-truncate">
            <i v-for="eq in item.equipment" :key="eq.id" :class="`bi bi-${eq.equipment?.icon || 'tools'}`" class="me-2 text-primary" :title="eq.equipment?.title"></i>
        </div>
        <div class="col-2 d-none d-lg-block text-truncate">{{ formatDate(item.created) }}</div>

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
import Entity from "../data/Entity"
import FormModalButton from "../details/FormModalButton.vue"
import { FormModalButton as FloorButton, useEntityStore as useFloorStore } from "@/entities/floors"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getFloor } = useFloorStore()
</script>
