<script setup lang="ts">
import { useToastStore } from "@/stores/toast"

const toast = useToastStore()
</script>

<template>
  <div class="ws-toasts">
    <TransitionGroup name="toast">
      <div v-for="m in toast.messages" :key="m.id" class="ws-toast" :class="`ws-toast-${m.variant}`">
        <i class="bi" :class="m.variant === 'success' ? 'bi-check-circle-fill' : m.variant === 'error' ? 'bi-exclamation-triangle-fill' : 'bi-info-circle-fill'"></i>
        <span>{{ m.text }}</span>
        <button type="button" class="ws-toast-close" @click="toast.dismiss(m.id)" aria-label="Dismiss">
          <i class="bi bi-x"></i>
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.ws-toasts {
  position: fixed;
  right: 1rem;
  bottom: 1rem;
  z-index: 2000;
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
  max-width: min(360px, calc(100vw - 2rem));
}
.ws-toast {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  background: var(--ws-ink);
  color: #fff;
  padding: 0.75rem 0.9rem;
  border-radius: var(--ws-radius-sm);
  box-shadow: var(--ws-shadow-lg);
  font-size: 0.88rem;
}
.ws-toast-success {
  background: var(--ws-success);
}
.ws-toast-error {
  background: var(--ws-sale);
}
.ws-toast span {
  flex: 1 1 auto;
}
.ws-toast-close {
  background: transparent;
  border: none;
  color: inherit;
  opacity: 0.75;
  cursor: pointer;
  line-height: 1;
  padding: 0;
}
.ws-toast-close:hover {
  opacity: 1;
}
.toast-enter-active,
.toast-leave-active {
  transition: all 0.2s ease;
}
.toast-enter-from {
  opacity: 0;
  transform: translateY(8px);
}
.toast-leave-to {
  opacity: 0;
  transform: translateX(12px);
}
</style>
