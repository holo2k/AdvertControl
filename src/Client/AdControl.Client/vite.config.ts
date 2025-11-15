/// <reference types="node" />
import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import { env } from 'process';

export default defineConfig(({ mode }) => {


  const API = process.env.SERVER_API_URL ?
      `${process.env.SERVER_API_URL}` : 'http://localhost:5000/api';

  return {
    plugins: [react()],
    server: {
      proxy: {
        "/api": {
          target: API,
          changeOrigin: true,
          secure: false,
          rewrite: (p) => p.replace(/^\/api/, ""),
        },
      },
      host: "0.0.0.0",
      port: 5173,
      protocol: 'wss',
      allowedHosts: [
        "ad-control.ru",
        "localhost",
      ],
      hmr: true,
    },
  };
});
