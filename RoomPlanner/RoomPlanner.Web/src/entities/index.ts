import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as buildingPlugin } from "./buildings"
import { plugin as floorPlugin } from "./floors"
import { plugin as equipmentPlugin } from "./equipment"
import { plugin as employeePlugin } from "./employees"
import { plugin as roomPlugin } from "./rooms"
import { plugin as reservationPlugin } from "./reservations"

// lookups first: their selectors are used inside the forms of the entities that follow
export const plugins = [buildingPlugin, floorPlugin, equipmentPlugin, employeePlugin, roomPlugin, reservationPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
