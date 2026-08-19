<script setup lang="ts">
import { useConfig } from "@/app-config"
import { Dashboard } from "@/components/entity-navigation"
const { title } = useConfig()
</script>
<template>
    <section>
        <h1 class="text-center my-4">{{ $tm(title) }}</h1>
        <p class="text-center text-muted mb-4">Manage employee training credits - 1 QCredit = half a working day / EUR 250</p>

        <div class="row justify-content-center mb-4">
            <div class="col-auto">
                <router-link :to="{ name: 'balances' }" class="btn btn-outline-primary btn-lg">
                    <i class="bi bi-bar-chart-line me-1"></i> {{ $t("balances") }}
                </router-link>
            </div>
        </div>

        <Dashboard />
    </section>
</template>
