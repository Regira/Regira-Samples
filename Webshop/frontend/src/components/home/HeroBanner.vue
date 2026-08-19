<script setup lang="ts"></script>

<template>
  <section class="ws-hero">
    <div class="ws-container ws-hero-inner">
      <div class="ws-hero-copy">
        <span class="ws-eyebrow">New season, new favourites</span>
        <h1 class="display-font">Everyday essentials,<br />thoughtfully curated.</h1>
        <p class="ws-hero-sub">
          Over 500 products across electronics, fashion, home and more &mdash; handpicked for quality, priced
          fairly, delivered fast.
        </p>
        <div class="ws-hero-actions">
          <RouterLink :to="{ name: 'shop' }" class="btn-ws btn-ws-primary">
            Shop the collection <i class="bi bi-arrow-right"></i>
          </RouterLink>
          <RouterLink :to="{ name: 'shop', query: { onSale: '1' } }" class="btn-ws btn-ws-outline"> View sale </RouterLink>
        </div>
      </div>
      <div class="ws-hero-art">
        <div class="ws-hero-card ws-hero-card-a">
          <img src="https://picsum.photos/seed/webshop-hero-a/420/420" alt="Featured product" />
        </div>
        <div class="ws-hero-card ws-hero-card-b">
          <img src="https://picsum.photos/seed/webshop-hero-b/260/260" alt="Featured product" />
        </div>
        <div class="ws-hero-badge">
          <i class="bi bi-stars"></i>
          <div>
            <strong>4.8/5</strong>
            <span>from 12k+ reviews</span>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.ws-hero {
  background: radial-gradient(120% 140% at 100% 0%, #eee9ff 0%, var(--ws-bg) 55%);
  padding: 3.5rem 0 4.5rem;
}
.ws-hero-inner {
  display: grid;
  grid-template-columns: 1.1fr 1fr;
  gap: 2.5rem;
  align-items: center;
}
@media (max-width: 900px) {
  .ws-hero-inner {
    grid-template-columns: 1fr;
  }
}
.ws-hero-copy h1 {
  font-size: clamp(2.1rem, 4vw, 3.1rem);
  line-height: 1.08;
  margin: 0.6rem 0 1rem;
  color: var(--ws-ink);
}
.ws-hero-sub {
  color: var(--ws-ink-soft);
  font-size: 1.02rem;
  max-width: 46ch;
  margin-bottom: 1.8rem;
}
.ws-hero-actions {
  display: flex;
  gap: 0.9rem;
  flex-wrap: wrap;
}
.ws-hero-art {
  position: relative;
  min-height: 320px;
}
.ws-hero-card {
  position: absolute;
  border-radius: var(--ws-radius-lg);
  overflow: hidden;
  box-shadow: var(--ws-shadow-lg);
  border: 6px solid #fff;
}
.ws-hero-card img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.ws-hero-card-a {
  width: min(340px, 70%);
  aspect-ratio: 1/1;
  right: 0;
  top: 0;
}
.ws-hero-card-b {
  width: min(190px, 45%);
  aspect-ratio: 1/1;
  left: 0;
  bottom: 0;
}
.ws-hero-badge {
  position: absolute;
  left: 8%;
  top: 6%;
  background: #fff;
  border-radius: var(--ws-radius);
  box-shadow: var(--ws-shadow);
  padding: 0.7rem 0.9rem;
  display: flex;
  align-items: center;
  gap: 0.6rem;
  font-size: 0.82rem;
}
.ws-hero-badge i {
  color: var(--ws-accent);
  font-size: 1.3rem;
}
.ws-hero-badge div {
  display: flex;
  flex-direction: column;
  line-height: 1.2;
}
.ws-hero-badge span {
  color: var(--ws-muted);
  font-size: 0.74rem;
}
@media (max-width: 500px) {
  .ws-hero-badge {
    display: none;
  }
}
</style>
