#!/usr/bin/env bash
set -euo pipefail

service="$1"
commit_sha="$2"
timeout_seconds="${3:-600}"
poll_interval_seconds=10
deadline=$((SECONDS + timeout_seconds))

while (( SECONDS < deadline )); do
  status=$(railway deployment list --service "$service" --json \
    | jq -r --arg sha "$commit_sha" 'map(select(.meta.cliMessage == $sha)) | first | .status // "NOT_FOUND"')

  echo "$service deployment for $commit_sha: $status"

  case "$status" in
    SUCCESS) exit 0 ;;
    FAILED | CRASHED | REMOVED | SKIPPED) exit 1 ;;
  esac

  sleep "$poll_interval_seconds"
done

echo "Timed out after ${timeout_seconds}s waiting for $service deployment"
exit 1
