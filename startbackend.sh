#!/bin/bash

# Start script for the backend

# Check if .env file exists in the backend directory
if [ ! -f skbackend/.env ]; then
    echo "Error: skbackend/.env file not found. Please run setup.sh first."
    exit 1
fi

# Load environment variables from the backend .env file
export $(grep -v '^#' skbackend/.env | xargs)

# Update Today's Date in SystemMessage.txt
today=$(date +%m/%d/%Y)
awk -v date="$today" '
    /\[Today'"'"'s Date\]/ { $0 = "[Today'"'"'s Date]=" date }
    { print }
' skbackend/SystemMessage.txt > skbackend/SystemMessage.tmp && mv skbackend/SystemMessage.tmp skbackend/SystemMessage.txt

echo "Updated Today's Date in SystemMessage.txt to $today"

# Start the backend application
cd skbackend
dotnet run --project .
