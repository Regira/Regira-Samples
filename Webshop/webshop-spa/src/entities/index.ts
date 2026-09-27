import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as categoryPlugin } from "./categories"
import { plugin as brandPlugin } from "./brands"
import { plugin as promotionPlugin } from "./promotions"
import { plugin as productPlugin } from "./products"
import { plugin as orderPlugin } from "./orders"

export const plugins = [categoryPlugin, brandPlugin, promotionPlugin, productPlugin, orderPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
