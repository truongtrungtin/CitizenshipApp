#!/usr/bin/env bash
set -euo pipefail

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

# Keep only last 10 releases
cd "$BASE/releases"
ls -1t | tail -n +11 | xargs -r rm -rf
