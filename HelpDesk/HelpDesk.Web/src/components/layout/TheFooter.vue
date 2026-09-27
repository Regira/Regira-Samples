<script setup lang="ts">
const year = new Date().getFullYear()
</script>
<template>
    <div class="d-flex justify-content-between flex-wrap gap-2 w-100 py-2 px-3 text-muted small">
        <span>&copy; {{ year }} HelpDesk support center</span>
        <span>Built on Regira Entities &amp; @regira/modules</span>
    </div>
</template>
