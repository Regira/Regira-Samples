<script setup lang="ts">
import { computed, ref } from "vue"
import { useAuthStore, getAccountName } from "@regira/modules/vue/auth"
import { useConfig } from "@/app-config"
import { NavBar, NavSearch } from "@/components/entity-navigation"
import { Roles } from "@/infrastructure/permissions"

const { title } = useConfig()
const open = ref(false)
const closeMenu = () => (open.value = false)
const authStore = useAuthStore()
const logout = () => authStore.logout()
// resolved from $auth (the store the auth plugin was configured with); not every JWT carries a displayName claim
const accountLabel = computed(() => getAccountName())
const roleLabel = computed(() => (authStore.hasRole(Roles.ADMIN) ? Roles.ADMIN : authStore.hasRole(Roles.MANAGER) ? Roles.MANAGER : authStore.isAuthenticated ? "Viewer" : ""))
</script>
<template>
    <nav class="navbar navbar-expand-sm" v-click-outside="closeMenu">
        <div class="container-fluid">
            <router-link class="navbar-brand ah-brand" :to="{ name: 'home' }"><i class="bi bi-box-seam me-1"></i>{{ $tm(title) }}</router-link>
            <button class="navbar-toggler" type="button" @click.stop="open = !open"><span class="navbar-toggler-icon"></span></button>
            <div class="collapse navbar-collapse" :class="{ show: open }">
                <NavBar @select="closeMenu" />
                <div class="d-flex ms-auto align-items-center gap-2">
                    <NavSearch @search="closeMenu" />
                    <router-link v-if="$auth.enabled && $auth.isAuthenticated" class="nav-link" :to="{ name: 'account' }" @click="closeMenu">
                        <!-- the $t fallback guards a token with no name claims at all — never an empty (invisible) link -->
                        <i class="bi bi-person-circle me-1"></i>{{ accountLabel ?? $t("account") }}
                        <span v-if="roleLabel" class="badge rounded-pill text-bg-light ms-1">{{ roleLabel }}</span>
                    </router-link>
                    <button v-if="$auth.enabled && $auth.isAuthenticated" class="btn btn-outline-light btn-sm text-nowrap" @click="logout">
                        {{ $t("signOut") }}
                    </button>
                </div>
            </div>
        </div>
    </nav>
</template>
