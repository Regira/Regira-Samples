<!-- Owned-collection editor for Asset.Attachments -- an editable table of scalar rows.
     Bind it in the parent Form.vue to the ARRAY: <AssetAttachmentOverview v-model="item.attachments" /> -->
<script setup lang="ts">
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { formatDate } from "@regira/modules/vue/formatters"
import AssetAttachment from "./Entity"

const props = defineProps<{ modelValue?: Array<AssetAttachment>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<AssetAttachment>] }>()
const { items, newItem, handleSave } = useOwnedCollection<AssetAttachment>({
    props,
    emit,
    createRow: () => Object.assign(new AssetAttachment(), { uploadedAt: new Date() }),
})
</script>

<template>
    <div class="attachments-editor">
        <div v-if="items.length" class="row g-2 mb-1 fw-bold small text-muted">
            <div class="col">{{ $t("assetAttachment.fileName") }}</div>
            <div class="col-2">{{ $t("assetAttachment.contentType") }}</div>
            <div class="col-2">{{ $t("assetAttachment.description") }}</div>
            <div class="col-2">{{ $t("assetAttachment.uploadedAt") }}</div>
        </div>
        <div v-for="row in items" :key="row.id" class="row g-2 mb-1 align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col">
                <input v-model="row.fileName" :readonly="readonly || row._deleted" class="form-control" :placeholder="$t('assetAttachment.fileName')" />
            </div>
            <div class="col-2">
                <input v-model="row.contentType" :readonly="readonly || row._deleted" class="form-control" placeholder="application/pdf" />
            </div>
            <div class="col-2">
                <input v-model="row.description" :readonly="readonly || row._deleted" class="form-control" :placeholder="$t('assetAttachment.description')" />
            </div>
            <div class="col-2 text-truncate">{{ formatDate(row.uploadedAt) }}</div>
            <div v-if="!readonly" class="col-auto">
                <button type="button" class="btn btn-outline-danger" :title="row._deleted ? 'Restore' : 'Remove'" @click="row._deleted = !row._deleted">
                    {{ row._deleted ? "↺" : "×" }}
                </button>
            </div>
        </div>
        <div v-if="newItem && !readonly" class="row g-2 mb-1 align-items-center">
            <div class="col">
                <input
                    v-model="newItem.fileName"
                    class="form-control"
                    :placeholder="$t('assetAttachment.fileName')"
                    @keyup.enter="handleSave({ saved: newItem, isNew: true })"
                />
            </div>
            <div class="col-2"><input v-model="newItem.contentType" class="form-control" placeholder="application/pdf" /></div>
            <div class="col-2"><input v-model="newItem.description" class="form-control" :placeholder="$t('assetAttachment.description')" /></div>
            <div class="col-2"></div>
            <div class="col-auto"><button type="button" class="btn btn-success" @click="handleSave({ saved: newItem, isNew: true })">+</button></div>
        </div>
    </div>
</template>
