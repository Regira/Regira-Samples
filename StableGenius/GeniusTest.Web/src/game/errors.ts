import type { Router } from "vue-router"
import { toFeedbackError } from "@regira/modules/vue/ui"

export const httpStatus = (ex: unknown): number | undefined => (ex as { response?: { status?: number } })?.response?.status

/** Sends the player to the (apologetic) error page matching a failed request. Never their fault. */
export function toErrorPage(router: Router, ex: unknown) {
    const status = httpStatus(ex)
    const name = status === 404 ? "notFound" : status === 403 ? "forbidden" : status === 401 ? "unauthorized" : "serverError"
    return router.replace({ name, query: { url: router.currentRoute.value.fullPath } })
}

/** The API's own (friendly) message for a failed request, as one line - never a list of field names. */
export function friendlyMessage(ex: unknown): string | undefined {
    const error = toFeedbackError(ex)
    if (!error) return undefined
    if (typeof error === "string") return error
    const lines = Object.values(error).flatMap((v) => (Array.isArray(v) ? v : [v]))
    return lines.length ? lines.join(" ") : undefined
}
