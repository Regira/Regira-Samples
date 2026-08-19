<script setup lang="ts">
import { computed } from "vue"

const props = defineProps<{ rating: number; reviewCount?: number; size?: "sm" | "md" }>()

const stars = computed(() => {
  const r = Math.max(0, Math.min(5, props.rating))
  return Array.from({ length: 5 }, (_, i) => {
    const diff = r - i
    if (diff >= 0.75) return "full"
    if (diff >= 0.25) return "half"
    return "empty"
  })
})
</script>

<template>
  <span class="ws-rating" :class="props.size === 'sm' ? 'ws-rating-sm' : ''">
    <span class="ws-rating-stars">
      <i
        v-for="(s, i) in stars"
        :key="i"
        class="bi"
        :class="s === 'full' ? 'bi-star-fill' : s === 'half' ? 'bi-star-half' : 'bi-star'"
      ></i>
    </span>
    <span v-if="props.reviewCount !== undefined" class="ws-rating-count">({{ props.reviewCount }})</span>
  </span>
</template>

<style scoped>
.ws-rating {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
}
.ws-rating-stars {
  display: inline-flex;
  gap: 1px;
  color: var(--ws-accent);
  font-size: 0.9rem;
}
.ws-rating-sm .ws-rating-stars {
  font-size: 0.76rem;
}
.ws-rating-count {
  font-size: 0.78rem;
  color: var(--ws-muted);
}
</style>
