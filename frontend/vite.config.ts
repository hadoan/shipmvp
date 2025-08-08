import react from '@vitejs/plugin-react';
import { componentTagger } from 'lovable-tagger';
import path from 'path';
import { defineConfig } from 'vite';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => ({
  server: {
    host: process.env.HOST || '0.0.0.0',
    port: parseInt(process.env.PORT || '8080'),
  },
  plugins: [react(), mode === 'development' && componentTagger()].filter(
    Boolean
  ),
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
}));
