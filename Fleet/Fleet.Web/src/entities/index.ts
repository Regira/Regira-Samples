import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as interventionTypePlugin } from "./intervention-types"
import { plugin as vehiclePlugin } from "./vehicles"
import { plugin as supplierPlugin } from "./suppliers"
import { plugin as invoicePlugin } from "./invoices"
import { plugin as interventionPlugin } from "./interventions"

export const plugins = [interventionTypePlugin, vehiclePlugin, supplierPlugin, invoicePlugin, interventionPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
