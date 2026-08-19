<template>
    <RouterLink :to="{ name: config.key + 'Details', params: { id: item.$id } }" class="event-card card h-100 text-decoration-none text-reset shadow-sm">
        <div class="event-banner" :style="item.bannerImageUrl ? { backgroundImage: `url(${item.bannerImageUrl})` } : undefined">
            <span v-if="item.isFeatured" class="badge text-bg-warning event-featured-badge"><Icon name="bi bi-star-fill" class="me-1" />{{ $t("featured") }}</span>
            <span
                v-if="item.eventCategory"
                class="badge event-category-badge"
                :style="{ backgroundColor: item.eventCategory.colorHex || '#6c757d' }"
            >
                {{ item.eventCategory.title }}
            </span>
            <div class="event-date-chip">
                <div class="event-date-month">{{ monthAbbr(item.startDate) }}</div>
                <div class="event-date-day">{{ formatDateTime(item.startDate, "d") }}</div>
            </div>
        </div>
        <div class="card-body d-flex flex-column">
            <h3 class="h6 mb-1 text-truncate">{{ item.$title }}</h3>
            <div class="small text-muted mb-2 text-truncate">
                <Icon name="bi bi-geo-alt-fill" class="me-1" />{{ item.location?.title }}
                <span v-if="item.location?.city"> · {{ item.location.city }}</span>
            </div>
            <div class="small text-muted mb-2">
                <Icon name="bi bi-calendar-week" class="me-1" />{{ formatDate(item.startDate) }}
                <template v-if="item.endDate && formatDate(item.endDate) !== formatDate(item.startDate)"> – {{ formatDate(item.endDate) }}</template>
            </div>
            <div class="mt-auto d-flex justify-content-between align-items-center small">
                <span class="text-muted"><Icon name="bi bi-mic-fill" class="me-1" />{{ item.sessionCount ?? 0 }} {{ $t("sessions") }}</span>
                <span class="text-muted"><Icon name="bi bi-people-fill" class="me-1" />{{ item.registrationCount ?? 0 }} {{ $t("registered") }}</span>
            </div>
        </div>
        <div class="card-footer bg-transparent border-0 pt-0 d-flex justify-content-end">
            <ConfirmButton icon="delete" :modal-type="ModalType.danger" @confirm.stop.prevent="$emit('request-remove', item)" @click.stop.prevent>
                {{ $t("deleteItem", { title: item?.$title }) }}
            </ConfirmButton>
        </div>
    </RouterLink>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import { ModalType, ConfirmButton, Icon } from "@regira/modules/vue/ui"
import { formatDate, formatDateTime } from "@regira/modules/vue/formatters"
import type { SaveResult } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"

const emit = defineEmits<{
    (e: "update:modelValue", value: Entity): void
    (e: "save", value: SaveResult<Entity>): void
    (e: "remove", value: Entity): void
    (e: "request-save", value: Entity): void
    (e: "request-remove", value: Entity): void
}>()
defineProps<{ readonly?: boolean }>()

const item = defineModel<Entity>({ required: true })
// formatDateTime's mask has no textual-month token (d/dd, M/MM, yy/yyyy, h/m only) — compose the
// 3-letter month abbreviation via Intl instead.
function monthAbbr(date?: Date | string) {
    if (!date) return ""
    const d = new Date(date)
    return Number.isNaN(d.getTime()) ? "" : new Intl.DateTimeFormat(undefined, { month: "short" }).format(d)
}
</script>

<style scoped>
.event-card {
    overflow: hidden;
    border: none;
    transition:
        transform 0.15s ease,
        box-shadow 0.15s ease;
}
.event-card:hover {
    transform: translateY(-4px);
    box-shadow: 0 0.75rem 1.5rem rgba(0, 0, 0, 0.12) !important;
}
.event-banner {
    height: 140px;
    background-size: cover;
    background-position: center;
    background-color: var(--rg-accent, #4361ee);
    background-image: linear-gradient(135deg, var(--rg-accent, #4361ee), #7209b7);
    position: relative;
}
.event-featured-badge {
    position: absolute;
    top: 0.5rem;
    left: 0.5rem;
}
.event-category-badge {
    position: absolute;
    top: 0.5rem;
    right: 0.5rem;
    color: #fff;
    font-size: 0.7rem;
}
.event-date-chip {
    position: absolute;
    bottom: -18px;
    left: 0.75rem;
    background: #fff;
    border-radius: 0.5rem;
    width: 48px;
    text-align: center;
    box-shadow: 0 0.25rem 0.5rem rgba(0, 0, 0, 0.15);
    overflow: hidden;
}
.event-date-month {
    background: var(--bs-danger);
    color: #fff;
    font-size: 0.65rem;
    text-transform: uppercase;
    font-weight: 700;
    padding: 1px 0;
}
.event-date-day {
    font-size: 1.1rem;
    font-weight: 700;
    line-height: 1.3;
    color: #212529;
}
.card-body {
    padding-top: 1.5rem;
}
</style>
