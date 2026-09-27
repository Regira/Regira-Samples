<!-- Avatar button + bottom sheet to pick which shopper this device shops as. -->
<script setup lang="ts">
import { onMounted, ref } from "vue"
import { RouterLink } from "vue-router"
import { injectModal } from "@regira/modules/vue/ui"
import useCurrentShopper from "@/infrastructure/current-shopper"

const Modal = injectModal()
const current = useCurrentShopper()
const isOpen = ref(false)

onMounted(() => {
    if (!current.isLoaded) current.load()
})

function choose(id: number) {
    current.select(current.shoppers.find((x) => x.id === id))
    isOpen.value = false
}
</script>

<template>
    <button type="button" class="sm-avatar-btn" :aria-label="$t('switchShopper')" @click="isOpen = true">
        <span class="sm-avatar" :style="{ background: current.shopper?.color || '#6c757d' }">{{ current.shopper?.$initials ?? "?" }}</span>
    </button>
    <Teleport to="#modals">
        <component :is="Modal" :is-visible="isOpen" :title="$t('whoIsShopping')" :show-footer="false" size="sm" @close="isOpen = false" @cancel="isOpen = false">
            <div class="list-group list-group-flush">
                <button
                    v-for="s in current.shoppers"
                    :key="s.id"
                    type="button"
                    class="list-group-item list-group-item-action d-flex align-items-center gap-3 py-3"
                    :class="{ active: s.id === current.shopperId }"
                    @click="choose(s.id)"
                >
                    <span class="sm-avatar" :style="{ background: s.color || '#6c757d' }">{{ s.$initials }}</span>
                    <span class="flex-grow-1 text-start">
                        <span class="d-block fw-semibold">{{ s.name }}</span>
                        <small class="opacity-75">{{ s.email }}</small>
                    </span>
                    <i v-if="s.id === current.shopperId" class="bi bi-check2 fs-5"></i>
                </button>
            </div>
            <RouterLink :to="{ name: 'ShopperOverview' }" class="btn btn-outline-secondary w-100 mt-3" @click="isOpen = false">
                <i class="bi bi-people me-1"></i>{{ $t("manageShoppers") }}
            </RouterLink>
        </component>
    </Teleport>
</template>
