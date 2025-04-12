import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

// Check if we're in Docker
const isDocker = process.env.DOCKER_BUILD === 'true';

export default defineConfig({
  plugins: [react()],
  build: {
    // Use 'dist' if in Docker, otherwise use backend's wwwroot
    outDir: isDocker ? 'dist' : path.resolve(__dirname, '../T9Backend/wwwroot'),
    emptyOutDir: true
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5019',  // Adjust to match your backend port
        changeOrigin: true
      }
    }
  }
})