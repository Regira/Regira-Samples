<script setup lang="ts"></script>

<template>
  <div class="ws-container ws-notfound">
    <i class="bi bi-signpost-split"></i>
    <h1 class="display-font">Page not found</h1>
    <p class="ws-muted">The page you're looking for doesn't exist or may have moved.</p>
    <RouterLink :to="{ name: 'home' }" class="btn-ws btn-ws-primary">Back to home</RouterLink>
  </div>
</template>

<style scoped>
.ws-notfound {
  text-align: center;
  padding: 6rem 1.5rem;
}
.ws-notfound i {
  font-size: 3rem;
  color: var(--ws-muted);
}
.ws-notfound h1 {
  font-size: 1.9rem;
  margin: 1rem 0 0.5rem;
}
.ws-notfound p {
  margin-bottom: 1.5rem;
}
</style>
