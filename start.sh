#!/bin/bash

# Start script for Retail Brand Agent

# Make sure the scripts are executable
chmod +x startbackend.sh startfrontend.sh

# Start the backend application in the background
./startbackend.sh &

# Function to check if the backend is running
check_backend() {
    # Replace 5000 with the port your backend listens on
    local BACKEND_PORT=5000
    nc -z localhost $BACKEND_PORT
}

# Initial wait time to give the backend time to start
INITIAL_WAIT=3
echo "Waiting for $INITIAL_WAIT seconds for the backend to start..."
sleep $INITIAL_WAIT

# Wait for the backend to be ready
echo "Checking if the backend is ready..."
MAX_WAIT=120  # Maximum wait time in seconds
WAIT_INTERVAL=5  # Interval between checks in seconds
elapsed=0

while ! check_backend; do
    if [ $elapsed -ge $MAX_WAIT ]; then
        echo "Backend did not start within $MAX_WAIT seconds. Exiting."
        exit 1
    fi
    echo "Backend not ready yet, waiting for $WAIT_INTERVAL seconds..."
    sleep $WAIT_INTERVAL
    elapsed=$((elapsed + WAIT_INTERVAL))
done

echo "Backend is ready."

# Start the frontend application
./startfrontend.sh
