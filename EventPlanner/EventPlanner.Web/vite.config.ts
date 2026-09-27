import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    // SPA on 5822, API on 5821: /api is proxied, so the SPA and the API share one origin (no CORS)
    server: {
        port: 5822,
        strictPort: true,
        proxy: { "/api": { target: "http://localhost:5821", changeOrigin: true, xfwd: true } },
    },
    preview: { port: 5822, strictPort: true },
})
