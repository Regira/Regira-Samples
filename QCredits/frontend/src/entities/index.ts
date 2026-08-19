import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as employeePlugin } from "./employees"
import { plugin as creditPolicyPlugin } from "./credit-policies"
import { plugin as employeeCarryOverPlugin } from "./employee-carry-overs"
import { plugin as groupTrainingPlugin } from "./group-trainings"
import { plugin as qCreditRequestPlugin } from "./q-credit-requests"

// order matters where one entity's selecting/Selector.vue is used inside another's form
export const plugins = [employeePlugin, creditPolicyPlugin, groupTrainingPlugin, employeeCarryOverPlugin, qCreditRequestPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
