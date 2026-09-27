import type { RouteRecordRaw } from "vue-router"
import HomeView from "@/views/HomeView.vue"
import NotFound from "@/views/NotFound.vue"
import Forbidden from "@/views/Forbidden.vue"
import Unauthorized from "@/views/Unauthorized.vue"
import ListBoardView from "@/views/ListBoardView.vue"

// no-auth app: every route is public
const routes: Array<RouteRecordRaw> = [
    { path: "/", name: "home", component: HomeView, meta: { allowAnonymous: true } },
    // the in-store "board" of one shopping list (the list's own create/edit form stays at /shopping-lists/:id)
    { path: "/lists/:id", name: "ShoppingListBoard", component: ListBoardView, props: true, meta: { allowAnonymous: true } },
    { path: "/401", name: "unauthorized", component: Unauthorized, props: (to) => ({ url: to.query.url }), meta: { allowAnonymous: true } },
    { path: "/403", name: "forbidden", component: Forbidden, props: (to) => ({ url: to.query.url }) },
    { path: "/404", name: "notFound", component: NotFound, props: (to) => ({ url: to.query.url }), meta: { allowAnonymous: true } },
    {
        path: "/:pathMatch(.*)*",
        name: "catchAll",
        redirect: (from) => ({ name: "notFound", query: { url: from.fullPath } }),
        meta: { allowAnonymous: true },
    },
]

export default routes
