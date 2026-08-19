import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    server: { port: Number(process.env.PORT) || 6161 }, // honor a harness/preview-assigned PORT (Vite ignores it by default)
    // Simplest dev setup: config.json → api points straight at the API origin (http://localhost:6160/api)
    // and the API enables CORS for loopback origins in Program.cs — no dev proxy needed.
})
