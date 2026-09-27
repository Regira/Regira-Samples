<template>
    <div class="row fleet-table__row align-items-center" :class="{ 'fleet-row--inactive': !item.isActive }">
        <div class="col-auto">
            <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
        </div>
        <div class="col-2 col-lg-1 text-truncate"><code>{{ item.code }}</code></div>
        <div class="col text-truncate fw-semibold">{{ item.$title }}</div>
        <div class="col-2 d-none d-md-block text-truncate">
            <i :class="categoryIcon[item.category]" class="me-1 text-secondary"></i>{{ item.category }}
        </div>
        <div class="col-2 d-none d-lg-block text-truncate text-secondary small">{{ interval }}</div>
        <div class="col-3 col-md-2 col-lg-1 text-end fleet-num">{{ fmtMoney(item.standardCost) }}</div>
        <div class="col-1 d-none d-md-block">
            <span v-if="item.isActive" class="fleet-badge fleet-badge--good"><i class="bi bi-check"></i><span>Yes</span></span>
            <span v-else class="fleet-badge fleet-badge--neutral"><i class="bi bi-dash"></i><span>No</span></span>
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
import { computed } from "vue"
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import { categoryIcon, fmtMoney, fmtKm } from "@/infrastructure/domain"
import config from "../config/config"
import Entity from "../data/Entity"

defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const interval = computed(
    () =>
        [item.value.intervalKm ? `every ${fmtKm(item.value.intervalKm)}` : "", item.value.intervalMonths ? `${item.value.intervalMonths} months` : ""]
            .filter((x) => x)
            .join(" / ") || "on demand"
)
</script>
