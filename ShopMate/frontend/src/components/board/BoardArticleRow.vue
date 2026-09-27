<!--
    One article on the shopping-list board.
    tap the circle / swipe right: tick off (or put back) - swipe left: delete (confirmed) - tap the text: edit -
    drag the grip: reorder (the focused grip also answers ArrowUp / ArrowDown).
-->
<script setup lang="ts">
import { computed, ref } from "vue"
import { ConfirmButton, ModalType } from "@regira/modules/vue/ui"
import { FormModalButton as ArticleButton, type Entity as Article } from "@/entities/articles"
import { useCategoryTree } from "@/entities/categories"
import SwipeRow from "@/components/ui/SwipeRow.vue"

const props = defineProps<{ article: Article; sortable?: boolean; dragging?: boolean; offsetY?: number }>()
const emit = defineEmits<{
    (e: "toggle", article: Article): void
    (e: "remove", article: Article): void
    (e: "changed"): void
    (e: "grip-down", ev: PointerEvent): void
    (e: "grip-move", ev: PointerEvent): void
    (e: "grip-up", ev: PointerEvent): void
    (e: "move", delta: number): void
}>()

const tree = useCategoryTree()
const confirm = ref<InstanceType<typeof ConfirmButton>>()
const categories = computed(() =>
    (props.article.categories ?? []).map((x) => tree.byId.get(x.categoryId) ?? x.category).filter((x) => x != null)
)
</script>

<template>
    <div class="sm-article" data-drag-row :class="{ 'is-bought': !article.isActive, 'is-dragging': dragging }" :style="dragging ? { transform: `translateY(${offsetY}px)` } : undefined">
        <SwipeRow :disabled="dragging" @swipe-right="emit('toggle', article)" @swipe-left="confirm?.open()">
            <template #start>
                <i class="bi fs-4 me-2" :class="article.isActive ? 'bi-check2-circle' : 'bi-arrow-counterclockwise'"></i>
                {{ article.isActive ? $t("bought") : $t("needAgain") }}
            </template>
            <template #end>
                {{ $t("delete") }} <i class="bi bi-trash3 fs-4 ms-2"></i>
            </template>

            <div class="sm-article__inner">
                <button
                    type="button"
                    class="sm-check"
                    :class="{ 'is-checked': !article.isActive }"
                    :aria-label="article.isActive ? $t('markBought') : $t('markNeeded')"
                    :aria-pressed="!article.isActive"
                    data-no-swipe
                    @click="emit('toggle', article)"
                >
                    <i class="bi" :class="article.isActive ? 'bi-circle' : 'bi-check-circle-fill'"></i>
                </button>

                <ArticleButton :model-value="article" class="sm-article__main" :close-on-save="true" @save="emit('changed')" @remove="emit('changed')">
                    <span class="sm-article__title">{{ article.title }}</span>
                    <span class="sm-article__meta">
                        <span v-if="article.$amount" class="sm-article__amount">{{ article.$amount }}</span>
                        <span v-if="article.description" class="sm-article__note text-truncate">{{ article.description }}</span>
                    </span>
                </ArticleButton>

                <span class="sm-article__cats" aria-hidden="true">
                    <span v-for="c in categories.slice(0, 3)" :key="c!.id" class="sm-mini-chip" :title="c!.title">{{ c!.icon }}</span>
                </span>

                <button
                    v-if="sortable"
                    type="button"
                    class="sm-grip"
                    :aria-label="$t('reorder')"
                    data-no-swipe
                    @pointerdown="emit('grip-down', $event)"
                    @pointermove="emit('grip-move', $event)"
                    @pointerup="emit('grip-up', $event)"
                    @pointercancel="emit('grip-up', $event)"
                    @keydown.up.prevent="emit('move', -1)"
                    @keydown.down.prevent="emit('move', 1)"
                >
                    <i class="bi bi-grip-vertical"></i>
                </button>
            </div>
        </SwipeRow>

        <!-- the swipe-left target: kept in the tree so the swipe can raise the same confirmation -->
        <ConfirmButton
            ref="confirm"
            class="d-none"
            icon="delete"
            :modal-type="ModalType.danger"
            :modal-title="$t('delete')"
            :modal-labels="{ cancel: $t('cancel'), submit: $t('delete') }"
            @confirm="emit('remove', article)"
        >
            {{ $t("deleteItem", { title: article.title }) }}
        </ConfirmButton>
    </div>
</template>
