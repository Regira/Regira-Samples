<script setup lang="ts"></script>

<template>
  <section class="ws-container ws-promo-duo">
    <RouterLink :to="{ name: 'shop', query: { onSale: '1' } }" class="ws-promo ws-promo-sale">
      <div class="ws-promo-copy">
        <span class="ws-eyebrow" style="color: #fff">Limited time</span>
        <h3 class="display-font">Up to 40% off select items</h3>
        <span class="ws-promo-cta">Shop the sale <i class="bi bi-arrow-right"></i></span>
      </div>
      <img src="https://picsum.photos/seed/webshop-promo-sale/360/300" alt="Sale" />
    </RouterLink>
    <RouterLink :to="{ name: 'shop', query: { featured: '1' } }" class="ws-promo ws-promo-new">
      <div class="ws-promo-copy">
        <span class="ws-eyebrow">Just landed</span>
        <h3 class="display-font">Featured picks of the week</h3>
        <span class="ws-promo-cta">Explore featured <i class="bi bi-arrow-right"></i></span>
      </div>
      <img src="https://picsum.photos/seed/webshop-promo-new/360/300" alt="Featured" />
    </RouterLink>
  </section>
</template>

<style scoped>
.ws-promo-duo {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.25rem;
  padding: 2.5rem 0;
}
@media (max-width: 768px) {
  .ws-promo-duo {
    grid-template-columns: 1fr;
  }
}
.ws-promo {
  position: relative;
  border-radius: var(--ws-radius-lg);
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: space-between;
  min-height: 190px;
  padding: 1.8rem 2rem;
  color: #fff;
}
.ws-promo-sale {
  background: linear-gradient(120deg, #b91c2e 0%, #e0463f 100%);
}
.ws-promo-new {
  background: linear-gradient(120deg, var(--ws-primary-dark) 0%, var(--ws-primary) 100%);
}
.ws-promo-copy {
  max-width: 60%;
  position: relative;
  z-index: 1;
}
.ws-promo h3 {
  color: #fff;
  font-size: 1.4rem;
  margin: 0.4rem 0 0.9rem;
  line-height: 1.2;
}
.ws-promo-cta {
  font-weight: 700;
  font-size: 0.88rem;
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
}
.ws-promo img {
  position: absolute;
  right: -1rem;
  bottom: -1rem;
  width: 55%;
  height: 130%;
  object-fit: cover;
  border-radius: 50%;
  opacity: 0.35;
  mix-blend-mode: luminosity;
}
@media (max-width: 500px) {
  .ws-promo-copy {
    max-width: 100%;
  }
  .ws-promo img {
    opacity: 0.2;
  }
}
</style>
