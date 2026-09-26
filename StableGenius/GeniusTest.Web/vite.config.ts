import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

export default defineConfig(({ command }) => ({
    // production is served by the IIS virtual directory /stablegenius/play (public/web.config); dev stays at the root
    base: command === "build" ? "/stablegenius/play/" : "/",
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    server: {
        port: Number(process.env.PORT) || 5712,
        strictPort: true,
        // URL contract: axios base "/api" (config.json) + IConfig.api "/questions" -> proxied to the API as "/questions";
        // the API has no route prefix of its own (in IIS the /stablegenius/api application path plays that role)
        proxy: { "/api": { target: "http://localhost:5711", changeOrigin: true, rewrite: (path) => path.replace(/^\/api/, "") } },
    },
}))
