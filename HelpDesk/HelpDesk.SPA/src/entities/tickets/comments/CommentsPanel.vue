<!-- Conversation-style comment thread for one ticket. Comments are NOT synced through Ticket's save path
     (see Ticket.ts / TicketController.cs) - they are posted one at a time through custom
     GET/POST /tickets/{id}/comments endpoints, so this panel talks to the raw axios instance directly
     instead of going through the entity service. -->
<template>
    <div class="comments-panel">
        <div v-if="loading" class="text-muted">{{ $t("search") }}...</div>

        <div v-else class="comment-thread mb-3">
            <p v-if="!comments.length" class="text-muted">{{ $t("noComments") }}</p>
            <div
                v-for="c in comments"
                :key="c.id"
                class="comment-bubble mb-2 p-2 rounded"
                :class="{ 'comment-agent': isAgentAuthor(c), 'comment-internal': c.isInternal }"
            >
                <div class="d-flex justify-content-between small text-muted mb-1">
                    <span>
                        <strong>{{ c.author?.fullName ?? "?" }}</strong>
                        <span v-if="c.author" class="ms-1">({{ $t(c.author.role) }})</span>
                        <span v-if="c.isInternal" class="badge text-bg-warning ms-1">{{ $t("internal") }}</span>
                    </span>
                    <span>{{ format(new Date(c.created), "dd/MM/yyyy HH:mm") }}</span>
                </div>
                <div class="comment-message">{{ c.message }}</div>
            </div>
        </div>

        <div v-if="!readonly" class="new-comment border-top pt-2">
            <div class="row">
                <div class="col-md-4 mb-2">
                    <PersonInputSelector v-model="author" v-model:idValue="authorId" :placeholder="$t('postedBy')" />
                    <FormLabel :label="$t('postedBy')" />
                </div>
                <div class="col-md-8 mb-2">
                    <textarea v-model="message" class="form-control" rows="2" :placeholder="$t('writeComment')"></textarea>
                </div>
            </div>
            <div class="d-flex justify-content-between align-items-center">
                <div class="form-check">
                    <input id="isInternalComment" v-model="isInternal" type="checkbox" class="form-check-input" />
                    <label for="isInternalComment" class="form-check-label">{{ $t("internal") }}</label>
                </div>
                <button type="button" class="btn btn-primary" :disabled="!canPost" @click="postComment">
                    <Icon name="new" /> {{ $t("addComment") }}
                </button>
            </div>
            <Feedback :feedback="feedback" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { Icon, FormLabel, Feedback, useFeedback } from "@regira/modules/vue/ui"
import { useAxios } from "@regira/modules/vue/http"
import { format } from "date-fns"
import { InputSelector as PersonInputSelector, type Entity as Person } from "@/entities/people"
import type { TicketComment } from "../data/Entity"

const props = defineProps<{ ticketId?: number; readonly?: boolean }>()

const axios = useAxios()
const feedback = useFeedback()

const comments = ref<Array<TicketComment>>([])
const loading = ref(false)

const author = ref<Person>()
const authorId = ref<number>()
const message = ref("")
const isInternal = ref(false)

const canPost = computed(() => !!authorId.value && message.value.trim().length > 0)

function isAgentAuthor(c: TicketComment) {
    return c.author?.role === "Agent" || c.author?.role === "Admin"
}

async function load() {
    if (!props.ticketId) {
        comments.value = []
        return
    }
    loading.value = true
    try {
        const { data } = await axios.get(`/tickets/${props.ticketId}/comments`)
        comments.value = data.items ?? []
    } finally {
        loading.value = false
    }
}

async function postComment() {
    if (!props.ticketId || !canPost.value) return
    feedback.pending("Saving...")
    try {
        await axios.post(`/tickets/${props.ticketId}/comments`, {
            authorId: authorId.value,
            message: message.value.trim(),
            isInternal: isInternal.value,
        })
        message.value = ""
        isInternal.value = false
        await load()
        feedback.success("Saved")
    } catch (ex: any) {
        feedback.fail("Save failed", ex.response?.data?.errors ?? ex.response?.data?.error ?? ex.message)
    }
}

onMounted(load)
watch(
    () => props.ticketId,
    () => load()
)
</script>

<style scoped>
.comment-thread {
    max-height: 28rem;
    overflow-y: auto;
}
.comment-bubble {
    background-color: var(--bs-secondary-bg, #f1f1f1);
    max-width: 90%;
}
.comment-bubble.comment-agent {
    background-color: var(--bs-primary-bg-subtle, #cfe2ff);
    margin-left: auto;
}
.comment-bubble.comment-internal {
    background-color: var(--bs-warning-bg-subtle, #fff3cd);
}
.comment-message {
    white-space: pre-wrap;
}
</style>
