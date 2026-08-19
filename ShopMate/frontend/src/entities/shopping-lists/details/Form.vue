<template>
    <form @submit.prevent="handleSubmit">
        <div class="row form-toolbar align-items-center mb-3">
            <div class="col col-md-auto order-1">
                <FormButtonsRow
                    :item="item"
                    :readonly="readonly"
                    :feedback="feedback"
                    :show-delete="item?.id > 0"
                    @cancel="handleCancel"
                    @remove="handleRemove"
                    @restore="handleRestore"
                />
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
            <div class="row">
                <div class="col-12 col-sm-6 mb-3">
                    <input v-model="item.ownerName" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('shopper')" />
                </div>
                <div class="col-12 col-sm-6 mb-3">
                    <input v-model="item.description" :readonly="readonly" class="form-control" />
                    <FormLabel :label="$t('description')" />
                </div>
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
            <div class="mb-1">
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
        </FormSection>

        <section v-if="item.id > 0" class="mt-4">
            <h2 class="h6 mb-2">{{ $t("articles") }}</h2>
            <ArticleManager :shopping-list-id="item.id" />
        </section>

        <Debug :modelValue="{ item }" />
    </form>
</template>

<script setup lang="ts">
import { RouterLink, type RouteRecordRaw } from "vue-router"
import { Feedback, FormButtonsRow, FormSection, FormLabel, Icon } from "@regira/modules/vue/ui"
import { Debug } from "@regira/modules/vue/debug"
import { useForm, type FormEmits, formDefaults } from "@regira/modules/vue/entities"
import config from "../config/config"
import Entity from "../data/Entity"
import useEntityStore from "../data/store"
import ArticleManager from "./ArticleManager.vue"

interface Emits extends /* @vue-ignore */ FormEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = withDefaults(
    defineProps<{ modelValue: Entity; readonly?: boolean; overviewUrl?: string | RouteRecordRaw; isPopup?: boolean; initialTab?: string }>(),
    { ...formDefaults }
)

const { item, feedback, handleCancel, handleSubmit, handleRemove, handleRestore } = useForm<Entity>({ entityService: useEntityStore().service, props, emit })

const iconOptions = ["bi-cart4", "bi-basket", "bi-bag", "bi-house", "bi-airplane", "bi-tree", "bi-gift", "bi-balloon", "bi-fire", "bi-cup-hot", "bi-film", "bi-heart"]
const colorOptions = ["#16a34a", "#4caf50", "#ffca28", "#e57373", "#8d6e63", "#4fc3f7", "#ba68c8", "#78909c", "#f06292", "#ffb74d", "#90caf9", "#a1887f"]
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
