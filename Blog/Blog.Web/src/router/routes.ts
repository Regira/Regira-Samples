import type { RouteRecordRaw } from "vue-router"
import BlogHome from "@/views/blog/BlogHome.vue"
import PostView from "@/views/blog/PostView.vue"
import HomeView from "@/views/HomeView.vue"
import NotFound from "@/views/NotFound.vue"
import Forbidden from "@/views/Forbidden.vue"
import Unauthorized from "@/views/Unauthorized.vue"

// public pages (published posts only) + the management dashboard; entity slices add their own routes
const routes: Array<RouteRecordRaw> = [
    { path: "/", name: "blog", component: BlogHome, meta: { allowAnonymous: true, public: true } },
    { path: "/read/:slug", name: "post", component: PostView, props: true, meta: { allowAnonymous: true, public: true } },
    { path: "/manage", name: "home", component: HomeView, meta: { allowAnonymous: true } },
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
