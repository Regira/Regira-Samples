<script setup lang="ts">
const items = [
  { icon: "bi-truck", title: "Free shipping", text: "On orders over €75" },
  { icon: "bi-shield-check", title: "Secure checkout", text: "Encrypted & protected" },
  { icon: "bi-arrow-repeat", title: "Easy returns", text: "30-day return window" },
  { icon: "bi-headset", title: "24/7 support", text: "We are here to help" },
]
</script>

<template>
  <section class="ws-container ws-usp">
    <div v-for="item in items" :key="item.title" class="ws-usp-item">
      <i class="bi" :class="item.icon"></i>
      <div>
        <strong>{{ item.title }}</strong>
        <span>{{ item.text }}</span>
      </div>
    </div>
  </section>
</template>

<style scoped>
.ws-usp {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
  padding: 0.5rem 0 1rem;
}
@media (max-width: 768px) {
  .ws-usp {
    grid-template-columns: repeat(2, 1fr);
  }
}
.ws-usp-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  background: var(--ws-surface);
  border: 1px solid var(--ws-line);
  border-radius: var(--ws-radius);
  padding: 1rem 1.1rem;
}
.ws-usp-item i {
  font-size: 1.4rem;
  color: var(--ws-primary);
}
.ws-usp-item div {
  display: flex;
  flex-direction: column;
  line-height: 1.3;
}
.ws-usp-item strong {
  font-size: 0.88rem;
}
.ws-usp-item span {
  font-size: 0.76rem;
  color: var(--ws-muted);
}
</style>
