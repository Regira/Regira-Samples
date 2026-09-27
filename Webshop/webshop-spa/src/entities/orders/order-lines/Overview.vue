<!-- Owned-collection editor for Order.OrderLines — an editable table of rows that carry a relation (product) and a scalar (quantity).
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged rows so
     `Related()` deletes them by omission. New rows mint negative temp ids and insert with the parent's save().
     Prices are server-owned: the API prices new lines from the product and keeps existing lines at their ordered price. -->
<script setup lang="ts">
import { computed } from "vue"
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { InputSelector as ProductInputSelector } from "@/entities/products"
import { money } from "@/shop/money"
import OrderLine from "./Entity"

const props = defineProps<{ modelValue?: Array<OrderLine>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<OrderLine>] }>()
const { items, newItem, handleSave } = useOwnedCollection<OrderLine>({ props, emit, createRow: () => new OrderLine() })

const usedProductIds = computed(() => items.value.filter((l) => !l._deleted && l.productId).map((l) => l.productId!))
function addLine() {
    if (!newItem.value?.productId || !(newItem.value.quantity > 0)) return
    newItem.value.productTitle = newItem.value.product?.title
    newItem.value.unitPrice = newItem.value.product?.price
    newItem.value.lineTotal = (newItem.value.product?.price ?? 0) * newItem.value.quantity
    handleSave({ saved: newItem.value, isNew: true })
}
</script>

<template>
    <div class="order-lines-editor">
        <div class="row g-2 fw-semibold border-bottom pb-2 mb-2 small text-body-secondary">
            <div class="col">{{ $t("product") }}</div>
            <div class="col-3 col-md-2">{{ $t("quantity") }}</div>
            <div class="col-2 d-none d-md-block text-end">{{ $t("unitPrice") }}</div>
            <div class="col-2 text-end">{{ $t("lineTotal") }}</div>
            <div v-if="!readonly" class="col-auto" style="width: 3rem"></div>
        </div>
        <div v-for="row in items" :key="row.id" class="row g-2 mb-2 align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col">
                <ProductInputSelector v-if="row.id < 0" v-model="row.product" v-model:idValue="row.productId" :readonly="readonly || row._deleted" :can-edit="false" />
                <span v-else class="text-truncate d-block">{{ row.$title }}</span>
            </div>
            <div class="col-3 col-md-2">
                <input type="number" min="1" max="99" v-model.number="row.quantity" :readonly="readonly || row._deleted" class="form-control" />
            </div>
            <div class="col-2 d-none d-md-block text-end">{{ money(row.unitPrice ?? row.product?.price) }}</div>
            <div class="col-2 text-end fw-semibold">{{ money((row.unitPrice ?? row.product?.price ?? 0) * row.quantity) }}</div>
            <div v-if="!readonly" class="col-auto">
                <button type="button" class="btn btn-outline-danger btn-sm" :title="row._deleted ? $t('restore') : $t('delete')" @click="row._deleted = !row._deleted">
                    <i class="bi" :class="row._deleted ? 'bi-arrow-counterclockwise' : 'bi-x-lg'"></i>
                </button>
            </div>
        </div>
        <div v-if="!items.length" class="text-body-secondary small mb-2">{{ $t("noOrderLines") }}</div>
        <div v-if="newItem && !readonly" class="row g-2 mb-3 align-items-center ws-add-row">
            <div class="col">
                <ProductInputSelector
                    v-model="newItem.product"
                    v-model:idValue="newItem.productId"
                    :can-edit="false"
                    :filter-defaults="{ exclude: usedProductIds, isActive: true }"
                    :placeholder="$t('addProduct')"
                />
            </div>
            <div class="col-3 col-md-2">
                <input type="number" min="1" max="99" v-model.number="newItem.quantity" class="form-control" @keyup.enter="addLine" />
            </div>
            <div class="col-2 d-none d-md-block text-end text-body-secondary">{{ newItem.product ? money(newItem.product.price) : "" }}</div>
            <div class="col-2"></div>
            <div class="col-auto">
                <button type="button" class="btn btn-success btn-sm" :disabled="!newItem.productId" :title="$t('add')" @click="addLine"><i class="bi bi-plus-lg"></i></button>
            </div>
        </div>
    </div>
</template>
