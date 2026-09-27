<script setup lang="ts">
import { computed } from "vue"
// A lookup rendered as a coloured pill (status, priority, team, category). Colours come from the lookup row.
const props = defineProps<{ title?: string; color?: string; icon?: string; solid?: boolean }>()
const style = computed(() => {
    const c = props.color || "#64748b"
    return props.solid ? { backgroundColor: c, color: "#fff" } : { backgroundColor: c + "1a", color: c, borderColor: c + "40" }
})
</script>
<template>
    <span v-if="title" class="hd-badge" :style="style">
        <i v-if="icon" :class="icon"></i><span v-else class="hd-dot" :style="{ backgroundColor: solid ? '#fff' : color || '#64748b' }"></span>{{ title }}
    </span>
</template>
