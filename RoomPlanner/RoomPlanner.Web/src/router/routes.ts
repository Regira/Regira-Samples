import type { RouteRecordRaw } from "vue-router"
import HomeView from "@/views/HomeView.vue"
import NotFound from "@/views/NotFound.vue"
import Forbidden from "@/views/Forbidden.vue"
import Unauthorized from "@/views/Unauthorized.vue"

// no-auth app: every route is anonymous
const routes: Array<RouteRecordRaw> = [
    { path: "/", name: "home", component: HomeView, meta: { allowAnonymous: true } },
    { path: "/planner", name: "planner", component: () => import("@/views/PlannerView.vue"), meta: { allowAnonymous: true } },
    { path: "/find-a-room", name: "roomFinder", component: () => import("@/views/RoomFinderView.vue"), meta: { allowAnonymous: true } },
    { path: "/calendar", name: "calendar", component: () => import("@/views/CalendarView.vue"), meta: { allowAnonymous: true } },
    { path: "/approvals", name: "approvals", component: () => import("@/views/ApprovalsView.vue"), meta: { allowAnonymous: true } },
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
