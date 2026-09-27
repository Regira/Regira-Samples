import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    server: {
        port: 5802,
        strictPort: true,
        // URL contract: axios base "/api" -> this proxy -> API (server route prefix "api")
        // xfwd: the API builds attachment URIs from the forwarded (SPA) host
        proxy: { "/api": { target: "http://localhost:5801", changeOrigin: true, xfwd: true } },
    },
})
