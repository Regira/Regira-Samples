import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as interventionTypePlugin } from "./intervention-types"
import { plugin as vehiclePlugin } from "./vehicles"
import { plugin as supplierPlugin } from "./suppliers"
import { plugin as invoicePlugin } from "./invoices"
import { plugin as interventionPlugin } from "./interventions"

// order matters where one entity's selecting/Selector.vue is used inside another's form
export const plugins = [interventionTypePlugin, vehiclePlugin, supplierPlugin, invoicePlugin, interventionPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
