<script setup lang="ts">
const year = new Date().getFullYear()
</script>
<template>
    <div class="d-flex justify-content-between w-100 py-2 text-muted">
        <span>&copy; {{ year }} Nordlys Store - back office</span>
        <router-link :to="{ name: 'home' }">Storefront</router-link>
    </div>
</template>
