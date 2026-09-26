<script setup lang="ts">
import { ref } from "vue"
import { NavBar, NavSearch } from "@/components/entity-navigation"

const open = ref(false)
const closeMenu = () => (open.value = false)
</script>
<template>
    <nav class="navbar navbar-expand-sm" v-click-outside="closeMenu">
        <div class="container-fluid">
            <router-link class="navbar-brand" :to="{ name: 'admin' }">👑 {{ $t("adminTitle") }}</router-link>
            <button class="navbar-toggler" type="button" @click.stop="open = !open"><span class="navbar-toggler-icon"></span></button>
            <div class="collapse navbar-collapse" :class="{ show: open }">
                <NavBar @select="closeMenu" />
                <div class="d-flex ms-auto align-items-center gap-2">
                    <NavSearch @search="closeMenu" />
                    <router-link class="btn btn-warning fw-bold text-nowrap" :to="{ name: 'home' }">🎮 {{ $t("backToGame") }}</router-link>
                </div>
            </div>
        </div>
    </nav>
</template>
