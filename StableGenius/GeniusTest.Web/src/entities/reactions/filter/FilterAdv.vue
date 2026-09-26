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

        <div class="row g-2 mb-2">
            <div class="col-sm-6">
                <select v-model="searchObject.part" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("allParts") }}</option>
                    <option v-for="p in Object.values(ReactionPart)" :key="p" :value="p">{{ $t("part" + p) }}</option>
                </select>
            </div>
            <div class="col-sm-6">
                <select v-model="searchObject.strategy" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("allStrategies") }}</option>
                    <option v-for="s in Object.values(SpinStrategy)" :key="s" :value="s">{{ $t("strategy" + s) }}</option>
                </select>
            </div>
        </div>
        <NullableCheckBox v-model="searchObject.isActive" id="reactionIsActive" :label="$t('isActive')" @update:modelValue="handleUpdate" />
    </div>
</template>

<script setup lang="ts">
import { IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import { ReactionPart, SpinStrategy } from "../data/Entity"
import SearchObject from "./SearchObject"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const { handleReset, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
</script>
