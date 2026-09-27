import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

// URL contract: config.json api "/api" (axios base) + IConfig.api "/rooms" → /api/rooms → proxied to the API,
// whose controllers sit under the central "api" route prefix.
export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    server: {
        port: 5862,
        strictPort: true,
        proxy: { "/api": { target: "http://localhost:5861", changeOrigin: true } },
    },
    preview: { port: 5862, strictPort: true },
})
