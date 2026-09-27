<template>
    <div class="row border-bottom py-2 align-items-center sm-row">
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
            <span class="sm-cat-icon me-2" :style="{ background: item.color }">{{ item.icon }}</span>
            <span class="fw-semibold">{{ item.$title }}</span>
            <small class="d-block d-md-none text-muted text-truncate">
                <template v-if="parents.length">{{ parents.map((x) => x.title).join(", ") }} &middot; </template>{{ $t("articleCount", { count: item.articleCount ?? 0 }) }}
            </small>
        </div>
        <div class="col d-none d-md-block text-truncate">
            <span v-for="p in parents" :key="p.id" class="me-1">
                <CategoryButton :model-value="p" class="btn btn-sm sm-chip sm-chip--sm">{{ p.icon }} {{ p.title }}</CategoryButton>
            </span>
            <span v-if="!parents.length" class="italic-muted small">{{ $t("topLevel") }}</span>
        </div>
        <!-- TODO: mirror List.vue's header slots 1:1 — same classes, same order, `text-truncate` on every
             text cell. A relation cell is the related entity's FormModalButton + its pooled label
             (`scaffold.mjs --rel <Related>` already wrote one above per relation); plain text is the
             exception — see entities.patterns.md → Resolving relations with fromPool.
        <div class="col d-none d-md-block text-truncate">{{ item.code }}</div>
        <div class="col d-none d-lg-block text-truncate">{{ item.status }}</div>
        <div class="col d-none d-xl-block text-truncate">{{ item.reference }}</div>
        -->
        <div class="col-2 d-none d-md-block text-end text-muted">{{ item.articleCount ?? 0 }}</div>

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
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import FormModalButton from "../details/FormModalButton.vue"
import CategoryButton from "../details/FormModalButton.vue"
import useEntityStore from "../data/store"
import { computed } from "vue"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
const { fromPool } = useEntityStore()
const parents = computed(() => (item.value.parentEntities ?? []).filter((x) => x.parent).map((x) => fromPool(x.parent as Entity)))
</script>
