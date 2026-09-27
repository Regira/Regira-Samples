import { computed, ref } from "vue"
import { defineStore } from "pinia"
import type Entity from "./Entity"
import useEntityStore from "./store"

/**
 * The whole category DAG (reference data, a few dozen rows) loaded once, with helpers to walk it.
 * A category may have several parents, so "descendants" is a graph walk with a visited-set.
 */
export const useCategoryTree = defineStore("categoryTree", () => {
    const store = useEntityStore()
    const items = ref<Array<Entity>>([])
    const isLoaded = ref(false)
    let loading: Promise<void> | undefined

    const byId = computed(() => new Map(items.value.map((x) => [x.id, x])))
    const roots = computed(() => items.value.filter((x) => x.$isRoot).sort(byTitle))

    function childrenOf(id?: number): Array<Entity> {
        if (id == null) return roots.value
        const category = byId.value.get(id)
        return (category?.childEntities ?? [])
            .map((x) => byId.value.get(x.childId))
            .filter((x): x is Entity => x != null)
            .sort(byTitle)
    }
    function parentsOf(id?: number): Array<Entity> {
        const category = id != null ? byId.value.get(id) : undefined
        return (category?.parentEntities ?? []).map((x) => byId.value.get(x.parentId)).filter((x): x is Entity => x != null)
    }
    /** The category itself first, then every descendant (any depth, any parent path). */
    function expand(id: number): Array<number> {
        const result: Array<number> = []
        const visit = (current: number) => {
            if (result.includes(current)) return
            result.push(current)
            childrenOf(current).forEach((x) => visit(x.id))
        }
        visit(id)
        return result
    }

    function load(force = false) {
        if (!force && (isLoaded.value || loading)) return loading ?? Promise.resolve()
        loading = store.service
            .list({ pageSize: 0 })
            .then((result) => {
                items.value = store.fromPool(result)
                isLoaded.value = true
            })
            .finally(() => (loading = undefined))
        return loading
    }

    return { items, isLoaded, byId, roots, childrenOf, parentsOf, expand, load }
})

function byTitle(a: Entity, b: Entity) {
    return a.title.localeCompare(b.title)
}

export default useCategoryTree
