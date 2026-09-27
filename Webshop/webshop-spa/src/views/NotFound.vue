<script setup lang="ts">
defineProps<{ url?: string }>()
</script>
<template>
    <section class="container-xl py-5 text-center">
        <h1 class="ws-page-title">404 - page not found</h1>
        <p>
            Page <router-link :to="url ?? '/'">{{ url }}</router-link> was not found.
        </p>
    </section>
</template>
