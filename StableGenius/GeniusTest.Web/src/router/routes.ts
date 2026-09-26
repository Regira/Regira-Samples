import type { RouteRecordRaw } from "vue-router"
import AdminHomeView from "@/views/HomeView.vue"
import LandingView from "@/game/views/LandingView.vue"
import ErrorView from "@/game/views/ErrorView.vue"

// meta.layout "game" = the kitsch player chrome; everything else gets the (gold-tinted) admin chrome.
// The slices push their routes under /admin/* (config.routePrefix).
const game = { layout: "game", allowAnonymous: true }

const routes: Array<RouteRecordRaw> = [
    { path: "/", name: "home", component: LandingView, meta: game },
    { path: "/play/:key", name: "play", component: () => import("@/game/views/PlayView.vue"), props: (to) => ({ gameKey: to.params.key }), meta: game },
    { path: "/results/:key", name: "results", component: () => import("@/game/views/ResultsView.vue"), props: (to) => ({ gameKey: to.params.key }), meta: game },
    { path: "/admin", name: "admin", component: AdminHomeView, meta: { allowAnonymous: true } },

    // error pages - never the player's fault
    { path: "/401", name: "unauthorized", component: ErrorView, props: (to) => ({ code: 401, url: to.query.url }), meta: game },
    { path: "/403", name: "forbidden", component: ErrorView, props: (to) => ({ code: 403, url: to.query.url }), meta: game },
    { path: "/404", name: "notFound", component: ErrorView, props: (to) => ({ code: 404, url: to.query.url }), meta: game },
    { path: "/500", name: "serverError", component: ErrorView, props: (to) => ({ code: 500, url: to.query.url }), meta: game },
    {
        path: "/:pathMatch(.*)*",
        name: "catchAll",
        redirect: (from) => ({ name: "notFound", query: { url: from.fullPath } }),
        meta: game,
    },
]

export default routes
