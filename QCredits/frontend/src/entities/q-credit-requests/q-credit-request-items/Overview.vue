<!-- Owned-collection editor for QCreditRequest.Items - an editable table of scalar rows.
     Bind it in the parent Form.vue to the ARRAY: <QCreditRequestItemOverview v-model="item.items" />
     Removal marks `_deleted` (undoable until save); the parent's EntityService.prepareItem drops flagged
     rows so `Related()` deletes them by omission. New rows mint negative temp ids and insert with save(). -->
<script setup lang="ts">
import { computed } from "vue"
import { useOwnedCollection } from "@regira/modules/vue/entities"
import { DateInput } from "@regira/modules/vue/ui"
import QCreditRequestItem, { CreditActivityTypes } from "./Entity"

const props = defineProps<{ modelValue?: Array<QCreditRequestItem>; readonly?: boolean }>()
const emit = defineEmits<{ "update:modelValue": [Array<QCreditRequestItem>] }>()
const { items, newItem, handleSave } = useOwnedCollection<QCreditRequestItem>({ props, emit, createRow: () => new QCreditRequestItem() })

const totalCredits = computed(() => items.value.filter((x) => !x._deleted).reduce((sum, x) => sum + (x.credits || 0), 0))
</script>

<template>
    <div class="items-editor">
        <div class="row fw-bold border-bottom pb-1 mb-1 d-none d-md-flex">
            <div class="col-md-3">Description</div>
            <div class="col-md-2">Type</div>
            <div class="col-md-1">Credits</div>
            <div class="col-md-2">Activity date</div>
            <div class="col-md-1">Cost</div>
            <div class="col-md-2">Provider</div>
            <div class="col-md-1"></div>
        </div>

        <div v-for="row in items" :key="row.id" class="row g-2 mb-1 align-items-center" :class="{ 'is-deleted': row._deleted }">
            <div class="col-md-3">
                <input v-model="row.description" :readonly="readonly || row._deleted" class="form-control" placeholder="Description" />
            </div>
            <div class="col-md-2">
                <select v-model="row.type" :disabled="readonly || row._deleted" class="form-select">
                    <option :value="CreditActivityTypes.Course">Course</option>
                    <option :value="CreditActivityTypes.Book">Book</option>
                    <option :value="CreditActivityTypes.Subscription">Subscription</option>
                    <option :value="CreditActivityTypes.SelfStudy">Self-study</option>
                </select>
            </div>
            <div class="col-md-1">
                <input type="number" step="0.5" min="0" v-model.number="row.credits" :readonly="readonly || row._deleted" class="form-control" />
            </div>
            <div class="col-md-2">
                <DateInput v-model="row.activityDate" :readonly="readonly || row._deleted" />
            </div>
            <div class="col-md-1">
                <input type="number" step="0.01" v-model.number="row.cost" :readonly="readonly || row._deleted" class="form-control" />
            </div>
            <div class="col-md-2">
                <input v-model="row.provider" :readonly="readonly || row._deleted" class="form-control" placeholder="Provider" />
            </div>
            <div v-if="!readonly" class="col-md-1 text-end">
                <button type="button" class="btn btn-outline-danger btn-sm" :title="row._deleted ? 'Restore' : 'Remove'" @click="row._deleted = !row._deleted">
                    {{ row._deleted ? "↺" : "×" }}
                </button>
            </div>
        </div>

        <div v-if="newItem && !readonly" class="row g-2 mb-1 align-items-center">
            <div class="col-md-3"><input v-model="newItem.description" class="form-control" placeholder="Description" /></div>
            <div class="col-md-2">
                <select v-model="newItem.type" class="form-select">
                    <option :value="CreditActivityTypes.Course">Course</option>
                    <option :value="CreditActivityTypes.Book">Book</option>
                    <option :value="CreditActivityTypes.Subscription">Subscription</option>
                    <option :value="CreditActivityTypes.SelfStudy">Self-study</option>
                </select>
            </div>
            <div class="col-md-1"><input type="number" step="0.5" min="0" v-model.number="newItem.credits" class="form-control" /></div>
            <div class="col-md-2"><DateInput v-model="newItem.activityDate" /></div>
            <div class="col-md-1"><input type="number" step="0.01" v-model.number="newItem.cost" class="form-control" /></div>
            <div class="col-md-2"><input v-model="newItem.provider" class="form-control" placeholder="Provider" /></div>
            <div class="col-md-1 text-end">
                <button type="button" class="btn btn-success btn-sm" @click="handleSave({ saved: newItem, isNew: true })">+</button>
            </div>
        </div>

        <div class="text-end fw-semibold mt-2">Total: {{ totalCredits }} QCredits</div>
    </div>
</template>
