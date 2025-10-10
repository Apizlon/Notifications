#!/bin/bash
cd "$(dirname "$0")"
BUILD_CONFIG=Release

# Check for --dev flag
for arg in "$@"; do
    if [ "$arg" == "--dev" ]; then
        BUILD_CONFIG=Debug
        break
    fi
done

# Build the Docker image with the specified configuration
docker-compose build --build-arg BUILD_CONFIG=$BUILD_CONFIG

if [ $? -eq 0 ]; then
    echo "Docker images built successfully with configuration $BUILD_CONFIG."
else
    echo "Failed to build Docker image."
    exit 1
fi