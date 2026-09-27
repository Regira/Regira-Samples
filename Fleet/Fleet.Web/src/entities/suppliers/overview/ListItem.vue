<template>
    <div class="row fleet-table__row align-items-center" :class="{ 'fleet-row--inactive': !item.isActive }">
        <div class="col-auto">
            <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="btn btn-link p-1">
                <Icon name="edit" />
            </RouterLink>
        </div>
        <div class="col text-truncate">
            <span class="fw-semibold">{{ item.$title }}</span>
            <small v-if="item.contactPerson" class="d-block text-secondary text-truncate">{{ item.contactPerson }} &middot; {{ item.phone }}</small>
        </div>
        <div class="col-2 d-none d-md-block text-truncate">{{ item.city }}</div>
        <div class="col-4 d-none d-lg-block">
            <!-- compact capability codes (full chips with quick-edit live on the supplier form) -->
            <span v-for="c in shownTypes" :key="c.interventionTypeId" class="fleet-chip" :title="c.interventionType?.title">{{ c.interventionType?.code }}</span>
            <span v-if="moreTypes > 0" class="fleet-chip fleet-chip--more">+{{ moreTypes }}</span>
        </div>
        <div class="col-2 col-md-1 d-none d-sm-block"><RatingStars :value="item.rating" /></div>
        <div class="col-auto fleet-col-status">
            <span v-if="item.isActive" class="fleet-badge fleet-badge--good"><i class="bi bi-check-circle"></i><span class="d-none d-xl-inline">Active</span></span>
            <span v-else class="fleet-badge fleet-badge--neutral"><i class="bi bi-pause-circle"></i><span class="d-none d-xl-inline">Inactive</span></span>
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
import RatingStars from "@/components/RatingStars.vue"
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
const maxChips = 6
const shownTypes = computed(() => (item.value.interventionTypes ?? []).slice(0, maxChips))
const moreTypes = computed(() => Math.max(0, (item.value.interventionTypes?.length ?? 0) - maxChips))
</script>
