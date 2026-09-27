<template>
    <img v-if="speaker?.photoUrl" :src="speaker.photoUrl" :alt="speaker.$title" class="ep-avatar" :class="`ep-avatar-${size}`" />
    <span v-else class="ep-avatar" :class="`ep-avatar-${size}`" :style="{ background: speaker?.$avatarColor }" :title="speaker?.$title">
        {{ speaker?.$initials }}
    </span>
</template>

<script setup lang="ts">
import type Entity from "../data/Entity"

withDefaults(defineProps<{ speaker?: Entity; size?: "sm" | "md" | "lg" }>(), { size: "md" })
</script>
