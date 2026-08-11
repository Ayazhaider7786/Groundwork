import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      // Proxying /api in development keeps the browser on one origin, so there is
      // no CORS preflight and no need to trust the ASP.NET dev certificate.
      // Production points VITE_API_BASE_URL at the real API origin instead.
      '/api': {
        target: 'https://localhost:7157',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
