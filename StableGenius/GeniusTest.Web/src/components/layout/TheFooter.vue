<script setup lang="ts">
const year = new Date().getFullYear()
const sourceUrl = "https://github.com/Regira/Regira-Samples/tree/master/StableGenius"
</script>
<template>
    <div class="d-flex justify-content-between align-items-center gap-3 w-100 py-2 small">
        <span>&copy; {{ year }} The Very Stable Genius Test™ — staff area</span>
        <span class="d-none d-md-inline">Every answer is great. Every edit, too.</span>
        <a :href="sourceUrl" target="_blank" rel="noopener" class="source-link text-nowrap"><i class="bi bi-github me-1"></i>Source on GitHub</a>
    </div>
</template>

<style scoped>
.source-link {
    color: #8a6a1f;
    text-decoration: none;
}
.source-link:hover {
    color: #2a2214;
    text-decoration: underline;
}
</style>
