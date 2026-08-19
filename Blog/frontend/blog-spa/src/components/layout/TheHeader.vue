<script setup lang="ts">
import { useConfig } from "@/app-config"

const { title } = useConfig()
</script>
<template>
    <nav class="navbar navbar-expand-sm site-header">
        <div class="container-fluid">
            <router-link class="navbar-brand site-brand" :to="{ name: 'blogHome' }">{{ $tm(title) }}</router-link>
            <div class="d-flex ms-auto align-items-center gap-3">
                <router-link class="nav-link" :to="{ name: 'blogHome' }">Home</router-link>
                <router-link class="nav-link" :to="{ name: 'home' }"><i class="bi bi-gear me-1"></i>Admin</router-link>
            </div>
        </div>
    </nav>
</template>
