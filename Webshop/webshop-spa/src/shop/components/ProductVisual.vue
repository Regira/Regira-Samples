<!-- Product image: the product's own imageUrl when set, otherwise a generated visual in the category's colour -->
<script setup lang="ts">
import { computed, ref } from "vue"

const props = withDefaults(
    defineProps<{ seed?: number; icon?: string; color?: string; imageUrl?: string; label?: string; size?: "sm" | "md" | "lg" }>(),
    { size: "md", color: "#6366f1", icon: "bag" }
)
const failed = ref(false)
const showImage = computed(() => !!props.imageUrl && !failed.value)
// small, deterministic variation per product so a grid of one category doesn't look identical
const angle = computed(() => 120 + ((props.seed ?? 0) * 37) % 100)
const hue = computed(() => (((props.seed ?? 0) * 53) % 50) - 25)
</script>

<template>
    <div class="ws-visual" :class="`ws-visual--${size}`">
        <img v-if="showImage" :src="imageUrl" :alt="label" loading="lazy" @error="failed = true" />
        <div
            v-else
            class="ws-visual__art"
            :style="{
                background: `linear-gradient(${angle}deg, ${color} 0%, color-mix(in srgb, ${color} 55%, #0f172a) 100%)`,
                filter: `hue-rotate(${hue}deg)`,
            }"
        >
            <span class="ws-visual__ring"></span>
            <i :class="`bi bi-${icon}`" class="ws-visual__icon"></i>
            <span v-if="label && size !== 'sm'" class="ws-visual__label">{{ label }}</span>
        </div>
        <slot />
    </div>
</template>
