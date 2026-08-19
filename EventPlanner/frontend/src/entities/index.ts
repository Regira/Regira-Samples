import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"

import { plugin as venuePlugin } from "./locations"
import { plugin as speakerPlugin } from "./speakers"
import { plugin as eventCategoryPlugin } from "./event-categories"
import { plugin as employeePlugin } from "./employees"
import { plugin as eventItemPlugin } from "./events"
import { plugin as sessionPlugin } from "./sessions"
import { plugin as registrationPlugin } from "./registrations"

// order matters where one entity's selecting/Selector.vue is used inside another's form
export const plugins = [
    venuePlugin,
    speakerPlugin,
    eventCategoryPlugin,
    employeePlugin,
    eventItemPlugin,
    sessionPlugin,
    registrationPlugin,
]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
