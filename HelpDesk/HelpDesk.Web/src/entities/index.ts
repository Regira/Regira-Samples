import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as supportTeamPlugin } from "./support-teams"
import { plugin as categoryPlugin } from "./categories"
import { plugin as priorityPlugin } from "./priorities"
import { plugin as statusPlugin } from "./statuses"
import { plugin as personPlugin } from "./persons"
import { plugin as ticketPlugin } from "./tickets"

export const plugins = [supportTeamPlugin, categoryPlugin, priorityPlugin, statusPlugin, personPlugin, ticketPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
