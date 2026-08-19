import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as categoryPlugin } from "./categories"
import { plugin as shoppingListPlugin } from "./shopping-lists"
import { plugin as articlePlugin } from "./articles"

// order matters where one entity's selecting/Selector.vue is used inside another's form:
// categories before articles (the article-categories picker selects a Category)
export const plugins: any[] = [categoryPlugin, shoppingListPlugin, articlePlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
