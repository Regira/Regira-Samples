import type { RouteRecordRaw } from "vue-router"
import HomeView from "@/views/HomeView.vue"
import NotFound from "@/views/NotFound.vue"
import Forbidden from "@/views/Forbidden.vue"
import Unauthorized from "@/views/Unauthorized.vue"
import ShopLayout from "@/shop/ShopLayout.vue"

// Storefront routes render inside ShopLayout (meta.layout = "shop"); the back office (/admin + every
// entity slice route under /admin/...) renders inside the scaffolded admin shell in App.vue.
const routes: Array<RouteRecordRaw> = [
    {
        path: "/",
        component: ShopLayout,
        meta: { layout: "shop", allowAnonymous: true },
        children: [
            { path: "", name: "home", component: () => import("@/shop/views/ShopHome.vue") },
            { path: "shop", name: "shop", component: () => import("@/shop/views/ShopCatalog.vue") },
            { path: "product/:id", name: "product", component: () => import("@/shop/views/ProductPage.vue"), props: true },
            { path: "cart", name: "cart", component: () => import("@/shop/views/CartPage.vue") },
            { path: "checkout", name: "checkout", component: () => import("@/shop/views/CheckoutPage.vue") },
            { path: "order/:id/confirmation", name: "orderConfirmation", component: () => import("@/shop/views/OrderConfirmation.vue"), props: true },
            { path: "404", name: "notFound", component: NotFound, props: (to) => ({ url: to.query.url }) },
        ],
    },
    { path: "/admin", name: "adminHome", component: HomeView, meta: { allowAnonymous: true } },
    { path: "/401", name: "unauthorized", component: Unauthorized, props: (to) => ({ url: to.query.url }), meta: { allowAnonymous: true } },
    { path: "/403", name: "forbidden", component: Forbidden, props: (to) => ({ url: to.query.url }) },
    {
        path: "/:pathMatch(.*)*",
        name: "catchAll",
        redirect: (from) => ({ name: "notFound", query: { url: from.fullPath } }),
        meta: { allowAnonymous: true },
    },
]

export default routes
