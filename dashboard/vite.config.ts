import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

// https://vite.dev/config/
export default defineConfig({
  build: {
    outDir: path.join(
      __dirname,
      "../TransparentCloudServerProxy.WebDashboard/wwwroot"
    ),
  },
  plugins: [react()],
});
