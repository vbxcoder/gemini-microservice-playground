#!/bin/sh
# This script runs as root

set -e

mkdir -p /data/logs
chown -R node:node /data
exec su-exec node "$@"