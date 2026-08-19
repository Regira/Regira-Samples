import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"

import { plugin as categoryPlugin } from "./categories"
import { plugin as assetStatusPlugin } from "./asset-statuses"
import { plugin as locationPlugin } from "./locations"
import { plugin as supplierPlugin } from "./suppliers"
import { plugin as employeePlugin } from "./employees"
import { plugin as assetPlugin } from "./assets"
import { plugin as assetAssignmentPlugin } from "./asset-assignments"

// order matters where one entity's selecting/Selector.vue is used inside another's form:
// lookups first, then Asset (relates to all four), then AssetAssignment (relates to Asset + Employee)
export const plugins = [categoryPlugin, assetStatusPlugin, locationPlugin, supplierPlugin, employeePlugin, assetPlugin, assetAssignmentPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
