import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    // the SPA calls /api/* on its own origin; the dev proxy forwards it to the Webshop API (server route prefix "api")
    server: {
        port: 5882,
        strictPort: true,
        proxy: { "/api": { target: "http://localhost:5881", changeOrigin: true, xfwd: true } },
    },
    preview: { port: 5882, strictPort: true, proxy: { "/api": { target: "http://localhost:5881", changeOrigin: true } } },
})
