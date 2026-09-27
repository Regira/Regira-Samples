<script setup lang="ts">
import { useConfig } from "@/app-config"
const { title } = useConfig()
const year = new Date().getFullYear()
</script>
<template>
    <div class="site-footer">
        <div class="container-xl d-flex flex-wrap justify-content-between gap-2 py-4">
            <span class="site-footer__brand">{{ $tm(title) }}</span>
            <span class="text-muted small">&copy; {{ year }} &middot; {{ $t("builtWith") }}</span>
        </div>
    </div>
</template>
