<script setup lang="ts">
import { computed, nextTick, ref } from "vue"
import { loadEmojiCatalogue, type EmojiEntry } from "./catalogue"

// Emoji field as one square button showing the emoji; it opens a searchable, grouped grid
// (public/data/emoji-picker.csv). Typing/pasting your own and clearing it happen inside the panel.
// insert: the button is a plain smiley and a pick is emitted (@pick) - for adding emoji to a text.
// align="end": the panel opens right-aligned under the button (when the button sits at the right edge).
const props = defineProps<{ readonly?: boolean; insert?: boolean; align?: "start" | "end" }>()
const emit = defineEmits<{ pick: [emoji: string] }>()
const model = defineModel<string | undefined>()

const open = ref(false)
const search = ref("")
const entries = ref<Array<EmojiEntry>>([])
const failed = ref(false)
const searchInput = ref<HTMLInputElement>()

const groups = computed(() => {
    const term = search.value.trim().toLowerCase()
    const matches = term ? entries.value.filter((e) => e.keywords.toLowerCase().includes(term) || e.group.toLowerCase().includes(term) || e.emoji === term) : entries.value
    const byGroup = new Map<string, Array<EmojiEntry>>()
    for (const e of matches) byGroup.set(e.group, [...(byGroup.get(e.group) ?? []), e])
    return [...byGroup.entries()]
})

async function toggle() {
    open.value = !open.value
    if (!open.value) return
    search.value = ""
    try {
        entries.value = await loadEmojiCatalogue()
        failed.value = false
    } catch (ex) {
        console.error(ex)
        failed.value = true
    }
    await nextTick()
    searchInput.value?.focus()
}
function close() {
    open.value = false
}
function choose(emoji: string | undefined) {
    if (props.insert) {
        if (emoji) emit("pick", emoji)
    } else model.value = emoji
    close()
}
</script>

<template>
    <div class="emoji-picker" :class="{ 'is-insert': insert }" v-click-outside="close" @keydown.esc="close">
        <button
            type="button"
            class="btn btn-outline-secondary emoji-picker__toggle"
            :disabled="readonly"
            :title="insert ? $t('emojiInsert') : $t('emojiPick')"
            :aria-label="insert ? $t('emojiInsert') : model ? `${$t('emoji')}: ${model}` : $t('emojiPick')"
            :aria-expanded="open"
            @click="toggle"
        >
            <span v-if="model && !insert" class="emoji-picker__current">{{ model }}</span>
            <i v-else class="bi bi-emoji-smile text-muted"></i>
        </button>

        <div v-if="open" class="emoji-picker__panel shadow" :class="{ 'is-end': align === 'end' }" role="dialog" :aria-label="$t('emojiPick')">
            <input ref="searchInput" v-model="search" type="search" class="form-control form-control-sm mb-2" :placeholder="$t('emojiSearch')" />
            <div class="emoji-picker__list">
                <p v-if="failed" class="small text-muted mb-0">{{ $t("emojiLoadFailed") }}</p>
                <template v-for="[group, items] in groups" :key="group">
                    <div class="emoji-picker__group">{{ group }}</div>
                    <div class="emoji-picker__grid">
                        <button
                            v-for="e in items"
                            :key="e.emoji"
                            type="button"
                            class="emoji-picker__item"
                            :class="{ 'is-selected': !insert && e.emoji === model }"
                            :title="e.keywords"
                            :aria-label="e.keywords"
                            @click="choose(e.emoji)"
                        >
                            {{ e.emoji }}
                        </button>
                    </div>
                </template>
                <p v-if="!failed && entries.length && !groups.length" class="small text-muted mb-0">{{ $t("emojiNone") }}</p>
            </div>
            <div v-if="!insert" class="emoji-picker__footer">
                <input v-model="model" maxlength="16" class="form-control form-control-sm emoji-picker__own" :placeholder="$t('emojiOwn')" :aria-label="$t('emojiOwn')" />
                <button v-if="model" type="button" class="btn btn-sm btn-outline-secondary text-nowrap" @click="choose(undefined)">{{ $t("emojiClear") }}</button>
            </div>
        </div>
    </div>
</template>

<style scoped>
.emoji-picker {
    position: relative;
    display: block; /* its own line, under an inline FormLabel - like a form-control */
    width: max-content;
}
.emoji-picker__toggle {
    width: 3.4rem;
    height: calc(1.5em + 0.75rem + 2px); /* same height as a form-control */
    padding: 0;
}
/* next to a large (fs-5) text field */
.emoji-picker.emoji-large .emoji-picker__toggle {
    height: calc(1.5 * 1.25rem + 0.75rem + 2px);
}
.emoji-picker__current {
    font-size: 1.5rem;
    line-height: 1;
}
.emoji-picker__panel {
    position: absolute;
    top: calc(100% + 0.35rem);
    left: 0;
    z-index: 1050;
    width: 21rem;
    max-width: calc(100vw - 32px);
    padding: 0.6rem;
    background: #fff;
    border: 1px solid #c9a227;
    outline: 1px solid #c9a227;
    outline-offset: 3px;
    border-radius: 4px;
}
.emoji-picker__panel.is-end {
    left: auto;
    right: 0;
}
/* insert mode: a small button next to a label */
.emoji-picker.is-insert .emoji-picker__toggle {
    width: 2.4rem;
    height: 2rem;
}
.emoji-picker__list {
    max-height: 17rem;
    overflow-y: auto;
}
.emoji-picker__footer {
    display: flex;
    gap: 0.4rem;
    margin-top: 0.5rem;
    padding-top: 0.5rem;
    border-top: 1px solid #f0e3b5;
}
.emoji-picker__own {
    flex: 1;
}
.emoji-picker__group {
    font-family: "Cinzel", Georgia, serif;
    font-size: 0.72rem;
    font-weight: 700;
    color: #8a6a1f;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin: 0.4rem 0 0.2rem;
}
.emoji-picker__grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(2.2rem, 1fr));
    gap: 0.15rem;
}
.emoji-picker__item {
    font-size: 1.35rem;
    line-height: 1;
    padding: 0.3rem 0;
    background: none;
    border: 1px solid transparent;
    border-radius: 4px;
    cursor: pointer;
}
.emoji-picker__item:hover,
.emoji-picker__item:focus-visible {
    background: #fff3c4;
    border-color: #c9a227;
}
.emoji-picker__item.is-selected {
    background: #e9c85a;
    border-color: #8a6a1f;
}
</style>
