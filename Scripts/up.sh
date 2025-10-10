#!/bin/bash
# Change to the script directory
cd "$(dirname "$0")"

# Default environment
ENVIRONMENT=Production

# Check for --dev flag and update .env file
for arg in "$@"; do
    if [ "$arg" == "--dev" ]; then
        ENVIRONMENT=Development
        break
    fi
done

# Update .env file with the environment
echo "ASPNETCORE_ENVIRONMENT=$ENVIRONMENT" > .env

# Start the services using the .env file
docker-compose up -d

if [ $? -eq 0 ]; then
    echo "Services started successfully in $ENVIRONMENT environment."
else
    echo "Failed to start services."
    exit 1
fi