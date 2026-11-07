#!/bin/bash
set -e

if [[ -z "$1" || -z "$2" ]]; then
    echo "Usage: $0 <docker-file> <image-version>"
    exit 1
fi

DOCKERFILE="$1"
VERSION="$2"

podman build --build-arg APP_VERSION=$VERSION -t docker.io/rodrigopanta/products-api:$VERSION -f $DOCKERFILE
podman tag rodrigopanta/products-api:$VERSION docker.io/rodrigopanta/products-api:$VERSION
podman tag rodrigopanta/products-api:$VERSION docker.io/rodrigopanta/products-api:latest
podman login docker.io
podman push docker.io/rodrigopanta/products-api:$VERSION
podman push docker.io/rodrigopanta/products-api:latest

kubectl set image deployment/products-api \
  api=rodrigopanta/products-api:$VERSION