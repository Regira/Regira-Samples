<script setup lang="ts">
import { ref } from "vue"
import { useRouter } from "vue-router"
import { useCartStore } from "@/stores/cart"

const router = useRouter()
const cart = useCartStore()
const q = ref("")
const menuOpen = ref(false)

function submitSearch() {
  router.push({ name: "shop", query: q.value ? { q: q.value } : {} })
  menuOpen.value = false
}
</script>

<template>
  <div class="ws-announce">
    <div class="ws-container ws-announce-inner">
      <span><i class="bi bi-truck me-1"></i> Free shipping on orders over &euro;75</span>
      <span class="d-none d-md-inline"><i class="bi bi-arrow-repeat me-1"></i> 30-day easy returns</span>
    </div>
  </div>

  <header class="ws-header">
    <div class="ws-container ws-header-inner">
      <button class="ws-burger d-lg-none" type="button" aria-label="Toggle menu" @click="menuOpen = !menuOpen">
        <i class="bi" :class="menuOpen ? 'bi-x-lg' : 'bi-list'"></i>
      </button>

      <RouterLink :to="{ name: 'home' }" class="ws-logo">
        <span class="ws-logo-mark"><i class="bi bi-bag-heart-fill"></i></span>
        <span class="ws-logo-text">Northwind<em>&amp;Co.</em></span>
      </RouterLink>

      <nav class="ws-nav d-none d-lg-flex">
        <RouterLink :to="{ name: 'home' }">Home</RouterLink>
        <RouterLink :to="{ name: 'shop' }">Shop</RouterLink>
        <RouterLink :to="{ name: 'shop', query: { featured: '1' } }">Featured</RouterLink>
        <RouterLink :to="{ name: 'shop', query: { onSale: '1' } }">Sale</RouterLink>
      </nav>

      <form class="ws-search d-none d-md-flex" @submit.prevent="submitSearch">
        <i class="bi bi-search"></i>
        <input v-model="q" type="search" placeholder="Search products, brands..." />
      </form>

      <div class="ws-header-actions">
        <RouterLink :to="{ name: 'cart' }" class="ws-cart-btn" aria-label="Cart">
          <i class="bi bi-bag"></i>
          <span v-if="cart.itemCount > 0" class="ws-cart-badge">{{ cart.itemCount }}</span>
        </RouterLink>
      </div>
    </div>

    <Transition name="drop">
      <div v-if="menuOpen" class="ws-mobile-panel d-lg-none">
        <form class="ws-search ws-search-mobile" @submit.prevent="submitSearch">
          <i class="bi bi-search"></i>
          <input v-model="q" type="search" placeholder="Search products..." />
        </form>
        <RouterLink :to="{ name: 'home' }" @click="menuOpen = false">Home</RouterLink>
        <RouterLink :to="{ name: 'shop' }" @click="menuOpen = false">Shop</RouterLink>
        <RouterLink :to="{ name: 'shop', query: { featured: '1' } }" @click="menuOpen = false">Featured</RouterLink>
        <RouterLink :to="{ name: 'shop', query: { onSale: '1' } }" @click="menuOpen = false">Sale</RouterLink>
      </div>
    </Transition>
  </header>
</template>

<style scoped>
.ws-announce {
  background: var(--ws-ink);
  color: #f2f0fa;
  font-size: 0.78rem;
}
.ws-announce-inner {
  display: flex;
  gap: 1.5rem;
  justify-content: center;
  padding: 0.4rem 1.5rem;
}

.ws-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid var(--ws-line);
}
.ws-header-inner {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  padding: 0.85rem 1.5rem;
}
.ws-burger {
  border: none;
  background: transparent;
  font-size: 1.3rem;
  color: var(--ws-ink);
  padding: 0.2rem 0.3rem;
  cursor: pointer;
}
.ws-logo {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 800;
  font-size: 1.2rem;
  color: var(--ws-ink);
  flex-shrink: 0;
}
.ws-logo-mark {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.2rem;
  height: 2.2rem;
  border-radius: 50%;
  background: var(--ws-primary);
  color: #fff;
  font-size: 1.05rem;
}
.ws-logo-text em {
  font-style: normal;
  color: var(--ws-primary);
}
.ws-nav {
  display: flex;
  gap: 1.4rem;
  font-weight: 600;
  font-size: 0.92rem;
  color: var(--ws-ink-soft);
}
.ws-nav a.router-link-active {
  color: var(--ws-primary);
}
.ws-nav a:hover {
  color: var(--ws-primary);
}
.ws-search {
  flex: 1 1 auto;
  max-width: 380px;
  margin-left: auto;
  align-items: center;
  gap: 0.5rem;
  background: var(--ws-bg-alt);
  border-radius: 999px;
  padding: 0.5rem 1rem;
  color: var(--ws-muted);
}
.ws-search input {
  border: none;
  background: transparent;
  outline: none;
  flex: 1 1 auto;
  font-size: 0.88rem;
  color: var(--ws-ink);
}
.ws-header-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-left: auto;
}
.ws-search:not(.ws-search-mobile) + .ws-header-actions {
  margin-left: 0;
}
.ws-cart-btn {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 2.6rem;
  height: 2.6rem;
  border-radius: 50%;
  background: var(--ws-bg-alt);
  color: var(--ws-ink);
  font-size: 1.15rem;
}
.ws-cart-btn:hover {
  background: var(--ws-primary);
  color: #fff;
}
.ws-cart-badge {
  position: absolute;
  top: -2px;
  right: -2px;
  background: var(--ws-accent);
  color: #241900;
  font-size: 0.68rem;
  font-weight: 800;
  min-width: 1.2rem;
  height: 1.2rem;
  border-radius: 999px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 0.25rem;
}

.ws-mobile-panel {
  display: flex;
  flex-direction: column;
  gap: 0.9rem;
  padding: 1rem 1.5rem 1.4rem;
  border-top: 1px solid var(--ws-line);
  font-weight: 600;
}
.ws-search-mobile {
  display: flex;
  max-width: none;
  margin: 0 0 0.3rem;
}
.drop-enter-active,
.drop-leave-active {
  transition: all 0.15s ease;
}
.drop-enter-from,
.drop-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}
</style>
