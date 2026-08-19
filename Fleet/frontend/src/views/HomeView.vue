<script setup lang="ts">
import { useConfig } from "@/app-config"
import { Dashboard } from "@/components/entity-navigation"
import FleetDashboard from "@/components/dashboard/FleetDashboard.vue"
const { title } = useConfig()
</script>
<template>
    <section>
        <h1 class="mb-4">{{ $tm(title) }}</h1>
        <FleetDashboard />
        <h5 class="mb-3">{{ $t("quickLinks") }}</h5>
        <Dashboard />
    </section>
</template>
