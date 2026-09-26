import { createApp } from "vue"
import { createPinia } from "pinia"
import type { RouteRecordRaw } from "vue-router"
import { initAxios } from "@regira/modules/vue/http"
import { plugin as servicesPlugin, type IServiceProvider } from "@regira/modules/vue/ioc"
import { plugin as appPlugin, AppStatus, whenAppReady } from "@regira/modules/vue/app"
import { plugin as langPlugin } from "@regira/modules/vue/lang"
import { useLang } from "@regira/modules/vue/lang" // used for document.title + (auth) setLangCode
import { iconPlugin, screenPlugin, loadingPlugin, feedbackPlugin } from "@regira/modules/vue/ui"
import { focus, grow, clickOutside } from "@regira/modules/vue/directives"
import "bootstrap/dist/css/bootstrap.min.css"
import "bootstrap-icons/font/bootstrap-icons.css"
import "@regira/modules/style.css"
import "@/assets/theme.scss" // the app theme — MUST come after bootstrap + regira styles so its overrides win
import "@/game/kitsch.scss" // the game's (deliberately) terrible taste - scoped to .vsg*
import { preloaderPlugin, defaultPoolCache, PoolCache } from "@regira/modules/vue/entities"
import { plugin as debugPlugin } from "@regira/modules/vue/debug"
import dateExtensions from "@regira/modules/extensions/date-extensions"
import entityPlugins from "@/entities"
import { routerFactory } from "@/router"
import appConfig, { createConfig } from "@/app-config"
import App from "@/App.vue"
import { loadScreenTexts } from "@/game/texts"

const loadingImg = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7" // 1×1 — swap for your spinner

dateExtensions.use() // serialize Dates to JSON without a timezone shift

// runtime files (config + public/data) always revalidate - a 304 when unchanged - so a redeploy shows up without a hard refresh
const revalidate: RequestInit = { cache: "no-cache" }

fetch(`${appConfig.baseUrl}/config.json`, revalidate)
    .then((r) => r.json())
    .then(async (raw) => {
        const config = createConfig(raw)
        const axios = initAxios({ api: config.api, includeCredentials: config.includeCredentials })
        const translations = await fetch(`${appConfig.baseUrl}/data/translations.json`, revalidate).then((r) => r.json())
        await loadScreenTexts(`${appConfig.baseUrl}/data/screen-texts.csv`) // the game's jokes (public/data/screen-texts.csv)

        const app = createApp(App)
        app.use(createPinia())
        app.use(appPlugin, { culture: config.culture })
        app.use(servicesPlugin, {
            configure: (sp: IServiceProvider) => sp.add("axios", () => axios).add(PoolCache.name, () => defaultPoolCache),
        })

        app.use(iconPlugin, { source: "bs" })
        app.use(screenPlugin)
        app.use(loadingPlugin, { img: loadingImg })
        app.use(feedbackPlugin, { autoHideDelay: 2500 })
        app.use(langPlugin, { defaultLang: "en", messages: translations })

        document.title = useLang().translateMessage(config.title) || document.title // browser-tab title from config.title (culture-keyed); re-set on culture change if you localize it

        app.use(focus)
        app.use(grow)
        app.use(clickOutside)

        const entityRoutes: Array<RouteRecordRaw> = []
        app.use(entityPlugins, { routes: entityRoutes })
        const router = routerFactory([...entityRoutes])
        app.use(router)
        // An unexpected crash is never the player's fault - show the apologetic 500 page instead of a broken view.
        app.config.errorHandler = (err, _instance, info) => {
            console.error(err, info)
            if (router.currentRoute.value.name !== "serverError")
                void router.replace({ name: "serverError", query: { url: router.currentRoute.value.fullPath } })
        }
        app.use(preloaderPlugin)
        app.use(debugPlugin, { isDebug: config.isDebug })


        app.config.globalProperties.$setAppStatus(AppStatus.Mounting)
        app.mount("#app")
        app.config.globalProperties.$setAppStatus(AppStatus.Ready)
        await whenAppReady()
    })
