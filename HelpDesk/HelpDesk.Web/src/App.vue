<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { useRoute } from "vue-router"
import { Feedback, LoadingContainer } from "@regira/modules/vue/ui"
import { LoginModal, LoginForm, ForgotPasswordModal, useAuthStore } from "@regira/modules/vue/auth"
import ForgotPasswordForm from "@/components/users/ForgotPasswordForm.vue"
import { AppStatus } from "@regira/modules/vue/app"
import TheHeader from "@/components/layout/TheHeader.vue"
import SideNav from "@/components/layout/SideNav.vue"
import TheFooter from "@/components/layout/TheFooter.vue"
import Main from "@/components/layout/Main.vue"
import { useMeStore } from "@/infrastructure/me"

const authStore = useAuthStore()
// on any protected route an unauthenticated visitor gets the sign-in modal immediately, before any 401
const showLogin = computed(() => authStore.isRequired && !authStore.isAuthenticated)

const forgotUsername = ref<string>()
const showForgot = ref(false)
function handleForgotPassword(username?: string) {
    forgotUsername.value = username
    showForgot.value = true
}
watch(showLogin, (gateOpen) => {
    if (!gateOpen) showForgot.value = false
})

useMeStore().init()

// off-canvas sidebar on small screens; closes on every navigation
const sidebarOpen = ref(false)
const route = useRoute()
watch(
    () => route.fullPath,
    () => (sidebarOpen.value = false)
)
</script>

<template>
    <div class="hd-app" :class="{ 'hd-sidebar-open': sidebarOpen }">
        <aside v-if="$auth.enabled && $auth.isAuthenticated" class="hd-sidebar">
            <SideNav />
        </aside>
        <div v-if="sidebarOpen" class="hd-backdrop d-lg-none" @click="sidebarOpen = false"></div>
        <div class="hd-content">
            <header class="hd-topbar"><TheHeader @toggle-sidebar="sidebarOpen = !sidebarOpen" /></header>
            <section class="position-relative overflow-hidden px-3">
                <Feedback :feedback="$feedback" :enable-error-popup="true" />
            </section>
            <main class="hd-main">
                <LoadingContainer :is-loading="$appStatus !== AppStatus.Ready && (!$auth.enabled || $auth.isAuthenticated)">
                    <Main />
                </LoadingContainer>
            </main>
            <footer class="hd-footer"><TheFooter /></footer>
        </div>

        <Teleport to="#loginModal">
            <LoginModal v-if="showLogin && !showForgot" :title="$t('signIn')">
                <p class="text-muted small mb-3">{{ $t("signInIntro") }}</p>
                <LoginForm @forgot-password="handleForgotPassword" />
            </LoginModal>
            <ForgotPasswordModal v-if="showLogin && showForgot" :username="forgotUsername" @close="showForgot = false" v-slot="{ username }">
                <ForgotPasswordForm :username="username" @login="showForgot = false" />
            </ForgotPasswordModal>
        </Teleport>
    </div>
</template>
