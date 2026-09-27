<template>
    <ListCard :item="item" :show-shopper="true">
        <template #actions>
            <div class="d-flex align-items-center">
                <!-- the related shopper opens in its own modal form -->
                <ShopperButton v-if="item.shopper" :model-value="getShopper(item.shopper)" class="sm-avatar-btn" :title="getShopper(item.shopper)?.$title">
                    <span class="sm-avatar sm-avatar--sm" :style="{ background: getShopper(item.shopper)?.color || '#6c757d' }">
                        {{ getShopper(item.shopper)?.$initials }}
                    </span>
                </ShopperButton>
                <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="sm-icon-btn" :aria-label="$t('editList')">
                    <Icon name="edit" />
                </RouterLink>
                <ConfirmButton
                    v-if="!readonly"
                    class="sm-icon-btn text-danger"
                    icon="delete"
                    :modal-type="ModalType.danger"
                    :modal-title="$t('delete')"
                    :modal-labels="{ cancel: $t('cancel'), submit: $t('delete') }"
                    @confirm="$emit('request-remove', item)"
                >
                    {{ $t("deleteListConfirm", { title: item?.$title, count: item.articleCount ?? 0 }) }}
                </ConfirmButton>
            </div>
        </template>
    </ListCard>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import ListCard from "./ListCard.vue"
import { FormModalButton as ShopperButton, useEntityStore as useShopperStore } from "@/entities/shoppers"

defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool: getShopper } = useShopperStore()
</script>
