<template>
    <div class="col">
        <div class="card h-100" :class="{ 'border-secondary opacity-75': !item.isActive }">
            <div class="card-body d-flex flex-column">
                <div class="d-flex justify-content-between align-items-start mb-1">
                    <h5 class="card-title mb-0 text-truncate">{{ item.$title }}</h5>
                    <span class="badge rounded-pill" :class="item.isActive ? 'text-bg-success' : 'text-bg-secondary'">
                        {{ item.isActive ? $t("active") : $t("inactive") }}
                    </span>
                </div>
                <div class="text-muted small mb-2 text-truncate">
                    <FloorButton :model-value="getFloor(item.floor)" />
                    {{ getFloor(item.floor)?.building?.title }} &middot; {{ getFloor(item.floor)?.$title }}
                </div>
                <div class="mb-2">
                    <span class="badge text-bg-light border me-1"><i class="bi bi-people"></i> {{ item.capacity }}</span>
                    <span v-if="item.requiresApproval" class="badge text-bg-warning me-1">{{ $t("requiresApproval") }}</span>
                </div>
                <div class="mb-3">
                    <span v-for="eq in equipmentList" :key="eq" class="badge text-bg-info-subtle text-info-emphasis border me-1 mb-1">{{ eq }}</span>
                    <span v-if="equipmentList.length === 0" class="text-muted small">{{ $t("noEquipment") }}</span>
                </div>
                <div class="mt-auto d-flex justify-content-between align-items-center">
                    <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-sm btn-outline-info">
                        <Icon name="edit" /> {{ $t("edit") }}
                    </RouterLink>
                    <ConfirmButton icon="delete" class="btn-sm" :modal-type="ModalType.danger" @confirm="$emit('request-remove', item)">
                        {{ $t("deleteItem", { title: item?.$title }) }}
                    </ConfirmButton>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
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

const equipmentList = computed(() =>
    (item.value.equipment ?? "")
        .split(",")
        .map((s) => s.trim())
        .filter(Boolean)
)
</script>
