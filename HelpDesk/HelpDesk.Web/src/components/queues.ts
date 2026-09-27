import type { LocationQueryRaw } from "vue-router"

/** Ticket queues = saved searches over the ticket overview (the query lands in its search object). */
export interface Queue {
    key: string
    icon: string
    query: LocationQueryRaw
}

export const queues: Array<Queue> = [
    { key: "queueMine", icon: "bi bi-person-check", query: { assignedToMe: "true", isClosed: "false", sortBy: "Priority" } },
    { key: "queueUnassigned", icon: "bi bi-inbox", query: { isAssigned: "false", isClosed: "false", sortBy: "Priority" } },
    { key: "queueOverdue", icon: "bi bi-alarm", query: { isOverdue: "true", sortBy: "DueDate" } },
    { key: "queueOpen", icon: "bi bi-envelope-open", query: { isClosed: "false", sortBy: "LastActivity" } },
    { key: "queueAll", icon: "bi bi-archive", query: { sortBy: "Newest" } },
]
