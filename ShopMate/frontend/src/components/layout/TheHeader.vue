<script setup lang="ts">
import { useConfig } from "@/app-config"

const { title } = useConfig()
</script>
<template>
    <div class="sm-topbar px-3">
        <router-link :to="{ name: 'home' }" class="sm-topbar__title text-decoration-none d-flex align-items-center gap-2">
            <i class="bi bi-cart4"></i>
            <span>{{ $tm(title) }}</span>
        </router-link>
    </div>
</template>
