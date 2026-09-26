<script setup lang="ts">
import { Dashboard } from "@/components/entity-navigation"
import ResetContent from "@/components/ResetContent.vue"
</script>
<template>
    <section>
        <h1 class="admin-title text-center my-4">👑 {{ $t("adminTitle") }} 👑</h1>
        <Dashboard />
        <ResetContent />
    </section>
</template>
