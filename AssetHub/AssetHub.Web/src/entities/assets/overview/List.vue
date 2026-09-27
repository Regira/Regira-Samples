<!-- Compact inventory table: tag, name, category, color-coded status, holder, location -->
<template>
    <div class="entity-list ah-table">
        <div class="row fw-bold border-bottom pb-1 ah-table__head">
            <div class="col-auto">
                <span v-if="config.isComplex" class="btn btn-link p-1 disabled"><Icon name="edit" /></span>
            </div>
            <div class="col-3 col-md-2 col-xl-1">{{ $t("assetTag") }}</div>
            <div class="col">{{ $t("asset") }}</div>
            <div class="col d-none d-md-block">{{ $t("category") }}</div>
            <div class="col-3 col-lg-2">{{ $t("status") }}</div>
            <div class="col d-none d-lg-block">{{ $t("assignedTo") }}</div>
            <div class="col d-none d-xl-block">{{ $t("location") }}</div>
            <div class="col-1 d-none d-xxl-block text-end">{{ $t("price") }}</div>
            <div class="col-auto">
                <span v-if="!readonly" class="btn disabled text-muted"><Icon name="delete" /></span>
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
