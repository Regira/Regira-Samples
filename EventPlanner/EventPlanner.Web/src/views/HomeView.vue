<script setup lang="ts">
import { computed, ref } from "vue"
import { RouterLink } from "vue-router"
import { LoadingContainer } from "@regira/modules/vue/ui"
import { onAuthenticated, getAccountName } from "@regira/modules/vue/auth"
import { useEntityStore as useEventStore, EventCard } from "@/entities/events"
import type { Entity as EventItem } from "@/entities/events"
import { useEntityStore as useRegistrationStore } from "@/entities/registrations"
import type { Entity as Registration } from "@/entities/registrations"
import { useEntityStore as useSpeakerStore } from "@/entities/speakers"
import { Dashboard } from "@/components/entity-navigation"
import { useAccess } from "@/access"

const { isAdmin } = useAccess()
const eventStore = useEventStore()
const registrationStore = useRegistrationStore()
const speakerStore = useSpeakerStore()

const featured = ref<Array<EventItem>>([])
const upcoming = ref<Array<EventItem>>([])
const upcomingCount = ref<number>()
const myNext = ref<Array<Registration>>([])
const myCount = ref<number>()
const speakerCount = ref<number>()
const isLoading = ref(true)

const hero = computed(() => featured.value[0])
const accountName = computed(() => getAccountName())

async function load() {
    isLoading.value = true
    try {
        const [f, u, mine, sp] = await Promise.all([
            eventStore.service.list({ isFeatured: true, upcoming: true, status: "Published", pageSize: 4, sortBy: ["StartDate"] }),
            eventStore.service.search({ upcoming: true, status: "Published", pageSize: 8, sortBy: ["StartDate"] }),
            registrationStore.service.search({ upcoming: true, status: ["Confirmed", "Waitlisted"], pageSize: 5, sortBy: ["EventDate"] }),
            speakerStore.service.search({ pageSize: 1 }),
        ])
        featured.value = f
        upcoming.value = u.items
        upcomingCount.value = u.count
        myNext.value = isAdmin() ? [] : mine.items
        myCount.value = mine.count
        speakerCount.value = sp.count
    } finally {
        isLoading.value = false
    }
}
onAuthenticated(load)
</script>

<template>
    <section class="ep-home">
        <!-- hero: the next featured event, or a generic welcome -->
        <div class="ep-hero ep-home-hero" :style="hero?.$bannerStyle">
            <div class="ep-hero-inner">
                <p class="ep-eyebrow mb-1"><i class="bi bi-stars me-1"></i>{{ accountName ? `Hi ${accountName.split(" ")[0]}!` : $t("featuredEvent") }}</p>
                <h1 class="ep-hero-title">{{ $t("heroTitle") }}</h1>
                <p class="ep-hero-summary">{{ $t("heroSubtitle") }}</p>
                <div class="d-flex flex-wrap gap-2">
                    <RouterLink :to="{ name: 'EventItemOverview', query: { upcoming: 'true' } }" class="btn btn-light btn-lg fw-semibold">
                        <i class="bi bi-calendar-event me-2"></i>{{ $t("browseEvents") }}
                    </RouterLink>
                    <RouterLink v-if="hero" :to="{ name: 'EventItemDetails', params: { id: hero.id } }" class="btn btn-outline-light btn-lg">
                        <i class="bi bi-star-fill me-2 text-warning"></i>{{ hero.title }}
                    </RouterLink>
                </div>
            </div>
        </div>

        <div class="row g-3 my-3">
            <div class="col-4">
                <div class="ep-stat" style="--ep-cat: #7c3aed">
                    <i class="bi bi-calendar2-week"></i><strong>{{ upcomingCount ?? "-" }}</strong><span>{{ $t("statsEvents") }}</span>
                </div>
            </div>
            <div class="col-4">
                <RouterLink :to="{ name: 'RegistrationOverview' }" class="ep-stat text-decoration-none" style="--ep-cat: #ec4899">
                    <i class="bi bi-ticket-perforated"></i><strong>{{ myCount ?? "-" }}</strong><span>{{ isAdmin() ? $t("registrations") : $t("myRegistrations") }}</span>
                </RouterLink>
            </div>
            <div class="col-4">
                <RouterLink :to="{ name: 'SpeakerOverview' }" class="ep-stat text-decoration-none" style="--ep-cat: #f97316">
                    <i class="bi bi-mic"></i><strong>{{ speakerCount ?? "-" }}</strong><span>{{ $t("statsSpeakers") }}</span>
                </RouterLink>
            </div>
        </div>

        <LoadingContainer :is-loading="isLoading">
            <section v-if="myNext.length" class="mb-4">
                <h3 class="ep-section-title"><i class="bi bi-ticket-perforated me-2"></i>{{ $t("myRegistrations") }}</h3>
                <div class="list-group ep-my-list">
                    <RouterLink
                        v-for="r in myNext"
                        :key="r.id"
                        :to="{ name: 'EventItemDetails', params: { id: r.eventId } }"
                        class="list-group-item list-group-item-action d-flex align-items-center gap-3"
                    >
                        <span class="ep-date-chip small-chip">
                            <span class="day">{{ r.event?.startDate?.substring(8, 10) }}</span>
                            <span class="month">{{ r.event?.startDate?.substring(5, 7) }}/{{ r.event?.startDate?.substring(2, 4) }}</span>
                        </span>
                        <span class="flex-grow-1 min-w-0 text-truncate fw-semibold">{{ r.event?.title }}</span>
                        <span class="badge ep-status" :class="`ep-status-${r.status}`">{{ $t(r.status) }}</span>
                    </RouterLink>
                </div>
            </section>

            <section v-if="featured.length" class="mb-4">
                <h3 class="ep-section-title"><i class="bi bi-star me-2"></i>{{ $t("featured") }}</h3>
                <div class="row row-cols-1 row-cols-md-2 row-cols-xl-4 g-3">
                    <div v-for="ev in featured" :key="ev.id" class="col"><EventCard :event="ev" /></div>
                </div>
            </section>

            <section class="mb-4">
                <div class="d-flex justify-content-between align-items-end">
                    <h3 class="ep-section-title"><i class="bi bi-calendar-event me-2"></i>{{ $t("upcomingEvents") }}</h3>
                    <RouterLink :to="{ name: 'EventItemOverview', query: { upcoming: 'true' } }" class="small mb-3">{{ $t("browseEvents") }} &rarr;</RouterLink>
                </div>
                <div class="row row-cols-1 row-cols-sm-2 row-cols-lg-4 g-3">
                    <div v-for="ev in upcoming" :key="ev.id" class="col"><EventCard :event="ev" /></div>
                </div>
            </section>
        </LoadingContainer>

        <section v-if="isAdmin()" class="mt-5">
            <h3 class="ep-section-title"><i class="bi bi-sliders me-2"></i>{{ $t("manage") }}</h3>
            <Dashboard />
        </section>
    </section>
</template>
