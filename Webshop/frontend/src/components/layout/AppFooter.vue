<script setup lang="ts">
const year = new Date().getFullYear()
</script>

<template>
  <footer class="ws-footer">
    <div class="ws-container ws-footer-grid">
      <div class="ws-footer-brand">
        <div class="ws-logo">
          <span class="ws-logo-mark"><i class="bi bi-bag-heart-fill"></i></span>
          <span class="ws-logo-text">Northwind<em>&amp;Co.</em></span>
        </div>
        <p class="ws-muted">
          A demo webshop built on the Regira framework &mdash; curated goods across electronics, fashion, home
          and more.
        </p>
      </div>
      <div>
        <h6>Shop</h6>
        <ul>
          <li><RouterLink :to="{ name: 'shop' }">All products</RouterLink></li>
          <li><RouterLink :to="{ name: 'shop', query: { featured: '1' } }">Featured</RouterLink></li>
          <li><RouterLink :to="{ name: 'shop', query: { onSale: '1' } }">Sale</RouterLink></li>
        </ul>
      </div>
      <div>
        <h6>Support</h6>
        <ul>
          <li><a href="#">Shipping &amp; returns</a></li>
          <li><a href="#">Contact us</a></li>
          <li><a href="#">FAQ</a></li>
        </ul>
      </div>
      <div>
        <h6>Newsletter</h6>
        <p class="ws-muted">Get 10% off your first order.</p>
        <form class="ws-newsletter" @submit.prevent>
          <input type="email" placeholder="you@example.com" class="ws-input" />
          <button type="submit" class="btn-ws btn-ws-primary btn-ws-sm">Join</button>
        </form>
      </div>
    </div>
    <div class="ws-container ws-footer-bottom">
      <span>&copy; {{ year }} Northwind &amp; Co. Demo webshop &mdash; not a real store.</span>
    </div>
  </footer>
</template>

<style scoped>
.ws-footer {
  background: var(--ws-ink);
  color: #cfcfe0;
  margin-top: 4rem;
  padding-top: 3rem;
}
.ws-footer-grid {
  display: grid;
  grid-template-columns: 1.6fr 1fr 1fr 1.2fr;
  gap: 2rem;
  padding-bottom: 2.5rem;
}
@media (max-width: 768px) {
  .ws-footer-grid {
    grid-template-columns: 1fr 1fr;
  }
}
.ws-footer h6 {
  color: #fff;
  font-weight: 700;
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin-bottom: 0.9rem;
}
.ws-footer ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.55rem;
  font-size: 0.9rem;
}
.ws-footer a:hover {
  color: #fff;
}
.ws-footer .ws-logo {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 800;
  font-size: 1.1rem;
  color: #fff;
  margin-bottom: 0.75rem;
}
.ws-logo-mark {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.1rem;
  height: 2.1rem;
  border-radius: 50%;
  background: var(--ws-primary-light);
  color: #fff;
}
.ws-logo-text em {
  font-style: normal;
  color: var(--ws-accent);
}
.ws-footer p {
  font-size: 0.86rem;
  max-width: 32ch;
}
.ws-newsletter {
  display: flex;
  gap: 0.5rem;
}
.ws-newsletter .ws-input {
  background: rgba(255, 255, 255, 0.06);
  border-color: rgba(255, 255, 255, 0.14);
  color: #fff;
}
.ws-newsletter .ws-input::placeholder {
  color: #9c9cae;
}
.ws-footer-bottom {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  padding: 1.1rem 1.5rem;
  font-size: 0.78rem;
  color: #9c9cae;
}
</style>
