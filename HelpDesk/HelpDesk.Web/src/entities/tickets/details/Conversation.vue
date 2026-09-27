<template>
    <div>
        <div class="hd-thread mb-3">
            <!-- the ticket description opens the thread, as the customer's first message -->
            <div class="hd-msg hd-msg-customer">
                <span class="hd-avatar hd-avatar-customer">{{ initials(ticket.customer) }}</span>
                <div>
                    <div class="hd-msg-meta">
                        <strong>{{ personName(ticket.customer) }}</strong> · {{ formatDateTime(ticket.created, dateMask) }}
                    </div>
                    <div class="hd-bubble">{{ ticket.description || $t("noDescription") }}</div>
                </div>
            </div>

            <div
                v-for="c in comments"
                :key="c.id"
                class="hd-msg"
                :class="{ 'hd-msg-staff': isStaffAuthor(c), 'hd-msg-customer': !isStaffAuthor(c), 'hd-msg-internal': c.isInternal }"
            >
                <span class="hd-avatar" :class="isStaffAuthor(c) ? 'hd-avatar-staff' : 'hd-avatar-customer'">{{ initials(c.author) }}</span>
                <div>
                    <div class="hd-msg-meta">
                        <strong>{{ personName(c.author) }}</strong>
                        <span v-if="c.author?.jobTitle" class="ms-1">({{ c.author.jobTitle }})</span>
                        · {{ formatDateTime(c.created, dateMask) }}
                        <span v-if="c.isInternal" class="badge text-bg-warning ms-1"><i class="bi bi-lock-fill me-1"></i>{{ $t("internalNote") }}</span>
                    </div>
                    <div class="hd-bubble">{{ c.body }}</div>
                </div>
            </div>
            <p v-if="!comments.length" class="text-muted fst-italic small text-center my-2">{{ $t("noReplies") }}</p>
        </div>

        <div v-if="!readonly" class="hd-composer" :class="{ 'hd-composer-internal': isInternal }">
            <textarea
                v-model="body"
                class="form-control mb-2"
                rows="3"
                :placeholder="isInternal ? $t('internalNotePlaceholder') : $t('replyPlaceholder')"
                @keydown.ctrl.enter.prevent="send"
            ></textarea>
            <div class="d-flex align-items-center flex-wrap gap-2">
                <div v-if="isStaff" class="form-check form-switch mb-0">
                    <input id="hd-internal" v-model="isInternal" class="form-check-input" type="checkbox" />
                    <label class="form-check-label small" for="hd-internal"><i class="bi bi-lock me-1"></i>{{ $t("internalNote") }}</label>
                </div>
                <small class="text-muted d-none d-md-inline">{{ $t("ctrlEnterToSend") }}</small>
                <div class="flex-grow-1"><Feedback :feedback="feedback" /></div>
                <button type="button" class="btn" :class="isInternal ? 'btn-warning' : 'btn-primary'" :disabled="!body.trim() || feedback.isPending" @click="send">
                    <i class="bi bi-send me-1"></i>{{ isInternal ? $t("addNote") : $t("sendReply") }}
                </button>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue"
import { get } from "@regira/modules/vue/ioc"
import { Feedback, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { formatDateTime } from "@regira/modules/vue/formatters"
import { useLang } from "@regira/modules/vue/lang"
import type { Entity as Person } from "@/entities/persons"
import Entity, { type TicketComment } from "../data/Entity"
import type EntityService from "../data/EntityService"
import { useAccess } from "@/infrastructure/access"

const props = defineProps<{ readonly?: boolean }>()
const ticket = defineModel<Entity>({ required: true })

const { isStaff } = useAccess()
const { translate } = useLang()
const feedback = useFeedback()
const body = ref("")
const isInternal = ref(false)
const dateMask = "dd/MM/yyyy HH:mm"

const comments = computed(() => ticket.value.comments ?? [])
const isStaffAuthor = (c: TicketComment) => c.author?.role === "Employee"
const personName = (p?: Partial<Person>) => (p ? [p.givenName, p.familyName].filter(Boolean).join(" ") || p.fullName : "")
const initials = (p?: Partial<Person>) => (((p?.givenName?.[0] ?? "") + (p?.familyName?.[0] ?? "")).toUpperCase() || "?")

// comments have one writer: POST tickets/{id}/comments (a custom endpoint on the raw service, not the pooled store)
async function send() {
    const text = body.value.trim()
    if (!text || props.readonly || !ticket.value.id) return
    feedback.pending(translate("sending"))
    try {
        const svc = get<EntityService>(Entity.name)!
        const comment = await svc.addComment(ticket.value.id, text, isStaff.value && isInternal.value)
        ticket.value.comments = [...comments.value, comment]
        ticket.value.commentCount = (ticket.value.commentCount ?? 0) + 1
        body.value = ""
        isInternal.value = false
        feedback.success(translate("sent"))
    } catch (ex: any) {
        console.error("Sending the reply failed", ex)
        feedback.fail(translate("sendFailed"), toFeedbackError(ex) ?? ex?.message)
    }
}
</script>
