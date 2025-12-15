#!/bin/sh
set -eu

# Установим mc если его нет
if ! command -v mc >/dev/null 2>&1; then
  echo "Installing mc..."
  apk add --no-cache curl ca-certificates >/dev/null 2>&1 || true
  curl -fsSL https://dl.min.io/client/mc/release/linux-amd64/mc -o /usr/local/bin/mc
  chmod +x /usr/local/bin/mc
fi

# Настроим alias и бакет (без падения если уже есть)
echo "Configuring mc..."
mc alias set local http://minio:9000 minioaccess miniosecret
mc mb --ignore-existing local/files
mc anonymous set download local/files || true

echo "MinIO bucket files is public"
