#!/usr/bin/env bash
set -euo pipefail

LOG_DIR="/var/log/citizenship"
LOG_FILE="$LOG_DIR/deploy-api.log"
mkdir -p "$LOG_DIR"
touch "$LOG_FILE"
chmod 640 "$LOG_FILE"

# Mirror output to log file
exec > >(tee -a "$LOG_FILE") 2>&1

echo "[$(date -Is)] Deploy start"

# Usage: deploy-citizenship-api.sh <publish_dir>
SRC="${1:?Usage: deploy-citizenship-api.sh <publish_dir>}"
BASE="/var/www/citizenship-api"
stamp="$(date +%F_%H%M%S)"
rel="$BASE/releases/$stamp"

# Validate publish output
if [ ! -f "$SRC/Api.dll" ]; then
  echo "ERROR: $SRC/Api.dll not found (publish output invalid)"
  exit 1
fi

mkdir -p "$rel"
rsync -a --delete "$SRC/" "$rel/"

ln -sfn "$rel" "$BASE/current"
chown -R dev:dev "$rel"

systemctl restart citizenship-api

echo "OK deployed: $rel"
echo "[$(date -Is)] Deploy done"

# Keep only last 10 releases
cd "$BASE/releases"
ls -1t | tail -n +11 | xargs -r rm -rf
