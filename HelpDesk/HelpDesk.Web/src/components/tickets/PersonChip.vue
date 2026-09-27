<script setup lang="ts">
import { computed } from "vue"
import { FormModalButton as PersonButton, useEntityStore as usePersonStore, type Entity as Person } from "@/entities/persons"

// A person on a ticket: avatar initial + pooled label, and the person's FormModalButton (open the record).
const props = defineProps<{ person?: Partial<Person>; showButton?: boolean; muted?: string }>()
const { fromPool } = usePersonStore()
const pooled = computed(() => (props.person ? fromPool(props.person as Person) : undefined))
const initials = computed(() => ((pooled.value?.givenName?.[0] ?? "") + (pooled.value?.familyName?.[0] ?? "")).toUpperCase() || "?")
</script>
<template>
    <span v-if="pooled" class="d-inline-flex align-items-center gap-1 text-truncate mw-100">
        <span class="hd-avatar hd-avatar-sm" :class="pooled.role === 'Employee' ? 'hd-avatar-staff' : 'hd-avatar-customer'">{{ initials }}</span>
        <span class="text-truncate">{{ pooled.$title }}</span>
        <PersonButton v-if="showButton" :model-value="pooled" class="btn btn-link btn-sm p-0 ms-1" />
    </span>
    <span v-else-if="muted" class="text-muted fst-italic small">{{ muted }}</span>
</template>
