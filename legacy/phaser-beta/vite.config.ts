import { defineConfig } from 'vite';
import { copyFileSync, existsSync, mkdirSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';

export default defineConfig({
  base: './',
  server: {
    host: '0.0.0.0',
    port: 43125,
  },
  preview: {
    host: '0.0.0.0',
    port: 43125,
  },
  build: {
    outDir: 'dist',
    assetsInlineLimit: 4096,
  },
  plugins: [
    {
      name: 'yandex-sdk-stub',
      configureServer(server) {
        server.middlewares.use('/sdk.js', (_req, res) => {
          res.setHeader('Content-Type', 'application/javascript');
          res.end('/* local YaGames stub — real SDK injected on Yandex Games */\n');
        });
      },
      closeBundle() {
        const dist = resolve(__dirname, 'dist');
        if (!existsSync(dist)) mkdirSync(dist, { recursive: true });
        // Placeholder so ZIP structure is valid offline; console replaces with CDN path.
        writeFileSync(
          resolve(dist, 'sdk.js'),
          '/* Replace with https://yandex.ru/games/sdk/v2 when publishing */\n',
        );
        const favicon = resolve(__dirname, 'public/favicon.svg');
        if (existsSync(favicon)) {
          copyFileSync(favicon, resolve(dist, 'favicon.svg'));
        }
      },
    },
  ],
});
