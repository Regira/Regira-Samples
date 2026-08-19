<template>
    <section>
        <h1>{{ $t("balances") }}</h1>

        <div class="row align-items-end mb-3">
            <div class="col-auto">
                <label class="form-label mb-1">{{ $t("year") }}</label>
                <select v-model.number="year" class="form-select" @change="loadAll">
                    <option v-for="y in years" :key="y" :value="y">{{ y }}</option>
                </select>
            </div>
            <div class="col-auto" style="min-width: 260px">
                <label class="form-label mb-1">{{ $t("employee") }}</label>
                <EmployeeInputSelector v-model="selectedEmployee" @select="loadOne" :placeholder="$t('employee')" />
            </div>
        </div>

        <div v-if="selectedBalance" class="card mb-4">
            <div class="card-body">
                <h5 class="card-title">{{ selectedBalance.employeeName }} — {{ selectedBalance.year }}</h5>
                <p class="text-muted mb-2">{{ selectedBalance.department }}</p>
                <BalanceBar :balance="selectedBalance" />
            </div>
        </div>

        <LoadingContainer :is-loading="isLoading">
            <table class="table align-middle">
                <thead>
                    <tr>
                        <th>{{ $t("employee") }}</th>
                        <th class="d-none d-md-table-cell">{{ $t("department") }}</th>
                        <th>{{ $t("remainingCredits") }}</th>
                        <th class="d-none d-lg-table-cell">{{ $t("pendingCredits") }}</th>
                        <th style="width: 220px">{{ $t("balances") }}</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="row in balances" :key="row.employeeId">
                        <td class="text-truncate">{{ row.employeeName }}</td>
                        <td class="d-none d-md-table-cell text-truncate">{{ row.department }}</td>
                        <td :class="row.remainingCredits < 0 ? 'text-danger fw-semibold' : ''">{{ row.remainingCredits }}</td>
                        <td class="d-none d-lg-table-cell">{{ row.pendingCredits }}</td>
                        <td><BalanceBar :balance="row" compact /></td>
                    </tr>
                </tbody>
            </table>
        </LoadingContainer>
    </section>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue"
import { LoadingContainer } from "@regira/modules/vue/ui"
import { useAxios } from "@regira/modules/vue/http"
import { InputSelector as EmployeeInputSelector, type Entity as Employee } from "@/entities/employees"
import BalanceBar from "@/components/balances/BalanceBar.vue"
import type { EmployeeBalance } from "@/components/balances/types"

const axios = useAxios()
const currentYear = new Date().getFullYear()
const years = [currentYear - 2, currentYear - 1, currentYear]
const year = ref(currentYear)

const balances = ref<Array<EmployeeBalance>>([])
const isLoading = ref(false)
const selectedEmployee = ref<Employee>()
const selectedBalance = ref<EmployeeBalance>()

async function loadAll() {
    isLoading.value = true
    try {
        const { data } = await axios.get<{ items: Array<EmployeeBalance> }>("/balances", { params: { year: year.value } })
        balances.value = data.items
    } finally {
        isLoading.value = false
    }
    if (selectedEmployee.value) {
        await loadOne()
    }
}
async function loadOne() {
    if (!selectedEmployee.value?.id) {
        selectedBalance.value = undefined
        return
    }
    const { data } = await axios.get<EmployeeBalance>(`/balances/${selectedEmployee.value.id}`, { params: { year: year.value } })
    selectedBalance.value = data
}

onMounted(loadAll)
</script>
