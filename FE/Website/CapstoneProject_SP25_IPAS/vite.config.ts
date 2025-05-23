import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import svgr from "vite-plugin-svgr";

export default defineConfig({
  plugins: [react(), svgr()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    watch: {
      usePolling: true,
    },
    headers: {
      "Cross-Origin-Opener-Policy": "same-origin-allow-popups",
      "Cross-Origin-Embedder-Policy": "require-corp",
      // "Cross-Origin-Embedder-Policy": "unsafe-none",
    },
    proxy: {
      '/blob': {
        target: 'https://irisprodseatraining.blob.core.windows.net',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/blob/, ''),
      },
    },
  },
});
