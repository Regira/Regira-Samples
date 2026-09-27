import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as shopperPlugin } from "./shoppers"
import { plugin as shoppingListPlugin } from "./shopping-lists"
import { plugin as categoryPlugin } from "./categories"
import { plugin as articlePlugin } from "./articles"

export const plugins = [shoppingListPlugin, articlePlugin, categoryPlugin, shopperPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
