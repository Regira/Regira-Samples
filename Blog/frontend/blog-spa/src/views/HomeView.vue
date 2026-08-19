<script setup lang="ts">
import { NavBar, NavSearch, Dashboard } from "@/components/entity-navigation"
</script>
<template>
    <section>
        <div class="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-4">
            <h1 class="mb-0">Admin</h1>
            <NavSearch />
        </div>
        <NavBar class="mb-4" />
        <Dashboard />
    </section>
</template>
