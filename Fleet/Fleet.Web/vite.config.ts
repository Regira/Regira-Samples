import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    server: {
        port: 5832,
        strictPort: true,
        // URL contract: axios base "/api" -> proxy -> API (controllers under the "api" route prefix)
        proxy: { "/api": { target: "http://localhost:5831", changeOrigin: true, xfwd: true } },
    },
})
