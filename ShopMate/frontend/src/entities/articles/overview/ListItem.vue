<template>
    <SwipeRow :disabled="readonly" @swipe-right="toggle" @swipe-left="confirm?.open()">
        <template #start>
            <i class="bi fs-4 me-2" :class="item.isActive ? 'bi-check2-circle' : 'bi-arrow-counterclockwise'"></i>
            {{ item.isActive ? $t("bought") : $t("needAgain") }}
        </template>
        <template #end>{{ $t("delete") }} <i class="bi bi-trash3 fs-4 ms-2"></i></template>

        <div class="row align-items-center sm-row" :class="{ 'is-bought': !item.isActive }">
            <div class="col-auto">
                <button
                    type="button"
                    class="sm-check"
                    :class="{ 'is-checked': !item.isActive }"
                    :disabled="readonly || isBusy"
                    :aria-pressed="!item.isActive"
                    :aria-label="item.isActive ? $t('markBought') : $t('markNeeded')"
                    data-no-swipe
                    @click="toggle"
                >
                    <i class="bi" :class="item.isActive ? 'bi-circle' : 'bi-check-circle-fill'"></i>
                </button>
            </div>

            <div class="col text-truncate">
                <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="sm-row__link">
                    <span class="sm-article__title">{{ item.$title }}</span>
                    <span v-if="item.$amount" class="sm-article__amount ms-2">{{ item.$amount }}</span>
                </RouterLink>
                <small class="d-block d-md-none text-muted text-truncate">
                    <span v-if="list">{{ list.$title }}</span>
                    <span v-for="c in categories" :key="c.id" class="ms-1">{{ c.icon }}</span>
                </small>
            </div>
            <div class="col-3 d-none d-md-block text-truncate">
                <ShoppingListButton v-if="item.shoppingList" :model-value="list" class="btn btn-sm btn-link px-1" /> {{ list?.$title }}
            </div>
            <div class="col-2 d-none d-lg-block text-truncate">
                <span v-for="c in categories" :key="c.id" class="sm-mini-chip me-1" :title="c.title">{{ c.icon }}</span>
            </div>

            <div class="col-auto">
                <ConfirmButton
                    v-if="!readonly"
                    ref="confirm"
                    class="d-none d-md-inline-block"
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
    </SwipeRow>
</template>

<script setup lang="ts">
import { computed, ref } from "vue"
import { RouterLink } from "vue-router"
import { get } from "@regira/modules/vue/ioc"
import { ModalType, ConfirmButton, useAppFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import type EntityService from "../data/EntityService"
import { FormModalButton as ShoppingListButton, useEntityStore as useShoppingListStore } from "@/entities/shopping-lists"
import { useCategoryTree } from "@/entities/categories"
import SwipeRow from "@/components/ui/SwipeRow.vue"

defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
const props = defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const confirm = ref<InstanceType<typeof ConfirmButton>>()
const { fromPool: getShoppingList } = useShoppingListStore()
const tree = useCategoryTree()
const appFeedback = useAppFeedback()
const isBusy = ref(false)

const list = computed(() => (item.value.shoppingList ? getShoppingList(item.value.shoppingList) : undefined))
const categories = computed(() =>
    (item.value.categories ?? []).map((x) => tree.byId.get(x.categoryId) ?? x.category).filter((x) => x != null) as Array<{ id: number; title: string; icon?: string }>
)

// single-field PATCH through the raw service (the pooled store only exposes the CRUD surface)
async function toggle() {
    if (props.readonly || isBusy.value) return
    const next = !item.value.isActive
    item.value.isActive = next
    isBusy.value = true
    try {
        await get<EntityService>(Entity.name)!.setActive(item.value.id, next)
    } catch (ex) {
        console.error(ex)
        item.value.isActive = !next
        appFeedback.fail("Updating the article failed", toFeedbackError(ex))
    } finally {
        isBusy.value = false
    }
}
</script>
