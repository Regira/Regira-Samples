import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

// URL contract: config.json → api "/api" (axios base) + IConfig.api "/articles" → Vite proxy "/api" →
// http://localhost:5871/api/articles (the API applies the "api" route prefix centrally)
export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    server: {
        port: 5872,
        strictPort: true,
        proxy: { "/api": { target: "http://localhost:5871", changeOrigin: true } },
    },
    preview: { port: 5872, strictPort: true },
})
