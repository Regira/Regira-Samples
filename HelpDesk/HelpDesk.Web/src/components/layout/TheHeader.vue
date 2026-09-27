<script setup lang="ts">
import { computed } from "vue"
import { useAuthStore, getAccountName } from "@regira/modules/vue/auth"
import { Icon } from "@regira/modules/vue/ui"
import { useConfig } from "@/app-config"
import { NavSearch } from "@/components/entity-navigation"
import { useMeStore } from "@/infrastructure/me"
import { useAccess } from "@/infrastructure/access"

defineEmits<{ (e: "toggle-sidebar"): void }>()

const { title } = useConfig()
const authStore = useAuthStore()
const meStore = useMeStore()
const { isAdmin, isStaff } = useAccess()
const logout = () => authStore.logout()
const accountLabel = computed(() => meStore.me?.person?.fullName ?? getAccountName())
const roleLabel = computed(() => (isAdmin.value ? "roleAdmin" : isStaff.value ? "roleAgent" : "roleCustomer"))
</script>

<template>
    <nav class="d-flex align-items-center gap-2 px-3 py-2">
        <button v-if="$auth.enabled && $auth.isAuthenticated" class="btn btn-link text-body d-lg-none p-1" type="button" :aria-label="$t('menu')" @click="$emit('toggle-sidebar')">
            <i class="bi bi-list fs-4"></i>
        </button>
        <router-link class="hd-brand d-lg-none text-decoration-none" :to="{ name: 'home' }">
            <i class="bi bi-life-preserver me-1"></i>{{ $tm(title) }}
        </router-link>
        <div class="flex-grow-1 d-none d-md-block hd-search">
            <NavSearch v-if="$auth.enabled && $auth.isAuthenticated" />
        </div>
        <div v-if="$auth.enabled && $auth.isAuthenticated" class="d-flex align-items-center gap-2 ms-auto">
            <router-link class="d-flex align-items-center gap-2 text-decoration-none text-body" :to="{ name: 'account' }">
                <span class="hd-avatar">{{ (accountLabel ?? "?").substring(0, 1).toUpperCase() }}</span>
                <span class="d-none d-sm-flex flex-column lh-sm">
                    <span class="fw-semibold small">{{ accountLabel ?? $t("account") }}</span>
                    <span class="text-muted" style="font-size: 0.75rem">{{ $t(roleLabel) }}</span>
                </span>
            </router-link>
            <button class="btn btn-outline-secondary btn-sm" :title="$t('signOut')" @click="logout">
                <Icon name="exit" /><span class="d-none d-md-inline ms-1">{{ $t("signOut") }}</span>
            </button>
        </div>
    </nav>
</template>
