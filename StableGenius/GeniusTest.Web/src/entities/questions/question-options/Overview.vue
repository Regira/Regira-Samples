<!-- Owned-collection editor for Question.Options — an editable table of scalar rows.
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged
     rows so `Related()` deletes them by omission. New rows mint negative temp ids and insert with save(). -->
<script setup lang="ts">
import { useOwnedCollection } from "@regira/modules/vue/entities"
import QuestionOption from "./Entity"

const props = defineProps<{ modelValue?: Array<QuestionOption>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<QuestionOption>] }>()
const { items, newItem, handleSave } = useOwnedCollection<QuestionOption>({ props, emit, createRow: () => new QuestionOption() })
</script>

<template>
    <div class="options-editor">
        <div class="row g-2 mb-1 small fw-bold text-muted d-none d-md-flex">
            <div class="col-md-2">{{ $t("optionText") }}</div>
            <div class="col-md-2">{{ $t("praiseAs") }}</div>
            <div class="col-md-auto">{{ $t("isCorrect") }}</div>
            <div class="col-md-auto">{{ $t("isUnreachable") }}</div>
            <div class="col-md">{{ $t("customReaction") }}</div>
            <div v-if="!readonly" class="col-auto" style="width: 3rem"></div>
        </div>
        <div v-for="row in items" :key="row.id" class="row g-2 mb-2 align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col-6 col-md-2">
                <input v-model="row.text" :readonly="readonly || row._deleted" maxlength="128" class="form-control" :placeholder="$t('optionText')" />
            </div>
            <div class="col-6 col-md-2">
                <input v-model="row.praiseAs" :readonly="readonly || row._deleted" maxlength="128" class="form-control" :placeholder="$t('praiseAsPlaceholder')" :title="$t('praiseAsHint')" />
            </div>
            <div class="col-auto">
                <div class="form-check mb-0" :title="$t('isCorrect')">
                    <input :id="`opt-correct-${row.id}`" v-model="row.isCorrect" :disabled="readonly || row._deleted" type="checkbox" class="form-check-input" />
                    <label :for="`opt-correct-${row.id}`" class="form-check-label d-md-none">{{ $t("isCorrect") }}</label>
                </div>
            </div>
            <div class="col-auto">
                <div class="form-check mb-0" :title="$t('isUnreachableHint')">
                    <input :id="`opt-unreachable-${row.id}`" v-model="row.isUnreachable" :disabled="readonly || row._deleted" type="checkbox" class="form-check-input" />
                    <label :for="`opt-unreachable-${row.id}`" class="form-check-label d-md-none">{{ $t("isUnreachable") }}</label>
                </div>
            </div>
            <div class="col">
                <input
                    v-model="row.customReaction"
                    :readonly="readonly || row._deleted || row.isCorrect"
                    maxlength="512"
                    class="form-control"
                    :placeholder="row.isCorrect ? '—' : $t('customReactionPlaceholder')"
                />
            </div>
            <div v-if="!readonly" class="col-auto">
                <button type="button" class="btn btn-outline-danger" :title="row._deleted ? $t('restore') : $t('remove')" @click="row._deleted = !row._deleted">
                    <i :class="row._deleted ? 'bi bi-arrow-counterclockwise' : 'bi bi-x-lg'"></i>
                </button>
            </div>
        </div>
        <div v-if="newItem && !readonly" class="row g-2 mb-1 align-items-center">
            <div class="col-6 col-md-2">
                <input v-model="newItem.text" maxlength="128" class="form-control" :placeholder="$t('newOption')" @keyup.enter="handleSave({ saved: newItem, isNew: true })" />
            </div>
            <div class="col-6 col-md-2">
                <input v-model="newItem.praiseAs" maxlength="128" class="form-control" :placeholder="$t('praiseAsPlaceholder')" :title="$t('praiseAsHint')" />
            </div>
            <div class="col-auto">
                <div class="form-check mb-0" :title="$t('isCorrect')">
                    <input id="opt-correct-new" v-model="newItem.isCorrect" type="checkbox" class="form-check-input" />
                    <label for="opt-correct-new" class="form-check-label d-md-none">{{ $t("isCorrect") }}</label>
                </div>
            </div>
            <div class="col-auto">
                <div class="form-check mb-0" :title="$t('isUnreachableHint')">
                    <input id="opt-unreachable-new" v-model="newItem.isUnreachable" type="checkbox" class="form-check-input" />
                    <label for="opt-unreachable-new" class="form-check-label d-md-none">{{ $t("isUnreachable") }}</label>
                </div>
            </div>
            <div class="col">
                <input v-model="newItem.customReaction" maxlength="512" class="form-control" :placeholder="$t('customReactionPlaceholder')" />
            </div>
            <div class="col-auto">
                <button type="button" class="btn btn-success" :disabled="!newItem.text" @click="handleSave({ saved: newItem, isNew: true })">
                    <i class="bi bi-plus-lg"></i>
                </button>
            </div>
        </div>
    </div>
</template>
