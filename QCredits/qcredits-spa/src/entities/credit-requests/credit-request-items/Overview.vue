<!-- Owned-collection editor for CreditRequest.Items — the purchases/activities of one request.
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged
     rows so `Related()` deletes them by omission. New rows mint negative temp ids and insert with save(). -->
<script setup lang="ts">
import { computed } from "vue"
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { ActivityType, activityTypeIcon, CREDIT_VALUE_EUR, formatCredits, formatEuro } from "@/domain/enums"
import CreditRequestItem from "./Entity"

const props = defineProps<{ modelValue?: Array<CreditRequestItem>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<CreditRequestItem>] }>()
const { items, newItem, handleSave } = useOwnedCollection<CreditRequestItem>({ props, emit, createRow: () => new CreditRequestItem() })

const activeItems = computed(() => items.value.filter((x) => !x._deleted))
const totalCredits = computed(() => activeItems.value.reduce((sum, x) => sum + (Number(x.credits) || 0), 0))
const totalCost = computed(() => activeItems.value.reduce((sum, x) => sum + (Number(x.cost) || 0), 0))

/** 1 credit = half a day or EUR 250: suggest credits from time (half days) + cost, rounded up to 0.5 */
function suggestCredits(row: CreditRequestItem) {
    const fromCost = row.cost > 0 ? Math.max(0.5, Math.ceil((row.cost / CREDIT_VALUE_EUR) * 2) / 2) : 0
    if (fromCost > row.credits) row.credits = fromCost
}
function addRow() {
    if (!newItem.value?.description?.trim()) return
    handleSave({ saved: newItem.value, isNew: true })
}
</script>

<template>
    <div class="qc-items">
        <div class="row g-2 small fw-semibold text-muted border-bottom pb-1 d-none d-lg-flex">
            <div class="col-lg-2">{{ $t("type") }}</div>
            <div class="col-lg">{{ $t("description") }}</div>
            <div class="col-lg-2">{{ $t("provider") }}</div>
            <div class="col-lg-2">{{ $t("date") }}</div>
            <div class="col-lg-1">{{ $t("cost") }}</div>
            <div class="col-lg-1">{{ $t("creditsHeader") }}</div>
            <div v-if="!readonly" class="col-lg-auto qc-items-action"></div>
        </div>

        <div v-for="row in items" :key="row.id" class="row g-2 py-2 border-bottom align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col-6 col-lg-2">
                <div class="input-group input-group-sm">
                    <span class="input-group-text"><i :class="activityTypeIcon[row.type]"></i></span>
                    <select v-model="row.type" class="form-select form-select-sm" :disabled="readonly || row._deleted">
                        <option v-for="t in Object.values(ActivityType)" :key="t" :value="t">{{ $t(`activity${t}`) }}</option>
                    </select>
                </div>
            </div>
            <div class="col-12 col-lg order-first order-lg-0">
                <input v-model="row.description" :readonly="readonly || row._deleted" class="form-control form-control-sm" required maxlength="200" :placeholder="$t('description')" />
            </div>
            <div class="col-6 col-lg-2">
                <input v-model="row.provider" :readonly="readonly || row._deleted" class="form-control form-control-sm" maxlength="150" :placeholder="$t('provider')" />
            </div>
            <div class="col-6 col-lg-2">
                <input v-model="row.activityDate" type="date" :readonly="readonly || row._deleted" class="form-control form-control-sm" />
            </div>
            <div class="col-3 col-lg-1">
                <input v-model.number="row.cost" type="number" min="0" step="1" :readonly="readonly || row._deleted" class="form-control form-control-sm" :title="$t('costEur')" @change="suggestCredits(row)" />
            </div>
            <div class="col-3 col-lg-1">
                <input v-model.number="row.credits" type="number" min="0.5" max="40" step="0.5" :readonly="readonly || row._deleted" class="form-control form-control-sm fw-semibold" required />
            </div>
            <div v-if="!readonly" class="col-auto qc-items-action">
                <button type="button" class="btn btn-sm" :class="row._deleted ? 'btn-outline-secondary' : 'btn-outline-danger'" :title="row._deleted ? $t('restore') : $t('delete')" @click="row._deleted = !row._deleted">
                    <i :class="row._deleted ? 'bi bi-arrow-counterclockwise' : 'bi bi-trash'"></i>
                </button>
            </div>
        </div>

        <!-- add-row -->
        <div v-if="newItem && !readonly" class="row g-2 py-2 align-items-center qc-add-row">
            <div class="col-6 col-lg-2">
                <select v-model="newItem.type" class="form-select form-select-sm">
                    <option v-for="t in Object.values(ActivityType)" :key="t" :value="t">{{ $t(`activity${t}`) }}</option>
                </select>
            </div>
            <div class="col-12 col-lg order-first order-lg-0">
                <input v-model="newItem.description" class="form-control form-control-sm" maxlength="200" :placeholder="$t('newActivityPlaceholder')" @keydown.enter.prevent="addRow" />
            </div>
            <div class="col-6 col-lg-2"><input v-model="newItem.provider" class="form-control form-control-sm" maxlength="150" :placeholder="$t('provider')" /></div>
            <div class="col-6 col-lg-2"><input v-model="newItem.activityDate" type="date" class="form-control form-control-sm" /></div>
            <div class="col-3 col-lg-1"><input v-model.number="newItem.cost" type="number" min="0" class="form-control form-control-sm" :title="$t('costEur')" @change="suggestCredits(newItem)" /></div>
            <div class="col-3 col-lg-1"><input v-model.number="newItem.credits" type="number" min="0.5" max="40" step="0.5" class="form-control form-control-sm" /></div>
            <div class="col-auto qc-items-action">
                <button type="button" class="btn btn-sm btn-success" :title="$t('addActivity')" :disabled="!newItem.description?.trim()" @click="addRow"><i class="bi bi-plus-lg"></i></button>
            </div>
        </div>

        <div class="d-flex justify-content-end gap-4 pt-2 fw-semibold">
            <span class="text-muted">{{ $t("totalCost") }}: {{ formatEuro(totalCost) }}</span>
            <span>{{ $t("totalCredits") }}: <span class="text-primary">{{ formatCredits(totalCredits) }}</span></span>
        </div>
        <div class="form-text">{{ $t("creditRule") }}</div>
    </div>
</template>

<style scoped>
.qc-items-action {
    width: 3rem;
}
.qc-add-row {
    background-color: var(--bs-tertiary-bg);
    border-radius: 0.375rem;
}
</style>
