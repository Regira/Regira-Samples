import { initAxios } from "@regira/modules/vue/http"

// Headless data layer (see regira_modules.vue.entities -> entities.setup -> "Headless quick-start"):
// this storefront is a fully bespoke UI, so it talks to the API through the shared axios instance
// only -- no entity plugins/ioc/shell. The Vite dev proxy forwards "/api" to the back-end (port 6180).
export const http = initAxios({ api: "/api" })
