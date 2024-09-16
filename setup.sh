#!/bin/bash

# Setup script for Retail Brand Assistant

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null
then
    echo "dotnet is not installed. Please install .NET SDK and try again."
    exit 1
fi

# Check if npm is installed
if ! command -v npm &> /dev/null
then
    echo "npm is not installed. Please install Node.js and npm and try again."
    exit 1
fi

# Setting up the backend
echo "Setting up the backend..."

# Check if backend directory exists
if [ ! -d "skbackend" ]; then
    echo "Backend directory 'skbackend' does not exist. Creating it..."
    mkdir skbackend
fi

# Navigate to the backend directory
cd skbackend

# Install backend dependencies
dotnet restore

# Create a .env file for environment variables
if [ ! -f .env ]; then
    echo "Creating .env file..."
    cat << EOF > .env
AZURE_OPEN_AI=False

OPENAI_API_KEY=
OPENAI_MODEL_NAME=gpt-4  

AZURE_OPENAI_ENDPOINT=
AZURE_OPENAI_KEY=
AZURE_OPENAI_DEPLOYMENT_NAME=

BING_SUBSCRIPTION_KEY=
CUSTOM_CONFIG_ID=
EOF
    echo "Please edit the .env file and add your API keys and Custom Config ID."
else
    echo ".env file already exists. Please ensure it contains the necessary API keys and Custom Config ID."
fi

# Prompt for brand, brand website, language, market, and locale
read -p "Enter your brand name: " brand_name
read -p "Enter your brand website: " brand_website
read -p "Enter the language (default: English): " language
language=${language:-English}
read -p "Enter the market (default: United States): " market
market=${market:-United States}
read -p "Enter the locale (default: en-us): " locale
locale=${locale:-en-us}

# Update SystemMessage.txt
# Using awk for better cross-platform compatibility
awk -v brand="$brand_name" -v website="$brand_website" -v lang="$language" -v market="$market" -v locale="$locale" '
    /\[Your Brand\]=/ { $0 = "[Your Brand]=" brand }
    /\[Your Website\]=/ { $0 = "[Your Website]=" website }
    /\[Language\]=/ { $0 = "[Language]=" lang }
    /\[Market\]=/ { $0 = "[Market]=" market }
    /\[Locale\]=/ { $0 = "[Locale]=" locale }
    { print }
' SystemMessage.txt > SystemMessage.tmp && mv SystemMessage.tmp SystemMessage.txt

echo "SystemMessage.txt has been updated with your brand information and website."

# Navigate back to the root directory
cd ..

# Setting up the frontend
echo "Setting up the frontend..."

# Check if frontend directory exists
if [ ! -d "reactfrontend" ]; then
    echo "Frontend directory 'reactfrontend' does not exist. Creating it..."
    mkdir reactfrontend
fi

# Navigate to the frontend directory
cd reactfrontend

# Install frontend dependencies
npm install

# Navigate back to the root directory
cd ..

echo "Frontend setup completed."

echo "Setup complete. Please edit the .env file located in the /skbackend directory with your API keys and Bing Custom Config ID before starting the application."