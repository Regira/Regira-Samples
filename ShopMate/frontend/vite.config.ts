import { fileURLToPath, URL } from "node:url"
import { defineConfig } from "vite"
import vue from "@vitejs/plugin-vue"

export default defineConfig({
    plugins: [vue()],
    resolve: { alias: { "@": fileURLToPath(new URL("./src", import.meta.url)) } },
    define: { __APP_VERSION__: JSON.stringify(process.env.npm_package_version) },
    server: { port: Number(process.env.PORT) || 6171, strictPort: true },
    // API is called directly at its own origin (http://localhost:6170) with CORS enabled server-side —
    // see entities.setup.md -> Simplest dev setup - direct origin + CORS
})
