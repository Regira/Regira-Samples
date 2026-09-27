<template>
    <div class="adv-filter">
        <div class="row">
            <div class="col mb-2" v-if="resultCount != null">
                <span class="text-info">{{ resultCount }} {{ $t("results") }}</span>
                <small v-if="filterIsActive" class="ms-2 italic-muted">({{ $t("filtersAreApplied") }})</small>
            </div>
            <div class="col mb-2 text-end">
                <IconButton icon="clear" :showText="true" @click="handleReset" />
            </div>
        </div>

        <input v-model.lazy.trim="searchObject.q" class="form-control mb-2" :placeholder="$t('keywords')" @change="handleUpdate" />
        <div class="mb-2">
            <FormLabel :label="$t('canPerform')" />
            <InterventionTypeInputSelector
                v-model="filterType"
                v-model:idValue="searchObject.interventionTypeId as number"
                :canEdit="false"
                :placeholder="$t('interventionType')"
                @select="handleUpdate"
            />
        </div>
        <div class="row g-2">
            <div class="col-md-6 mb-2">
                <FormLabel :label="$t('city')" />
                <input v-model.lazy.trim="searchObject.city" class="form-control" @change="handleUpdate" />
            </div>
            <div class="col-md-6 mb-2 d-flex align-items-end">
                <NullableCheckBox v-model="searchObject.isActive" id="supplierIsActive" :label="$t('active')" @update:modelValue="handleUpdate" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref } from "vue"
import { FormLabel, IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import { InputSelector as InterventionTypeInputSelector } from "@/entities/intervention-types"
import type { Entity as InterventionType } from "@/entities/intervention-types"
import SearchObject from "./SearchObject"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const filterType = ref<InterventionType>()
const { handleReset: resetSearchObject, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
function handleReset() {
    resetSearchObject()
    filterType.value = undefined
}
</script>
