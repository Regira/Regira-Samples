import type { RouteRecordRaw } from "vue-router"
import HomeView from "@/views/HomeView.vue"
import AccountView from "@/views/AccountView.vue"
import ResetPasswordView from "@/views/ResetPasswordView.vue"
import NotFound from "@/views/NotFound.vue"
import Forbidden from "@/views/Forbidden.vue"
import Unauthorized from "@/views/Unauthorized.vue"

// login is driven by the App.vue modal (auth-on); routes without allowAnonymous are treated as protected
// — home included: an anonymous visitor gets the sign-in modal, not a dashboard they can't act on
const routes: Array<RouteRecordRaw> = [
    { path: "/", name: "home", component: HomeView },
    { path: "/account", name: "account", component: AccountView },
    // the recovery mail links here — allowAnonymous, or the visitor who forgot their password can't reach it
    { path: "/reset-password", name: "resetPassword", component: ResetPasswordView, meta: { allowAnonymous: true } },
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
