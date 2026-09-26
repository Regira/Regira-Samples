<script setup lang="ts">
import { computed } from "vue"
import type { TextValues } from "../texts"

// Renders a screen text with its {placeholders} filled in, each value emphasised (a bold IQ on the certificate).
const props = withDefaults(defineProps<{ text: string; values?: TextValues; tag?: string }>(), { tag: "strong" })

const segments = computed(() =>
    props.text.split(/(\{\w+\})/).map((part) => {
        const key = /^\{(\w+)\}$/.exec(part)?.[1]
        const value = key != null ? props.values?.[key] : undefined
        return value != null ? { text: String(value), isValue: true } : { text: part, isValue: false }
    })
)
</script>

<template>
    <template v-for="(segment, i) in segments" :key="i">
        <component :is="tag" v-if="segment.isValue">{{ segment.text }}</component>
        <template v-else>{{ segment.text }}</template>
    </template>
</template>
