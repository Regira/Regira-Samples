<script setup lang="ts">
import { computed } from "vue"
import { requestStatusBadge, requestStatusIcon, type RequestStatus } from "@/domain/enums"
const props = defineProps<{ status?: RequestStatus | string }>()
const s = computed(() => (props.status ?? "Draft") as RequestStatus)
</script>
<template>
    <span class="badge rounded-pill" :class="requestStatusBadge[s]"><i :class="requestStatusIcon[s]" class="me-1"></i>{{ $t(`status${s}`) }}</span>
</template>
