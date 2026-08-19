import { createRouter, createWebHistory } from "vue-router"

const router = createRouter({
  history: createWebHistory(),
  scrollBehavior(to, _from, savedPosition) {
    if (savedPosition) return savedPosition
    if (to.hash) return { el: to.hash, behavior: "smooth" }
    return { top: 0 }
  },
  routes: [
    { path: "/", name: "home", component: () => import("@/views/HomeView.vue") },
    { path: "/shop", name: "shop", component: () => import("@/views/ShopView.vue") },
    { path: "/shop/:categorySlug", name: "shop-category", component: () => import("@/views/ShopView.vue") },
    { path: "/products/:slug", name: "product-detail", component: () => import("@/views/ProductDetailView.vue") },
    { path: "/cart", name: "cart", component: () => import("@/views/CartView.vue") },
    { path: "/checkout", name: "checkout", component: () => import("@/views/CheckoutView.vue") },
    { path: "/order-confirmation/:id", name: "order-confirmation", component: () => import("@/views/OrderConfirmationView.vue") },
    { path: "/:pathMatch(.*)*", name: "not-found", component: () => import("@/views/NotFoundView.vue") },
  ],
})

export default router
