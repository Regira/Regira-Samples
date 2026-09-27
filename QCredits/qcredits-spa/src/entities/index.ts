import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as departmentPlugin } from "./departments"
import { plugin as employeePlugin } from "./employees"
import { plugin as creditYearPlugin } from "./credit-years"
import { plugin as creditAllocationPlugin } from "./credit-allocations"
import { plugin as creditRequestPlugin } from "./credit-requests"
import { plugin as groupTrainingPlugin } from "./group-trainings"

// order matters where one entity's selecting/Selector.vue is used inside another's form
export const plugins = [departmentPlugin, employeePlugin, creditYearPlugin, creditAllocationPlugin, creditRequestPlugin, groupTrainingPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
