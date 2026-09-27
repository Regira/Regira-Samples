<script setup lang="ts">
import { computed, ref } from "vue"
import { onAuthenticated } from "@regira/modules/vue/auth"
import { Feedback, LoadingContainer, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useLang } from "@regira/modules/vue/lang"
import { formatDate } from "@regira/modules/vue/formatters"
import { useEntityStore as useTicketStore, type Entity as Ticket } from "@/entities/tickets"
import ColorBadge from "@/components/tickets/ColorBadge.vue"
import { useMeStore } from "@/infrastructure/me"

// customer self-service portal: open requests first, then recently closed ones (row-scoped by the API)
const { service } = useTicketStore()
const meStore = useMeStore()
const { translate } = useLang()
const feedback = useFeedback()
const open = ref<Array<Ticket>>([])
const openCount = ref(0)
const recent = ref<Array<Ticket>>([])
const isLoading = ref(false)

async function load() {
    isLoading.value = true
    try {
        const [o, c] = await Promise.all([
            service.search({ isClosed: false, pageSize: 10, sortBy: ["LastActivity"] }),
            service.search({ isClosed: true, pageSize: 5, sortBy: ["LastActivity"] }),
        ])
        open.value = o.items
        openCount.value = o.count ?? o.items.length
        recent.value = c.items
    } catch (ex: any) {
        console.error("Loading your tickets failed", ex)
        feedback.fail(translate("loadFailed"), toFeedbackError(ex) ?? ex?.message)
    } finally {
        isLoading.value = false
    }
}
onAuthenticated(() => load())
const name = computed(() => meStore.me?.person?.givenName)
</script>

<template>
    <section>
        <div class="hd-card p-4 mb-3 d-flex flex-wrap align-items-center justify-content-between gap-3">
            <div>
                <h1 class="hd-page-title mb-1">{{ name ? $t("helloName", { name }) : $t("welcome") }}</h1>
                <div class="text-muted">{{ $t("customerIntro") }}</div>
            </div>
            <router-link :to="{ name: 'TicketDetails', params: { id: 'new' } }" class="btn btn-primary">
                <i class="bi bi-plus-lg me-1"></i>{{ $t("newTicket") }}
            </router-link>
        </div>
        <Feedback :feedback="feedback" />

        <LoadingContainer :is-loading="isLoading">
            <div class="row g-3">
                <div class="col-lg-7">
                    <div class="hd-card p-3 h-100">
                        <h2 class="h6 d-flex justify-content-between">
                            <span>{{ $t("openRequests") }}</span><span class="badge text-bg-primary">{{ openCount }}</span>
                        </h2>
                        <p v-if="!open.length" class="text-muted fst-italic">{{ $t("noOpenRequests") }}</p>
                        <router-link
                            v-for="t in open"
                            :key="t.id"
                            :to="{ name: 'TicketDetails', params: { id: t.id } }"
                            class="d-block border-bottom py-2 text-decoration-none text-body"
                        >
                            <div class="d-flex justify-content-between gap-2">
                                <span class="text-truncate fw-semibold">{{ t.title }}</span>
                                <ColorBadge :title="t.status?.title" :color="t.status?.color" solid />
                            </div>
                            <div class="small text-muted">
                                <span class="hd-code me-2">{{ t.code }}</span>{{ $t("updated") }} {{ formatDate(t.lastModified ?? t.created) }}
                                <span v-if="t.commentCount" class="ms-2"><i class="bi bi-chat-dots me-1"></i>{{ t.commentCount }}</span>
                            </div>
                        </router-link>
                        <router-link :to="{ name: 'TicketOverview' }" class="btn btn-link px-0 mt-2">{{ $t("allMyTickets") }} <i class="bi bi-arrow-right"></i></router-link>
                    </div>
                </div>
                <div class="col-lg-5">
                    <div class="hd-card p-3 h-100">
                        <h2 class="h6">{{ $t("recentlyResolved") }}</h2>
                        <p v-if="!recent.length" class="text-muted fst-italic">-</p>
                        <router-link
                            v-for="t in recent"
                            :key="t.id"
                            :to="{ name: 'TicketDetails', params: { id: t.id } }"
                            class="d-block border-bottom py-2 text-decoration-none text-body small"
                        >
                            <i class="bi bi-check2-circle text-success me-1"></i>{{ t.title }}
                            <span class="text-muted d-block">{{ t.code }} · {{ formatDate(t.closedAt ?? t.lastModified) }}</span>
                        </router-link>
                    </div>
                </div>
            </div>
        </LoadingContainer>
    </section>
</template>
