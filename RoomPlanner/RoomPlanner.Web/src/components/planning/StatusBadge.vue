<script setup lang="ts">
import { statusIcon, statusVariant } from "@/utilities/planning"
defineProps<{ status?: string; compact?: boolean }>()
</script>

<template>
    <span v-if="status" class="badge" :class="`text-bg-${statusVariant[status] ?? 'secondary'}`" :title="$t(status)">
        <i :class="statusIcon[status]"></i><span :class="compact ? 'd-none d-xl-inline ms-1' : 'ms-1'">{{ $t(status) }}</span>
    </span>
</template>
