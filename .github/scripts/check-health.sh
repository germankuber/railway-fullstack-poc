#!/usr/bin/env bash
set -euo pipefail

url="$1"
attempts="${2:-12}"
delay_seconds=5

for attempt in $(seq 1 "$attempts"); do
  status_code=$(curl --silent --output /dev/null --write-out '%{http_code}' --max-time 10 "$url" || echo "000")
  echo "GET $url -> $status_code (attempt $attempt/$attempts)"

  if [[ "$status_code" == "200" ]]; then
    exit 0
  fi

  sleep "$delay_seconds"
done

echo "Health check failed for $url"
exit 1
