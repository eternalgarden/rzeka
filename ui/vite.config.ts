import { defineConfig } from "vite"
import wasm from "vite-plugin-wasm"
import topLevelAwait from "vite-plugin-top-level-await"

// Production builds are deployed to https://eternalgarden.github.io/rzeka/ via
// GitHub Pages, so built asset URLs need the `/rzeka/` prefix. Dev mode keeps
// the root base so `npm run dev` still serves at http://localhost:5173/.
export default defineConfig(({ command }) => ({
    base: command === "build" ? "/rzeka/" : "/",
    plugins: [
        // Required for brotli-wasm: vite otherwise serves .wasm binaries incorrectly
        // in dev mode, causing a "failed to match magic number" CompileError.
        // topLevelAwait is needed alongside it because brotli-wasm uses top-level await
        // at module init time (const brotli = await brotliPromise).
        wasm(),
        topLevelAwait(),
    ],
    optimizeDeps: {
        // brotli-wasm's browser entry is CJS, which causes esbuild's pre-bundler to
        // consume the package before vite-plugin-wasm can intercept the .wasm import.
        // Excluding it makes vite serve the original files so the plugin handles them.
        exclude: ["brotli-wasm"],
    },
    build: {
        outDir: "dist",
    },
}))
