<template>
    <div class="adv-filter">
        <!-- top row: result count (left) + clear (right) — the overview-filter convention; keep it -->
        <div class="row">
            <div class="col mb-2" v-if="resultCount != null">
                <span class="text-info">{{ resultCount }} {{ $t("results") }}</span>
                <small v-if="filterIsActive" class="ms-2 italic-muted">({{ $t("filtersAreApplied") }})</small>
            </div>
            <div class="col mb-2 text-end">
                <IconButton icon="clear" :showText="true" @click="handleReset" />
            </div>
        </div>

        <!-- keywords (free-text q) -->
        <input v-model.lazy.trim="searchObject.q" class="form-control mb-2" :placeholder="$t('keywords')" @change="handleUpdate" />

        <div class="mb-2">
            <ShoppingListInputSelector
                v-model="filterShoppingList"
                v-model:idValue="searchObject.shoppingListId as number"
                :canEdit="false"
                :placeholder="$t('shoppingList')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <ShopperInputSelector
                v-model="filterShopper"
                v-model:idValue="searchObject.shopperId as number"
                :canEdit="false"
                :placeholder="$t('shopper')"
                @select="handleUpdate"
            />
        </div>
        <div class="mb-2">
            <NullableCheckBox v-model="searchObject.isActive" id="filterIsActive" :label="$t('stillToBuy')" @update:modelValue="handleUpdate" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import SearchObject from "./SearchObject"
import { InputSelector as ShoppingListInputSelector } from "@/entities/shopping-lists"
import type { Entity as ShoppingList } from "@/entities/shopping-lists"
import { InputSelector as ShopperInputSelector } from "@/entities/shoppers"
import type { Entity as Shopper } from "@/entities/shoppers"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterShoppingList = ref<ShoppingList>()
const filterShopper = ref<Shopper>()
// handleUpdate = sync the model + re-run the search; bind it on EVERY input above.
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
// Clear the selector-backing entities too — resetting the ids alone leaves each control showing a label.
function handleReset() {
    resetSearchObject()
    filterShoppingList.value = undefined
    filterShopper.value = undefined
}
</script>
