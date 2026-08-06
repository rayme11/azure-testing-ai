#!/bin/bash
# =============================================================================
# AI Testing Assistant - Container App Deployment Script
# =============================================================================

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
RESOURCE_GROUP="rg-ai-testing-assistant"
LOCATION="eastus"
ENVIRONMENT="dev"
AZ="$HOME/azure-cli-env/bin/az"

echo -e "${BLUE}🚀 Deploying AI Testing Assistant Container App${NC}"
echo "============================================================"

# Check Azure CLI
if ! $AZ account show &> /dev/null; then
    echo -e "${YELLOW}Not logged in. Running 'az login'...${NC}"
    $AZ login
fi

# Fix macOS quarantine if needed
xattr -dr com.apple.quarantine "$HOME/azure-cli-env" 2>/dev/null || true

SUBSCRIPTION=$($AZ account show --query name -o tsv)
echo -e "${GREEN}✓ Logged in as: $SUBSCRIPTION${NC}"

# Get OpenAI details
echo -e "\n${YELLOW}🔍 Getting OpenAI configuration...${NC}"
OPENAI_ENDPOINT=$($AZ cognitiveservices account show \
    --name aitesting-openai-dev \
    --resource-group $RESOURCE_GROUP \
    --query properties.endpoint -o tsv)

OPENAI_KEY=$($AZ cognitiveservices account keys list \
    --name aitesting-openai-dev \
    --resource-group $RESOURCE_GROUP \
    --query key1 -o tsv)

echo -e "${GREEN}✓ OpenAI Endpoint: $OPENAI_ENDPOINT${NC}"

# Check if Container Apps environment exists
echo -e "\n${YELLOW}🏗️ Deploying Container App...${NC}"

# Deploy Container App using Bicep
DEPLOYMENT_OUTPUT=$($AZ deployment group create \
    --resource-group $RESOURCE_GROUP \
    --template-file container-app.bicep \
    --parameters environmentName=$ENVIRONMENT \
                 openAiEndpoint=$OPENAI_ENDPOINT \
                 openAiApiKey=$OPENAI_KEY \
    --query properties.outputs -o json)

echo -e "${GREEN}✓ Container App deployed!${NC}"

# Get the URL
CONTAINER_URL=$(echo $DEPLOYMENT_OUTPUT | python3 -c "import sys,json; print(json.load(sys.stdin).get('containerAppUrl',{}).get('value',''))")

echo -e "\n${BLUE}📋 Deployment Summary${NC}"
echo "============================================================"
echo -e "Container App URL: ${GREEN}$CONTAINER_URL${NC}"
echo -e "Resource Group: ${GREEN}$RESOURCE_GROUP${NC}"

echo -e "\n${YELLOW}⚠️ Important:${NC}"
echo "The container is deployed but needs your application image."
echo "Next: Build and push your Docker image to Azure Container Registry"

echo -e "\n${BLUE}Next Steps:${NC}"
echo "1. Create Azure Container Registry"
echo "2. Build and push your Docker image"
echo "3. Update Container App to use your image"
