<!-- Toggle chips for the (small) equipment lookup — used as a multi-value room filter.
     The lookup has a handful of rows, so all of them are loaded once (pageSize 0) through the pooled store. -->
<script setup lang="ts">
import { onMounted, ref } from "vue"
import { useEntityStore as useEquipmentStore } from "@/entities/equipment"
import type { Entity as Equipment } from "@/entities/equipment"

const model = defineModel<Array<number> | undefined>()
const emit = defineEmits<{ change: [Array<number> | undefined] }>()
defineProps<{ size?: "sm" }>()

const { service } = useEquipmentStore()
const items = ref<Array<Equipment>>([])
onMounted(async () => {
    items.value = await service.list({ pageSize: 0 })
})

const isOn = (id: number) => model.value?.includes(id) ?? false
function toggle(id: number) {
    const current = model.value ?? []
    const next = isOn(id) ? current.filter((x) => x !== id) : [...current, id]
    model.value = next.length ? next : undefined
    emit("change", model.value)
}
</script>

<template>
    <div class="d-flex flex-wrap gap-1">
        <button
            v-for="eq in items"
            :key="eq.id"
            type="button"
            class="btn rounded-pill"
            :class="[isOn(eq.id) ? 'btn-primary' : 'btn-outline-secondary', size === 'sm' ? 'btn-sm' : '']"
            :title="eq.description"
            :aria-pressed="isOn(eq.id)"
            @click="toggle(eq.id)"
        >
            <i :class="eq.$iconClass" class="me-1"></i>{{ eq.title }}
        </button>
    </div>
</template>
