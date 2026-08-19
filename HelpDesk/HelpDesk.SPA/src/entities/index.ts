import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as categoryPlugin } from "./categories"
import { plugin as priorityPlugin } from "./priorities"
import { plugin as statusPlugin } from "./statuses"
import { plugin as supportTeamPlugin } from "./support-teams"
import { plugin as personPlugin } from "./people"
import { plugin as ticketPlugin } from "./tickets"

// order matters where one entity's selecting/Selector.vue is used inside another's form
export const plugins = [categoryPlugin, priorityPlugin, statusPlugin, supportTeamPlugin, personPlugin, ticketPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
