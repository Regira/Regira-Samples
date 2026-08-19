<script setup lang="ts">
import { computed } from "vue"

const props = defineProps<{ page: number; pageSize: number; count: number }>()
const emit = defineEmits<{ (e: "update:page", page: number): void }>()

const pageCount = computed(() => Math.max(1, Math.ceil(props.count / props.pageSize)))

const pages = computed(() => {
  const total = pageCount.value
  const current = props.page
  const set = new Set<number>([1, total, current, current - 1, current + 1])
  return [...set].filter((p) => p >= 1 && p <= total).sort((a, b) => a - b)
})

function go(p: number) {
  if (p < 1 || p > pageCount.value || p === props.page) return
  emit("update:page", p)
}
</script>

<template>
  <nav v-if="pageCount > 1" class="ws-pagination" aria-label="Pagination">
    <button type="button" class="ws-page-btn" :disabled="page <= 1" @click="go(page - 1)">
      <i class="bi bi-chevron-left"></i>
    </button>
    <template v-for="(p, idx) in pages" :key="p">
      <span v-if="idx > 0 && p - pages[idx - 1] > 1" class="ws-page-gap">&hellip;</span>
      <button type="button" class="ws-page-btn" :class="{ active: p === page }" @click="go(p)">{{ p }}</button>
    </template>
    <button type="button" class="ws-page-btn" :disabled="page >= pageCount" @click="go(page + 1)">
      <i class="bi bi-chevron-right"></i>
    </button>
  </nav>
</template>

<style scoped>
.ws-pagination {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  justify-content: center;
  flex-wrap: wrap;
}
.ws-page-btn {
  min-width: 2.2rem;
  height: 2.2rem;
  padding: 0 0.4rem;
  border-radius: var(--ws-radius-sm);
  border: 1px solid var(--ws-line);
  background: var(--ws-surface);
  color: var(--ws-ink-soft);
  font-weight: 600;
  font-size: 0.86rem;
  cursor: pointer;
}
.ws-page-btn:hover:not(:disabled) {
  border-color: var(--ws-primary);
  color: var(--ws-primary);
}
.ws-page-btn.active {
  background: var(--ws-primary);
  border-color: var(--ws-primary);
  color: #fff;
}
.ws-page-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}
.ws-page-gap {
  color: var(--ws-muted);
  padding: 0 0.15rem;
}
</style>
