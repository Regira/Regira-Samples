import { createRouter, createWebHistory, type RouteRecordRaw } from "vue-router"
import staticRoutes from "./routes"

export default function routerFactory(entityRoutes: Array<RouteRecordRaw>) {
    return createRouter({
        history: createWebHistory(import.meta.env.BASE_URL),
        routes: [...staticRoutes, ...entityRoutes],
        linkActiveClass: "active",
        // editorial pages start at the top; a Back navigation restores the reader's position
        scrollBehavior: (to, from, saved) => saved ?? (to.path !== from.path ? { top: 0 } : undefined),
    })
}
