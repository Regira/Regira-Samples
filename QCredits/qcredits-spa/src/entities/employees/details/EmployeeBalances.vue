<!-- Read-only credit history of one employee: the yearly allocations with their balance. -->
<script setup lang="ts">
import { ref, watch } from "vue"
import type { AxiosInstance } from "axios"
import { get } from "@regira/modules/vue/ioc"
import { FormSection, LoadingContainer } from "@regira/modules/vue/ui"
import { CreditBar } from "@/components/credits"
import { formatCredits } from "@/domain/enums"

interface AllocationRow {
    id: number
    year: number
    freeCredits: number
    usedCredits: number
    pendingCredits: number
    remainingCredits: number
    carriedOver: number
    minBalance: number
    reservedCredits: number
    reservedUsed: number
}

const props = defineProps<{ employeeId: number }>()
const rows = ref<Array<AllocationRow>>([])
const isLoading = ref(false)

async function load() {
    isLoading.value = true
    try {
        const axios = get<AxiosInstance>("axios")!
        const { data } = await axios.get("/credit-allocations/search", { params: { employeeId: props.employeeId, pageSize: 10 } })
        rows.value = data.items
    } finally {
        isLoading.value = false
    }
}
watch(() => props.employeeId, load, { immediate: true })
</script>

<template>
    <FormSection :title="$t('creditHistory')">
        <LoadingContainer :is-loading="isLoading">
            <p v-if="!rows.length" class="italic-muted mb-0">{{ $t("noResults") }}</p>
            <div v-for="row in rows" :key="row.id" class="row align-items-center py-2 border-bottom g-2">
                <div class="col-2 col-md-1 fw-semibold">{{ row.year }}</div>
                <div class="col"><CreditBar :free="row.freeCredits" :used="row.usedCredits" :pending="row.pendingCredits" :min-balance="row.minBalance" compact /></div>
                <div class="col-auto small text-muted d-none d-md-block">
                    {{ formatCredits(row.usedCredits) }} / {{ formatCredits(row.freeCredits) }} {{ $t("used") }}
                    <span v-if="row.carriedOver" class="ms-2">({{ row.carriedOver > 0 ? "+" : "" }}{{ formatCredits(row.carriedOver) }} {{ $t("carriedOver") }})</span>
                </div>
                <div class="col-auto">
                    <RouterLink :to="{ name: 'CreditAllocationDetails', params: { id: row.id } }" class="btn btn-sm btn-outline-secondary">
                        <i class="bi bi-box-arrow-up-right"></i>
                    </RouterLink>
                </div>
            </div>
        </LoadingContainer>
    </FormSection>
</template>
