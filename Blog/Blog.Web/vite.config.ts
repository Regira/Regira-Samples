import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

// URL contract: config.json api "/api" (axios base) + IConfig.api "/posts" -> /api/posts -> proxied to the
// API on :5811, which serves every controller under the central "api" route prefix.
export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    server: {
        port: 5812,
        strictPort: true,
        proxy: { "/api": { target: "http://localhost:5811", changeOrigin: true, xfwd: true } },
    },
    preview: { port: 5812, strictPort: true },
})
