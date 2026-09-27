<!-- Compact sticky app bar: brand, global article search (config.json → navigation.search) and the shopper switcher. -->
<script setup lang="ts">
import { ref } from "vue"
import { useRouter } from "vue-router"
import { useConfig } from "@/app-config"
import { useNavigation } from "@/components/entity-navigation"
import ShopperSwitcher from "./ShopperSwitcher.vue"

const { title } = useConfig()
const { searchItemConfig } = useNavigation()
const router = useRouter()
const searchOpen = ref(false)
const q = ref("")

function handleSearch() {
    const cfg = searchItemConfig.value
    if (!cfg || !q.value) return
    router.push({ name: `${cfg.key}Overview`, query: { q: q.value } })
    q.value = ""
    searchOpen.value = false
}
</script>

<template>
    <div class="sm-appbar">
        <template v-if="!searchOpen">
            <router-link class="sm-brand" :to="{ name: 'home' }">
                <img src="/favicon.svg" alt="" width="30" height="30" />
                <span>{{ $tm(title) }}</span>
            </router-link>
            <div class="d-flex align-items-center gap-1 ms-auto">
                <button v-if="searchItemConfig" type="button" class="sm-icon-btn" :aria-label="$t('search')" @click="searchOpen = true">
                    <i class="bi bi-search"></i>
                </button>
                <ShopperSwitcher />
            </div>
        </template>
        <form v-else class="sm-appbar__search" @submit.prevent="handleSearch">
            <button type="button" class="sm-icon-btn" :aria-label="$t('close')" @click="searchOpen = false"><i class="bi bi-arrow-left"></i></button>
            <input
                v-model.trim="q"
                v-focus
                type="search"
                class="form-control"
                enterkeyhint="search"
                :placeholder="`${$t('search')} ${$t(searchItemConfig?.overviewTitle || '')}`"
            />
            <button type="submit" class="sm-icon-btn" :aria-label="$t('search')"><i class="bi bi-search"></i></button>
        </form>
    </div>
</template>
