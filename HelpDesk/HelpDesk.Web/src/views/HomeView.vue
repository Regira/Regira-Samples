<script setup lang="ts">
import { useAccess } from "@/infrastructure/access"
import StaffDashboard from "@/components/dashboard/StaffDashboard.vue"
import CustomerHome from "@/components/dashboard/CustomerHome.vue"

// one landing route, two audiences: the support center KPIs for staff, a self-service portal for customers
const { isStaff } = useAccess()
</script>
<template>
    <StaffDashboard v-if="isStaff" />
    <CustomerHome v-else-if="$auth.enabled && $auth.isAuthenticated" />
</template>
