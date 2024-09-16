#!/bin/bash

# Stop script for Retail Brand Assistant

# Define the port your backend is running on
BACKEND_PORT=5000

# Find the process ID (PID) of the running backend application using the port
PID=$(lsof -t -i:$BACKEND_PORT)

if [ -z "$PID" ]; then
    echo "No running instance of the backend Retail Brand Assistant found on port $BACKEND_PORT."
else
    echo "Stopping backend Retail Brand Assistant (PID: $PID)..."
    kill $PID
    echo "Backend Retail Brand Assistant stopped."
fi

# Stop the frontend application
echo "Stopping the frontend application..."

# Find the process ID of the running frontend application using the default React port 3000
FRONTEND_PORT=3000
FRONTEND_PID=$(lsof -t -i:$FRONTEND_PORT)

if [ -z "$FRONTEND_PID" ]; then
    echo "No running instance of the frontend application found on port $FRONTEND_PORT."
else
    echo "Stopping frontend application (PID: $FRONTEND_PID)..."
    kill $FRONTEND_PID
    echo "Frontend application stopped."
fi
