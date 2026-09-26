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

        <div class="row g-2 mb-2 align-items-center">
            <div class="col-sm-6">
                <FormLabel :label="$t('sortBy')" />
                <select v-model="searchObject.sortBy" class="form-select" @change="handleUpdate">
                    <option :value="undefined">{{ $t("sortCreatedDesc") }}</option>
                    <option :value="GameSortBy.ScoreDesc">{{ $t("sortScoreDesc") }}</option>
                    <option :value="GameSortBy.PlayerName">{{ $t("sortPlayerName") }}</option>
                </select>
            </div>
            <div class="col-sm-6 pt-sm-4">
                <NullableCheckBox v-model="searchObject.isFinished" id="gameIsFinished" :label="$t('isFinished')" @update:modelValue="handleUpdate" />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { FormLabel, IconButton, NullableCheckBox } from "@regira/modules/vue/ui"
import { useFilter, type FilterEmits } from "@regira/modules/vue/entities"
import { GameSortBy } from "../data/Entity"
import SearchObject from "./SearchObject"

interface Emits extends /* @vue-ignore */ FilterEmits<SearchObject> {}
const emit = defineEmits<Emits & { "update:modelValue": (v: SearchObject) => true; filter: (v: SearchObject) => true; close: () => void }>()
defineProps<{ resultCount?: number }>()

const searchObject = defineModel<SearchObject>({ required: true })
const { handleReset, handleUpdate, filterIsActive } = useFilter({ searchObject, emit, Constructor: SearchObject })
</script>
