import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// BASE_PATH задаётся в GitHub Actions для деплоя на GitHub Pages (/<repo>/)
export default defineConfig({
  base: process.env.BASE_PATH || '/',
  plugins: [react()],
})
