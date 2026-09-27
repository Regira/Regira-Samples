<template>
    <!-- speaker cards: a responsive grid instead of table rows -->
    <div class="row row-cols-1 row-cols-sm-2 row-cols-lg-3 row-cols-xxl-4 g-3 mt-1">
        <div v-for="(item, i) in items" :key="item.$id" class="col">
            <ListItem
                v-model="items[i]!"
                :readonly="readonly"
                @request-save="$emit('request-save', $event)"
                @request-remove="$emit('request-remove', $event)"
                @save="$emit('save', $event)"
                @remove="$emit('remove', $event)"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import type { OverviewEmits } from "@regira/modules/vue/entities"
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
