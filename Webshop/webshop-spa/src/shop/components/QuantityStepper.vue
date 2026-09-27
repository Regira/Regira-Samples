<script setup lang="ts">
const props = withDefaults(defineProps<{ max?: number; min?: number; size?: "sm" | "md" }>(), { max: 99, min: 1, size: "md" })
const quantity = defineModel<number>({ required: true })
function set(value: number) {
    quantity.value = Math.max(props.min, Math.min(props.max, Math.round(value) || props.min))
}
</script>

<template>
    <div class="input-group ws-stepper" :class="{ 'input-group-sm': size === 'sm' }">
        <button type="button" class="btn btn-outline-secondary" :disabled="quantity <= min" aria-label="Decrease quantity" @click="set(quantity - 1)">
            <i class="bi bi-dash"></i>
        </button>
        <input
            type="number"
            class="form-control text-center"
            :value="quantity"
            :min="min"
            :max="max"
            aria-label="Quantity"
            @change="set(Number(($event.target as HTMLInputElement).value))"
        />
        <button type="button" class="btn btn-outline-secondary" :disabled="quantity >= max" aria-label="Increase quantity" @click="set(quantity + 1)">
            <i class="bi bi-plus"></i>
        </button>
    </div>
</template>
