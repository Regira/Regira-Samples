<script setup lang="ts">
const year = new Date().getFullYear()
</script>
<template>
    <div class="d-flex justify-content-between w-100 py-2 small text-secondary">
        <span>&copy; {{ year }} FleetOps &middot; built on Regira</span>
        <span class="d-none d-sm-inline">Fleet intervention management</span>
    </div>
</template>
