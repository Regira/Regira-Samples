<script setup lang="ts">
import { computed } from "vue"

const props = defineProps<{ price: number; compareAtPrice?: number | null; size?: "sm" | "md" | "lg" }>()

const onSale = computed(() => !!props.compareAtPrice && props.compareAtPrice > props.price)
const discountPct = computed(() => (onSale.value ? Math.round((1 - props.price / (props.compareAtPrice as number)) * 100) : 0))

function fmt(v: number) {
  return new Intl.NumberFormat("en-IE", { style: "currency", currency: "EUR" }).format(v)
}
</script>

<template>
  <span class="ws-price" :class="`ws-price-${props.size ?? 'md'}`">
    <span class="ws-price-now">{{ fmt(props.price) }}</span>
    <span v-if="onSale" class="ws-price-was">{{ fmt(props.compareAtPrice as number) }}</span>
    <span v-if="onSale" class="ws-price-pct">-{{ discountPct }}%</span>
  </span>
</template>

<style scoped>
.ws-price {
  display: inline-flex;
  align-items: baseline;
  gap: 0.5rem;
  flex-wrap: wrap;
}
.ws-price-now {
  font-weight: 800;
  color: var(--ws-ink);
}
.ws-price-md .ws-price-now {
  font-size: 1.05rem;
}
.ws-price-lg .ws-price-now {
  font-size: 1.7rem;
}
.ws-price-sm .ws-price-now {
  font-size: 0.92rem;
}
.ws-price-was {
  text-decoration: line-through;
  color: var(--ws-muted);
  font-size: 0.85em;
}
.ws-price-pct {
  color: var(--ws-sale);
  font-weight: 700;
  font-size: 0.78em;
}
</style>
