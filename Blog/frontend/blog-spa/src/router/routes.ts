import type { RouteRecordRaw } from "vue-router"
import BlogHome from "@/views/BlogHome.vue"
import BlogPostDetail from "@/views/BlogPostDetail.vue"
import HomeView from "@/views/HomeView.vue"
import NotFound from "@/views/NotFound.vue"
import Forbidden from "@/views/Forbidden.vue"
import Unauthorized from "@/views/Unauthorized.vue"

// no-auth app: every route is anonymous. Public blog is the site root; entity management lives under /admin.
const routes: Array<RouteRecordRaw> = [
    { path: "/", name: "blogHome", component: BlogHome, meta: { allowAnonymous: true } },
    { path: "/posts/:slug", name: "blogPostDetail", component: BlogPostDetail, props: true, meta: { allowAnonymous: true } },
    { path: "/admin", name: "home", component: HomeView, meta: { allowAnonymous: true } },
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
