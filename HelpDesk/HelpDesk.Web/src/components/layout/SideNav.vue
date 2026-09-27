<script setup lang="ts">
import type { RouteLocationRaw, LocationQueryRaw } from "vue-router"
import { Icon } from "@regira/modules/vue/ui"
import { isNavItem, type INavItem } from "@regira/modules/vue/entities"
import { useConfig } from "@/app-config"
import { useNavigation } from "@/components/entity-navigation"
import { useAccess } from "@/infrastructure/access"
import { queues } from "@/components/queues"

const { title } = useConfig()
const { isStaff } = useAccess()
// entity links come from config.json → navigation (+ the collected $configs), gated in useNavigation()
const { navbarTree } = useNavigation()
const to = (v: INavItem): RouteLocationRaw => ({ name: v.routeName, query: (v.initialQuery ?? {}) as LocationQueryRaw })
</script>

<template>
    <nav class="hd-sidenav">
        <router-link class="hd-brand d-flex align-items-center gap-2 text-decoration-none mb-4" :to="{ name: 'home' }">
            <span class="hd-brand-logo"><i class="bi bi-life-preserver"></i></span>
            <span>
                <span class="d-block fw-bold">{{ $tm(title) }}</span>
                <span class="d-block hd-brand-sub">{{ $t("supportCenter") }}</span>
            </span>
        </router-link>

        <template v-if="isStaff">
            <div class="hd-nav-section">{{ $t("workspace") }}</div>
            <router-link class="hd-nav-link" :to="{ name: 'home' }" exact-active-class="active">
                <Icon name="dashboard" /><span>{{ $t("dashboard") }}</span>
            </router-link>
            <router-link class="hd-nav-link" :to="{ name: 'board' }" active-class="active">
                <i class="bi bi-kanban"></i><span>{{ $t("kanbanBoard") }}</span>
            </router-link>

            <div class="hd-nav-section">{{ $t("queues") }}</div>
            <router-link v-for="q in queues" :key="q.key" class="hd-nav-link" active-class="" :to="{ name: 'TicketOverview', query: q.query }">
                <i :class="q.icon"></i><span>{{ $t(q.key) }}</span>
            </router-link>

            <div class="hd-nav-section">{{ $t("manage") }}</div>
            <template v-for="node in navbarTree?.roots ?? []" :key="node.value.id">
                <router-link v-if="isNavItem(node.value)" class="hd-nav-link" active-class="" :to="to(node.value as INavItem)">
                    <Icon :name="node.value.icon ?? ''" /><span>{{ $t(node.value.title) }}</span>
                </router-link>
                <template v-else-if="node.children?.length">
                    <div class="hd-nav-section">{{ $t(node.value.title) }}</div>
                    <router-link v-for="child in node.children" :key="child.value.id" class="hd-nav-link" active-class="" :to="to(child.value as INavItem)">
                        <Icon :name="child.value.icon ?? ''" /><span>{{ $t(child.value.title) }}</span>
                    </router-link>
                </template>
            </template>
        </template>

        <template v-else>
            <div class="hd-nav-section">{{ $t("mySupport") }}</div>
            <router-link class="hd-nav-link" :to="{ name: 'home' }" exact-active-class="active">
                <Icon name="home" /><span>{{ $t("home") }}</span>
            </router-link>
            <router-link class="hd-nav-link" active-class="" :to="{ name: 'TicketOverview' }">
                <i class="bi bi-ticket-detailed"></i><span>{{ $t("myTickets") }}</span>
            </router-link>
            <router-link class="hd-nav-link" :to="{ name: 'TicketDetails', params: { id: 'new' } }">
                <Icon name="new" /><span>{{ $t("newTicket") }}</span>
            </router-link>
        </template>

        <div class="hd-nav-section">{{ $t("account") }}</div>
        <router-link class="hd-nav-link" :to="{ name: 'account' }">
            <Icon name="user" /><span>{{ $t("myAccount") }}</span>
        </router-link>
    </nav>
</template>
