<template>
    <form @submit.prevent="handleSubmit">
        <div class="row form-toolbar align-items-center mb-3">
            <div class="col col-md-auto order-1">
                <FormButtonsRow :item="item" :readonly="readonly" :feedback="feedback" :show-delete="item?.id > 0" @cancel="handleCancel" @remove="handleRemove" />
            </div>
            <div class="col-auto order-2 order-md-3">
                <RouterLink v-if="isPopup" :to="{ name: `${config.key}Details`, params: { id: item.$id } }" target="_blank" class="btn btn-outline-secondary" :title="$t('popOut')">
                    <Icon name="popOut" />
                </RouterLink>
                <RouterLink v-else-if="overviewUrl" :to="overviewUrl" class="btn btn-outline-info">
                    <Icon name="list" /> <span class="d-none d-md-inline ms-1">{{ $t("overview") }}</span>
                </RouterLink>
            </div>
            <div class="col-md order-3 order-md-2"><Feedback :feedback="feedback" /></div>
        </div>

        <FormSection :title="$t(config.detailsTitle || '')" :readonly="readonly">
            <div class="mb-3">
                <input v-model="item.title" :readonly="readonly" class="form-control" required />
                <FormLabel :label="$t('name')" />
            </div>

            <div class="mb-3">
                <div class="sm-chip-row">
                    <button
                        v-for="opt in iconOptions"
                        :key="opt"
                        type="button"
                        class="sm-icon-pick"
                        :class="{ 'is-active': item.icon === opt }"
                        :disabled="readonly"
                        @click="item.icon = opt"
                    >
                        <i class="bi" :class="opt"></i>
                    </button>
                </div>
                <FormLabel :label="$t('icon')" />
            </div>

            <div class="mb-3">
                <div class="sm-chip-row">
                    <button
                        v-for="c in colorOptions"
                        :key="c"
                        type="button"
                        class="sm-color-pick"
                        :class="{ 'is-active': item.colorHex === c }"
                        :style="{ '--pick-color': c }"
                        :disabled="readonly"
                        @click="item.colorHex = c"
                    ></button>
                </div>
                <FormLabel :label="$t('color')" />
            </div>

            <div v-if="item.articleCount" class="mb-3 text-muted small">
                <i class="bi bi-basket2 me-1"></i>{{ item.articleCount }} {{ $t("items") }}
            </div>

            <div class="mb-3">
                <div class="sm-chip-row">
                    <button
                        v-for="c in parentChoices"
                        :key="c.id"
                        type="button"
                        class="sm-chip"
                        :class="{ 'is-active': isParentChecked(c.id) }"
                        :disabled="readonly"
                        @click="toggleParent(c)"
                    >
                        <i class="bi" :class="c.icon || 'bi-tag'"></i>{{ c.title }}
                    </button>
                    <span v-if="!parentChoices.length" class="text-muted small">{{ $t("noResults") }}</span>
                </div>
                <FormLabel :label="$t('parentCategories')" />
            </div>

            <div class="mb-3">
                <div class="sm-chip-row">
                    <button
                        v-for="c in childChoices"
                        :key="c.id"
                        type="button"
                        class="sm-chip"
                        :class="{ 'is-active': isChildChecked(c.id) }"
                        :disabled="readonly"
                        @click="toggleChild(c)"
                    >
                        <i class="bi" :class="c.icon || 'bi-tag'"></i>{{ c.title }}
                    </button>
                    <span v-if="!childChoices.length" class="text-muted small">{{ $t("noResults") }}</span>
                </div>
                <FormLabel :label="$t('childCategories')" />
            </div>
        </FormSection>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue"
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity, { type CategoryCore, type RelatedCategoryRef } from "../data/Entity"
import useEntityStore from "../data/store"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { service: entityService } = useEntityStore()
const { item, feedback, handleCancel, handleSubmit, handleRemove } = useForm<Entity>({ entityService, props, emit })

const iconOptions = [
    "bi-basket",
    "bi-apple",
    "bi-egg",
    "bi-bread-slice",
    "bi-egg-fried",
    "bi-snow",
    "bi-cup-straw",
    "bi-house",
    "bi-droplet",
    "bi-cookie",
    "bi-balloon",
    "bi-heart",
    "bi-flower1",
    "bi-tag",
]
const colorOptions = ["#4caf50", "#ffca28", "#e57373", "#8d6e63", "#4fc3f7", "#ba68c8", "#78909c", "#f06292", "#ffb74d", "#90caf9", "#a1887f", "#66bb6a"]

// All other categories, for the parent/child chip pickers — fetched once via the pooled service.
const allCategories = ref<Array<Entity>>([])
onMounted(async () => {
    allCategories.value = await entityService.list({ pageSize: 0 })
})
const otherCategories = computed(() => allCategories.value.filter((c) => c.id !== item.value.id))
const parentChoices = computed<Array<CategoryCore>>(() => otherCategories.value.filter((c) => !isChildChecked(c.id)).map(toCore))
const childChoices = computed<Array<CategoryCore>>(() => otherCategories.value.filter((c) => !isParentChecked(c.id)).map(toCore))
function toCore(c: Entity): CategoryCore {
    return { id: c.id, title: c.title, icon: c.icon, colorHex: c.colorHex }
}

function isParentChecked(id: number) {
    return item.value.parentEntities?.some((x) => x.parentId === id && !x._deleted) ?? false
}
function isChildChecked(id: number) {
    return item.value.childEntities?.some((x) => x.childId === id && !x._deleted) ?? false
}
function toggleParent(c: CategoryCore) {
    const rows = (item.value.parentEntities ??= [])
    const existing = rows.find((x) => x.parentId === c.id)
    if (existing) {
        if (existing.id) existing._deleted = !existing._deleted
        else rows.splice(rows.indexOf(existing), 1)
    } else {
        rows.push({ parentId: c.id, childId: item.value.id, parent: c } satisfies RelatedCategoryRef)
    }
}
function toggleChild(c: CategoryCore) {
    const rows = (item.value.childEntities ??= [])
    const existing = rows.find((x) => x.childId === c.id)
    if (existing) {
        if (existing.id) existing._deleted = !existing._deleted
        else rows.splice(rows.indexOf(existing), 1)
    } else {
        rows.push({ parentId: item.value.id, childId: c.id, child: c } satisfies RelatedCategoryRef)
    }
}
</script>

<style scoped>
.sm-icon-pick,
.sm-color-pick {
    flex: 0 0 auto;
    width: 44px;
    height: 44px;
    border-radius: 50%;
    border: 2px solid transparent;
    background: var(--sm-surface-muted);
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.1rem;
}
.sm-icon-pick.is-active {
    border-color: var(--sm-accent);
    color: var(--sm-accent);
}
.sm-color-pick {
    background: var(--pick-color);
}
.sm-color-pick.is-active {
    border-color: #1f2a24;
}
</style>
