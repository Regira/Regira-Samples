<!-- Participants of one event (administrators): a paged, searchable view on /registrations?eventId=…
     Deep imports into the registrations slice: that slice imports this one's barrel, so this edge must not. -->
<template>
    <section>
        <div class="d-flex flex-wrap gap-2 align-items-center mb-2">
            <div class="input-group w-auto flex-grow-1" style="max-width: 24rem">
                <span class="input-group-text"><i class="bi bi-search"></i></span>
                <input v-model.lazy.trim="searchObject.q" class="form-control" :placeholder="$t('keywords')" @change="searchHandler(true)" />
            </div>
            <select v-model="searchObject.status" class="form-select w-auto" @change="searchHandler(true)">
                <option :value="undefined">{{ $t("status") }}: {{ $t("all") }}</option>
                <option v-for="s in statuses" :key="s" :value="s">{{ $t(s) }}</option>
            </select>
            <RegistrationButton class="btn btn-info ms-auto" :item-defaults="{ eventId }" :close-on-save="true" @save="searchHandler(false)">
                <i class="bi bi-person-plus me-1"></i>{{ $t("register") }}
            </RegistrationButton>
        </div>
        <Feedback :feedback="feedback" :hide-close-button="true" />
        <LoadingContainer :is-loading="isLoading">
            <p v-if="items && !items.length" class="italic-muted">{{ $t("noResults") }}</p>
            <div class="entity-list">
                <div v-for="r in items ?? []" :key="r.$id" class="row border-bottom py-2 align-items-center">
                    <div class="col-auto">
                        <RegistrationButton :model-value="r" @save="searchHandler(false)" @remove="searchHandler(false)">
                            <i class="bi bi-pencil-square"></i>
                        </RegistrationButton>
                    </div>
                    <div class="col text-truncate">
                        <span class="ep-avatar ep-avatar-sm ep-avatar-employee me-2">{{ initials(r) }}</span>{{ r.user?.firstName }} {{ r.user?.lastName }}
                        <small class="text-muted d-none d-md-inline ms-1">{{ r.user?.department }}</small>
                    </div>
                    <div class="col-auto"><span class="badge ep-status" :class="`ep-status-${r.status}`">{{ $t(r.status) }}</span></div>
                    <div class="col-2 d-none d-md-block small text-muted"><i class="bi bi-collection me-1"></i>{{ r.sessions?.length ?? 0 }}</div>
                </div>
            </div>
            <Paging
                v-if="pagingInfo && (itemsCount ?? 0) > (pagingInfo.pageSize ?? 0)"
                class="mt-2"
                v-model="pagingInfo"
                :count="itemsCount || 0"
                :button-type="ButtonType.button"
                @change="searchHandler(false)"
            />
        </LoadingContainer>
    </section>
</template>

<script setup lang="ts">
import { Feedback, LoadingContainer, Paging, ButtonType } from "@regira/modules/vue/ui"
import { useSearchView } from "@regira/modules/vue/entities"
import { onAuthenticated } from "@regira/modules/vue/auth"
import useRegistrationStore from "@/entities/registrations/data/store"
import Registration, { RegistrationStatus } from "@/entities/registrations/data/Entity"
import RegistrationSearchObject from "@/entities/registrations/filter/SearchObject"
import RegistrationButton from "@/entities/registrations/details/FormModalButton.vue"

const props = defineProps<{ eventId: number }>()

const statuses = Object.values(RegistrationStatus)
const { service } = useRegistrationStore()
const { searchObject, pagingInfo, items, itemsCount, isLoading, feedback, searchHandler } = useSearchView<Registration, RegistrationSearchObject>({
    service,
    searchObject: Object.assign(new RegistrationSearchObject(), { eventId: props.eventId, sortBy: ["Employee"] }),
    defaultPageSize: 25,
})
const initials = (r: Registration) => `${r.user?.firstName?.[0] ?? ""}${r.user?.lastName?.[0] ?? ""}`
onAuthenticated(() => searchHandler(true))
</script>
