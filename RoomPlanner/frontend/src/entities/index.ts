import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as buildingPlugin } from "./buildings"
import { plugin as employeePlugin } from "./employees"
import { plugin as floorPlugin } from "./floors" // depends on Building's selecting/InputSelector
import { plugin as meetingRoomPlugin } from "./meeting-rooms" // depends on Floor's selecting/InputSelector
import { plugin as reservationPlugin } from "./reservations" // depends on Employee's and MeetingRoom's selecting/InputSelector

// order matters where one entity's selecting/Selector.vue is used inside another's form
export const plugins = [buildingPlugin, employeePlugin, floorPlugin, meetingRoomPlugin, reservationPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
