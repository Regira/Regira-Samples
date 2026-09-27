<!--
    Touch-friendly category filter: a scrollable row of root chips and, once one is picked, a second row with
    its sub-categories. v-model is the picked category id; "change" also hands over the id-set to filter on
    (the pick + every descendant), which is what the API's CategoryId filter expects.
-->
<script setup lang="ts">
import { computed, onMounted } from "vue"
import type Entity from "../data/Entity"
import useCategoryTree from "../data/useCategoryTree"

const model = defineModel<number | undefined>()
const emit = defineEmits<{ (e: "change", ids: Array<number> | undefined): void }>()
withDefaults(defineProps<{ allLabel?: string }>(), { allLabel: "All" })

const tree = useCategoryTree()
onMounted(() => tree.load())

const selected = computed(() => (model.value != null ? tree.byId.get(Number(model.value)) : undefined))

// first root reached by walking up the (first) parent path
const activeRoot = computed(() => {
    let current = selected.value
    const seen = new Set<number>()
    while (current && !current.$isRoot && !seen.has(current.id)) {
        seen.add(current.id)
        current = tree.parentsOf(current.id)[0]
    }
    return current
})
const subLevel = computed<Array<Entity>>(() => {
    const current = selected.value
    if (!current) return []
    const children = tree.childrenOf(current.id)
    if (children.length) return children
    return current.$isRoot ? [] : tree.childrenOf(tree.parentsOf(current.id)[0]?.id)
})
const subParent = computed(() => {
    const current = selected.value
    if (!current) return undefined
    return tree.childrenOf(current.id).length ? current : tree.parentsOf(current.id)[0]
})

function pick(category?: Entity) {
    const id = category && category.id !== selected.value?.id ? category.id : undefined
    model.value = id
    emit("change", id != null ? tree.expand(id) : undefined)
}
function pickSubParent() {
    if (subParent.value) pick(subParent.value.id === selected.value?.id ? undefined : subParent.value)
}
</script>

<template>
    <div class="sm-chip-filter">
        <div class="sm-chip-row" role="listbox" aria-label="Categories">
            <button type="button" class="sm-chip" :class="{ 'is-selected': !selected }" @click="pick(undefined)">
                <i class="bi bi-grid-3x3-gap"></i> {{ allLabel }}
            </button>
            <button
                v-for="root in tree.roots"
                :key="root.id"
                type="button"
                class="sm-chip"
                :class="{ 'is-selected': activeRoot?.id === root.id }"
                :style="{ '--sm-chip-color': root.color }"
                @click="pick(root)"
            >
                <span class="sm-chip__icon">{{ root.icon }}</span> {{ root.title }}
            </button>
        </div>
        <div v-if="subLevel.length" class="sm-chip-row sm-chip-row--sub">
            <button v-if="subParent" type="button" class="sm-chip sm-chip--sm" :class="{ 'is-selected': selected?.id === subParent.id }" @click="pickSubParent">
                <i class="bi bi-arrow-return-right"></i> {{ allLabel }} {{ subParent.title }}
            </button>
            <button
                v-for="child in subLevel"
                :key="child.id"
                type="button"
                class="sm-chip sm-chip--sm"
                :class="{ 'is-selected': selected?.id === child.id }"
                :style="{ '--sm-chip-color': child.color }"
                @click="pick(child)"
            >
                <span class="sm-chip__icon">{{ child.icon }}</span> {{ child.title }}
            </button>
        </div>
    </div>
</template>
