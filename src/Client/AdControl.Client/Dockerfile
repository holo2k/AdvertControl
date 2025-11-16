# Dockerfile
FROM node:22.15.0 AS builder
WORKDIR /app

# Копируем package.json и lock (для кэша)
COPY package*.json ./

RUN npm ci --prefer-offline --no-audit --progress=false

# Копируем код
COPY . .

# НЕ запускаем tsc; используем vite напрямую — esbuild transpiles TS without type-check.
RUN npx vite build

# Runtime stage (preview)
FROM node:22.15.0 AS runner
WORKDIR /app
RUN npm i -g serve
COPY --from=builder /app/dist /app/dist
EXPOSE 3000
CMD ["serve", "-s", "dist", "-l", "3000"]