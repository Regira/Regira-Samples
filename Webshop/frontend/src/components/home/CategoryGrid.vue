<script setup lang="ts">
import type { CategoryDto } from "@/types/models"

defineProps<{ categories: CategoryDto[] }>()
</script>

<template>
  <section class="ws-container ws-cat-section">
    <div class="ws-section-head">
      <div>
        <span class="ws-eyebrow">Browse</span>
        <h2 class="display-font">Shop by category</h2>
      </div>
      <RouterLink :to="{ name: 'shop' }" class="ws-see-all">See all products <i class="bi bi-arrow-right"></i></RouterLink>
    </div>
    <div class="ws-cat-grid">
      <RouterLink
        v-for="cat in categories"
        :key="cat.id"
        :to="{ name: 'shop-category', params: { categorySlug: cat.slug ?? String(cat.id) } }"
        class="ws-cat-tile"
      >
        <img :src="cat.imageUrl ?? ''" :alt="cat.title" loading="lazy" />
        <div class="ws-cat-overlay">
          <span class="ws-cat-title">{{ cat.title }}</span>
          <span v-if="cat.productCount" class="ws-cat-count">{{ cat.productCount }} products</span>
        </div>
      </RouterLink>
    </div>
  </section>
</template>

<style scoped>
.ws-cat-section {
  padding: 3.5rem 0 1rem;
}
.ws-section-head {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  margin-bottom: 1.6rem;
  gap: 1rem;
  flex-wrap: wrap;
}
.ws-section-head h2 {
  margin: 0.25rem 0 0;
  font-size: 1.7rem;
}
.ws-see-all {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--ws-primary);
  white-space: nowrap;
}
.ws-cat-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 1rem;
}
@media (max-width: 992px) {
  .ws-cat-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}
@media (max-width: 560px) {
  .ws-cat-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
.ws-cat-tile {
  position: relative;
  aspect-ratio: 1/1;
  border-radius: var(--ws-radius);
  overflow: hidden;
  box-shadow: var(--ws-shadow-sm);
}
.ws-cat-tile img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s ease;
}
.ws-cat-tile:hover img {
  transform: scale(1.07);
}
.ws-cat-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(180deg, rgba(23, 23, 28, 0) 40%, rgba(23, 23, 28, 0.78) 100%);
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  padding: 0.8rem;
  color: #fff;
}
.ws-cat-title {
  font-weight: 700;
  font-size: 0.92rem;
}
.ws-cat-count {
  font-size: 0.72rem;
  opacity: 0.85;
}
</style>
