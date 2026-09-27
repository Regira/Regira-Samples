<!--
    Sticky "add article" bar at the bottom of the board. Understands a leading quantity/unit
    ("2 kg apples", "6 eggs", "1.5 l milk") and copies the categories of an earlier article with the same name.
-->
<script setup lang="ts">
import { nextTick, ref } from "vue"
import { useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { Entity as ArticleModel, useEntityStore as useArticleStore, type Entity as Article } from "@/entities/articles"

const props = defineProps<{ shoppingListId: number }>()
const emit = defineEmits<{ (e: "added", article: Article): void }>()

const articleStore = useArticleStore()
const feedback = useFeedback({ autoHideDelay: 2500 })
const text = ref("")
const inputEl = ref<HTMLInputElement>()

const UNITS = ["kg", "g", "l", "ml", "cl", "pcs", "pc", "x", "pack", "packs", "box", "bag", "bottle", "bottles", "can", "cans", "jar"]

interface ParsedArticle {
    title: string
    quantity?: number
    unit?: string
}
function parse(input: string): ParsedArticle {
    const match = input.match(/^(\d+(?:[.,]\d+)?)\s*([a-zA-Z]+)?\s+(.+)$/)
    if (!match) return { title: input }
    const quantity = Number(match[1]!.replace(",", "."))
    const unit = match[2]?.toLowerCase()
    if (unit && !UNITS.includes(unit)) return { title: `${unit} ${match[3]}`.trim(), quantity }
    return { title: match[3]!.trim(), quantity, unit: unit === "x" ? undefined : unit }
}

async function add() {
    const value = text.value.trim()
    if (!value || feedback.isPending) return
    const parsed = parse(value)
    feedback.pending("Adding...")
    try {
        const article = new ArticleModel()
        article.shoppingListId = props.shoppingListId
        article.title = parsed.title.charAt(0).toUpperCase() + parsed.title.slice(1)
        article.quantity = parsed.quantity
        article.unit = parsed.unit
        article.isActive = true
        // smart categorize: reuse the categories of the most recent article with this name
        const known = await articleStore.service.search({ q: parsed.title, pageSize: 5, includes: ["Categories"], sortBy: ["CreatedDesc"] })
        const match = known.items.find((x) => x.title.toLowerCase() === parsed.title.toLowerCase()) ?? known.items[0]
        article.categories = match?.categories?.map((x) => ({ categoryId: x.categoryId }))
        if (!article.unit && match?.title.toLowerCase() === parsed.title.toLowerCase()) article.unit = match.unit

        const { saved } = await articleStore.service.save(article)
        feedback.success(`"${saved.title}" added`)
        text.value = ""
        emit("added", saved)
        await nextTick()
        inputEl.value?.focus()
    } catch (ex) {
        console.error(ex)
        feedback.fail("Adding the article failed", toFeedbackError(ex))
    }
}
</script>

<template>
    <form class="sm-quick-add" @submit.prevent="add">
        <div class="sm-quick-add__inner">
            <input
                ref="inputEl"
                v-model="text"
                class="form-control"
                :placeholder="$t('quickAddPlaceholder')"
                enterkeyhint="done"
                autocomplete="off"
                :aria-label="$t('addArticle')"
            />
            <button type="submit" class="sm-fab-sm" :disabled="!text.trim() || feedback.isPending" :aria-label="$t('addArticle')">
                <span v-if="feedback.isPending" class="spinner-border spinner-border-sm"></span>
                <i v-else class="bi bi-plus-lg"></i>
            </button>
        </div>
        <div v-if="feedback.status === 'Failed'" class="sm-quick-add__msg text-danger small">{{ feedback.message }}</div>
        <div v-else-if="feedback.status === 'Success'" class="sm-quick-add__msg text-success small">{{ feedback.message }}</div>
    </form>
</template>
