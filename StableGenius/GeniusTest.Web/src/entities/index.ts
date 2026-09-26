import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as questionPlugin } from "./questions"
import { plugin as reactionPlugin } from "./reactions"
import { plugin as gamePlugin } from "./games"

export const plugins = [questionPlugin, reactionPlugin, gamePlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
