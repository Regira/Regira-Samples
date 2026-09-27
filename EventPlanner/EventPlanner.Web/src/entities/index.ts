import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as eventCategoryPlugin } from "./event-categories"
import { plugin as locationPlugin } from "./locations"
import { plugin as speakerPlugin } from "./speakers"
import { plugin as employeePlugin } from "./employees"
import { plugin as eventPlugin } from "./events"
import { plugin as registrationPlugin } from "./registrations"

export const plugins = [eventCategoryPlugin, locationPlugin, speakerPlugin, employeePlugin, eventPlugin, registrationPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
