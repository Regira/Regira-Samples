<template>
    <section class="overview">
        <div class="row justify-content-between gx-0 gx-sm-1">
            <div class="col col-lg-auto order-1">
                <!-- Filter -->
                <Filter v-model="searchObject" @filter="updateOverviewRoute(true)" @change="updateOverviewRoute(true)" :result-count="itemsCount">
                    <template #adv="{ handleClose }">
                        <slot
                            name="filterAdv"
                            :result-count="itemsCount"
                            :search="updateOverviewRoute"
                            :search-object="searchObject"
                            :paging-info="pagingInfo"
                            :handle-close="handleClose"
                        >
                            <component
                                :is="FilterAdv"
                                v-model="searchObject"
                                :result-count="itemsCount"
                                @filter="updateOverviewRoute(true)"
                                @change="updateOverviewRoute(true)"
                                @close="handleClose"
                            />
                        </slot>
                    </template>
                </Filter>
            </div>
            <div class="col-12 col-lg order-3 order-lg-2">
                <Feedback v-bind="{ feedback }" :hideCloseButton="true" />
            </div>
            <div v-if="canWrite(config.key)" class="col-auto order-2 order-lg-3 ps-2">
                <template v-if="config.isComplex">
                    <RouterLink :to="{ name: config.key + 'Details', params: { id: 'new' } }" class="btn btn-info">
                        <Icon name="new" /><span class="d-none d-sm-inline ms-1">{{ $t("new") }}</span>
                    </RouterLink>
                </template>
                <template v-else>
                    <FormModalButton class="btn btn-info" @save="searchHandler(false)">
                        <Icon name="new" /><span class="d-none d-sm-inline ms-1">{{ $t("new") }}</span>
                    </FormModalButton>
                </template>
            </div>
        </div>

        <!-- Quick status filter (color-coded) + view switch -->
        <div class="d-flex flex-wrap align-items-center gap-2 mt-2">
            <div class="d-flex flex-wrap gap-1 flex-grow-1 ah-status-chips">
                <button type="button" class="btn btn-sm ah-chip" :class="{ active: !searchObject.statusId }" @click="setStatus(undefined)">
                    {{ $t("allStatuses") }}
                </button>
                <button
                    v-for="s in statuses"
                    :key="s.id"
                    type="button"
                    class="btn btn-sm ah-chip"
                    :class="{ active: searchObject.statusId == s.id }"
                    :style="{ '--ah-status-color': s.color }"
                    @click="setStatus(s.id)"
                >
                    <span class="ah-status__dot"></span>{{ s.title }}
                </button>
            </div>
            <div class="btn-group btn-group-sm" role="group" :aria-label="$t('view')">
                <button type="button" class="btn btn-outline-secondary" :class="{ active: viewMode === 'table' }" :title="$t('tableView')" @click="setViewMode('table')">
                    <i class="bi bi-list-ul"></i>
                </button>
                <button type="button" class="btn btn-outline-secondary" :class="{ active: viewMode === 'cards' }" :title="$t('cardView')" @click="setViewMode('cards')">
                    <i class="bi bi-grid-3x2-gap"></i>
                </button>
            </div>
        </div>

        <!-- Paging - ResultSummary -->
        <div class="row">
            <div class="col order-2">
                <template v-if="pagingInfo != null">
                    <Paging
                        class="mt-2"
                        v-show="!isLoading && itemsCount != null && itemsCount > pagingInfo.pageSize!"
                        v-model="pagingInfo"
                        :count="itemsCount || 0"
                        @change="updateOverviewRoute(false)"
                    />
                </template>
            </div>
            <div class="col-12 col-sm-auto order-1 order-sm-3">
                <ResultSummary v-if="items?.length" :visibleCount="items.length" :totalCount="itemsCount" />
            </div>
        </div>

        <!-- List - Loading -->
        <LoadingContainer :is-loading="isLoading">
            <component
                :is="viewMode === 'cards' ? Cards : List"
                :readonly="!canWrite(config.key)"
                v-if="items && items.length > 0"
                v-model="items"
                @request-save="handleRequestSave"
                @request-remove="handleRequestRemove"
                @save="handleSave"
                @remove="handleRemove"
                @request-reload="updateOverviewRoute(false)"
            />
            <p v-if="items && items.length <= 0" class="italic-muted">
                {{ $t("noResults") }}
            </p>
        </LoadingContainer>

        <!-- Paging -->
        <template v-if="pagingInfo != null">
            <Paging
                class="mt-2"
                v-show="!isLoading && itemsCount != null && itemsCount > pagingInfo.pageSize!"
                v-model="pagingInfo"
                :count="itemsCount || 0"
                @change="updateOverviewRoute(false)"
            />
        </template>

        <Debug
            :modelValue="{
                pagingInfo,
                items,
            }"
        />
    </section>
</template>

<script setup lang="ts">
import { useSearchView, useRouteOverview, type OverviewEmits } from "@regira/modules/vue/entities"
import { Icon, Paging, LoadingContainer, Feedback, ResultSummary } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { onAuthenticated } from "@regira/modules/vue/auth"
import { useAccess } from "@/infrastructure/access"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import SearchObject from "../filter/SearchObject"
import Filter from "../filter/Filter.vue"
import FilterAdv from "../filter/FilterAdv.vue"
import List from "./List.vue"
import Cards from "./Cards.vue"
import { ref } from "vue"
import { useEntityStore as useAssetStatusStore } from "@/entities/asset-statuses"
import type { Entity as AssetStatus } from "@/entities/asset-statuses"
import FormModalButton from "../details/FormModalButton.vue"

interface Emits extends /* @vue-ignore */ OverviewEmits<Entity> {}
defineEmits<Emits>()

const { canWrite } = useAccess()
const { service } = useEntityStore()

const { searchObject, pagingInfo, items, itemsCount, isLoading, feedback, applySave, applyRemove, handleSave, handleRemove, searchHandler } =
    useSearchView<Entity, SearchObject>({
        service,
        searchObject: new SearchObject(),
        defaultPageSize: config.defaultPageSize,
    })
const { updateOverviewRoute } = useRouteOverview({
    searchObject,
    pagingInfo,
    handler: searchHandler,
    defaultPageSize: config.defaultPageSize,
})

// re-search whenever a token arrives — sign-in, refresh, or one restored from storage on a hard reload.
// immediate: false — useRouteOverview already fetches on mount, and firing in setup would race it with an
// unpopulated search object. No-auth app: delete this line AND its import above (scaffold.mjs --no-auth strips both; see entities.setup.md#running-without-authentication)
onAuthenticated(() => searchHandler(false), { immediate: false })

// Status chips: the admin-managed statuses (a handful of rows), loaded through the pooled store
const { service: statusService } = useAssetStatusStore()
const statuses = ref<Array<AssetStatus>>([])
onAuthenticated(async () => {
    statuses.value = (await statusService.list({ pageSize: 0 })) ?? []
})
function setStatus(id?: number) {
    searchObject.value.statusId = id
    updateOverviewRoute(true)
}

// Table (compact) or cards - remembered per browser
const VIEW_KEY = "assethub.assets.view"
const viewMode = ref<"table" | "cards">(readViewMode())
function readViewMode(): "table" | "cards" {
    try {
        return localStorage.getItem(VIEW_KEY) === "cards" ? "cards" : "table"
    } catch {
        return "table"
    }
}
function setViewMode(mode: "table" | "cards") {
    viewMode.value = mode
    try {
        localStorage.setItem(VIEW_KEY, mode)
    } catch {
        /* storage unavailable */
    }
}

async function handleRequestSave(item: Entity) {
    const result = await applySave(item)
    if (result != null) {
        handleSave(result)
    }
}
async function handleRequestRemove(item: Entity) {
    // Guard on the result, exactly like save: a delete the server refused (409 while the row is still
    // referenced) would otherwise show the failure AND remove the row until the next fetch.
    if (await applyRemove(item)) {
        handleRemove(item)
    }
}
</script>
