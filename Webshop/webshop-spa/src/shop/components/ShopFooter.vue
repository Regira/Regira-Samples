<script setup lang="ts">
import { onMounted } from "vue"
import { RouterLink } from "vue-router"
import { useConfig } from "@/app-config"
import { useShopCatalog } from "../catalog"

const { title } = useConfig()
const { categories, load } = useShopCatalog()
onMounted(load)
</script>

<template>
    <footer class="ws-footer">
        <div class="container-xl">
            <div class="row g-4">
                <div class="col-lg-4">
                    <div class="ws-logo ws-logo--light mb-3">
                        <span class="ws-logo__mark"><i class="bi bi-bag-heart-fill"></i></span>
                        <span class="ws-logo__text">{{ $tm(title) }}</span>
                    </div>
                    <p class="text-white-50 mb-3">Thoughtfully made gear for home, work and play. Shipped from Brussels to the Benelux, Germany and France.</p>
                    <div class="d-flex gap-3 fs-5">
                        <i class="bi bi-instagram"></i><i class="bi bi-facebook"></i><i class="bi bi-youtube"></i><i class="bi bi-tiktok"></i>
                    </div>
                </div>
                <div class="col-6 col-lg-3">
                    <h6>Shop</h6>
                    <ul class="list-unstyled">
                        <li v-for="c in categories.slice(0, 6)" :key="c.id">
                            <RouterLink :to="{ name: 'shop', query: { categoryId: c.id } }">{{ c.title }}</RouterLink>
                        </li>
                        <li><RouterLink :to="{ name: 'shop', query: { onSale: 'true' } }">Deals</RouterLink></li>
                    </ul>
                </div>
                <div class="col-6 col-lg-2">
                    <h6>Service</h6>
                    <ul class="list-unstyled">
                        <li><span>Shipping &amp; delivery</span></li>
                        <li><span>30-day returns</span></li>
                        <li><span>Warranty</span></li>
                        <li><RouterLink :to="{ name: 'cart' }">Your cart</RouterLink></li>
                    </ul>
                </div>
                <div class="col-lg-3">
                    <h6>We accept</h6>
                    <div class="ws-paylogos">
                        <span><i class="bi bi-credit-card"></i> Card</span>
                        <span><i class="bi bi-paypal"></i> PayPal</span>
                        <span><i class="bi bi-bank"></i> Transfer</span>
                    </div>
                    <p class="small text-white-50 mt-3 mb-0">Demo store - no real payments are taken and no goods are shipped.</p>
                </div>
            </div>
            <hr class="border-secondary my-4" />
            <div class="d-flex flex-wrap justify-content-between small text-white-50 gap-2">
                <span>&copy; {{ new Date().getFullYear() }} {{ $tm(title) }}. Built on the Regira framework.</span>
                <RouterLink :to="{ name: 'adminHome' }" class="text-white-50">Back office</RouterLink>
            </div>
        </div>
    </footer>
</template>
