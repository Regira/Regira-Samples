<script setup lang="ts">
import { ref } from "vue"
import { useConfig } from "@/app-config"
import { NavBar, NavSearch } from "@/components/entity-navigation"

const { adminTitle } = useConfig()
const open = ref(false)
const closeMenu = () => (open.value = false)
</script>
<template>
    <nav class="navbar navbar-expand-sm" v-click-outside="closeMenu">
        <div class="container-fluid">
            <router-link class="navbar-brand fw-semibold" :to="{ name: 'adminHome' }"><i class="bi bi-speedometer2 me-2"></i>{{ $tm(adminTitle) }}</router-link>
            <button class="navbar-toggler" type="button" @click.stop="open = !open"><span class="navbar-toggler-icon"></span></button>
            <div class="collapse navbar-collapse" :class="{ show: open }">
                <NavBar @select="closeMenu" />
                <div class="d-flex ms-auto align-items-center gap-2">
                    <NavSearch @search="closeMenu" />
                    <router-link :to="{ name: 'home' }" class="btn btn-outline-primary btn-sm text-nowrap"><i class="bi bi-shop me-1"></i>View store</router-link>
                </div>
            </div>
        </div>
    </nav>
</template>
