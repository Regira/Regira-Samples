<template>
    <div class="entity-list">
        <div class="row fw-bold border-bottom pb-2">
            <div class="col-auto">
                <span v-if="config.isComplex" class="btn btn-link p-1 disabled"><Icon name="edit" /></span>
                <button v-else type="button" class="btn btn-default" disabled><Icon :name="config.key" /></button>
            </div>
            <div class="col">{{ $t("code") }}</div>
            <div class="col d-none d-md-block">{{ $t("supplier") }}</div>
            <div class="col d-none d-lg-block">{{ $t("status") }}</div>
            <div class="col d-none d-xl-block">{{ $t("dueDate") }}</div>
            <div class="col-2 text-end">{{ $t("totalAmount") }}</div>
            <div class="col-auto">
                <span class="btn disabled text-muted"><Icon name="delete" /></span>
            </div>
        </div>
        <ListItem
            v-for="(item, i) in items"
            :key="item.$id"
            v-model="items[i]!"
            :readonly="readonly"
            @request-save="$emit('request-save', $event)"
            @request-remove="$emit('request-remove', $event)"
            @save="$emit('save', $event)"
            @remove="$emit('remove', $event)"
        />
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { Icon } from "@regira/modules/vue/ui"
import type { OverviewEmits } from "@regira/modules/vue/entities"
import config from "../config/config"
import type Entity from "../data/Entity"
import useEntityStore from "../data/store"
import ListItem from "./ListItem.vue"

interface Emits extends /* @vue-ignore */ OverviewEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = defineProps<{ modelValue?: Array<Entity>; readonly?: boolean }>()

const { fromPool } = useEntityStore()
const items = computed<Array<Entity>>({
    get: () => fromPool(props.modelValue || []),
    set: (value) => emit("update:modelValue", value),
})
</script>
