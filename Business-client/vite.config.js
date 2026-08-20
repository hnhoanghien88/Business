import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      "/identity": {
        target: "https://localhost:7203",
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.slice(9),
      },
      "/backend": {
        target: "https://localhost:7021",
        changeOrigin: true,
        secure: false,
        rewrite: (path) => path.slice(8),
      },
    },
  },
});
