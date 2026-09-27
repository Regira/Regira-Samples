<script setup lang="ts">
import { useConfig } from "@/app-config"
import { Dashboard } from "@/components/entity-navigation"
const { adminTitle } = useConfig()
</script>
<template>
    <section>
        <h1 class="text-center my-4">{{ $tm(adminTitle) }}</h1>
        <p class="text-center text-body-secondary mb-4">Manage orders, the catalog and storefront promotions.</p>
        <Dashboard />
    </section>
</template>
