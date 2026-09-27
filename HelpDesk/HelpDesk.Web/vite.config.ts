import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

// URL contract: config.json → api "/api" (axios base) + IConfig.api "/tickets" → proxied to the API, whose
// controllers carry the central "api" route prefix. xfwd lets the API build attachment URIs on this origin.
export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    server: {
        port: 5842,
        strictPort: true,
        proxy: { "/api": { target: "http://localhost:5841", changeOrigin: false, xfwd: true } },
    },
    preview: { port: 5842, strictPort: true },
})
