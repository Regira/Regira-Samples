import type { App } from "vue"
import type { RouteRecordRaw } from "vue-router"
import { plugin as categoryPlugin } from "./categories"
import { plugin as tagPlugin } from "./tags"
import { plugin as blogPostPlugin } from "./blog-posts"

// order matters where one entity's selecting/Selector.vue is used inside another's form
export const plugins = [categoryPlugin, tagPlugin, blogPostPlugin]

export default {
    install(app: App<Element>, { routes }: { routes: Array<RouteRecordRaw> }) {
        plugins.forEach((plugin) => app.use(plugin as any, { routes }))
    },
}
