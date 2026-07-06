import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Static SPA build ("serverless" mode): the output in dist/ is plain files that can be hosted on
// any static host/CDN; all data comes from the Dotnetable API at runtime (see .env.example).
export default defineConfig({
  plugins: [react()],
});
