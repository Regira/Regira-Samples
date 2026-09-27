<script setup lang="ts">
import { NavSearch } from "@/components/entity-navigation"
import { culture } from "@/infrastructure/domain"

defineEmits<{ (e: "toggle-menu"): void }>()
const today = new Date().toLocaleDateString(culture, { weekday: "long", day: "numeric", month: "long", year: "numeric" })
</script>

<template>
    <div class="fleet-topbar">
        <button class="btn btn-outline-secondary d-lg-none" type="button" aria-label="Menu" @click="$emit('toggle-menu')">
            <i class="bi bi-list"></i>
        </button>
        <div class="fleet-topbar__search">
            <NavSearch />
        </div>
        <div class="ms-auto d-none d-md-flex align-items-center gap-3 text-secondary small">
            <span><i class="bi bi-calendar3 me-1"></i>{{ today }}</span>
            <span class="fleet-avatar" title="Fleet manager"><i class="bi bi-person-fill"></i></span>
        </div>
    </div>
</template>
