#!/bin/bash
# =============================================================================
# AI Testing Assistant - Azure Deployment Script
# =============================================================================

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

# Use isolated Azure CLI installation
AZ="$HOME/azure-cli-env/bin/az"

# Fix macOS quarantine if needed
if [ -f "$AZ" ]; then
    xattr -dr com.apple.quarantine "$HOME/azure-cli-env" 2>/dev/null || true
fi

echo -e "${BLUE}🚀 Deploying AI Testing Assistant to Azure${NC}"
echo "============================================================"

# Check Azure CLI
echo -e "${YELLOW}🔧 Checking Azure CLI...${NC}"
if $AZ account show &> /dev/null; then
    echo -e "${GREEN}✓ Azure CLI ready${NC}"
else
    echo -e "${YELLOW}Not logged in. Running 'az login'...${NC}"
    $AZ login
fi

# Check login
echo -e "\n${YELLOW}🔐 Checking Azure login...${NC}"
$AZ account show &> /dev/null || {
    echo -e "${YELLOW}Not logged in. Running 'az login'...${NC}"
    $AZ login
}

SUBSCRIPTION=$($AZ account show --query name -o tsv)
echo -e "${GREEN}✓ Logged in as: $SUBSCRIPTION${NC}"

# Check/Install Bicep CLI
echo -e "\n${YELLOW}🔧 Checking Bicep CLI...${NC}"
$AZ config set bicep.use_binary_from_path=false
if ! $AZ bicep version &> /dev/null 2>&1; then
    echo -e "${YELLOW}Installing Bicep CLI via Azure...${NC}"
    $AZ bicep install
    echo -e "${GREEN}✓ Bicep CLI installed${NC}"
else
    echo -e "${GREEN}✓ Bicep CLI ready${NC}"
fi

# Create Resource Group
echo -e "\n${YELLOW}📦 Creating Resource Group...${NC}"
$AZ group create \
    --name $RESOURCE_GROUP \
    --location $LOCATION \
    --tags environment=$ENVIRONMENT project=ai-testing \
    --output none

echo -e "${GREEN}✓ Resource Group: $RESOURCE_GROUP${NC}"

# Deploy Bicep template
echo -e "\n${YELLOW}🏗️ Deploying infrastructure...${NC}"
DEPLOYMENT_OUTPUT=$($AZ deployment group create \
    --resource-group $RESOURCE_GROUP \
    --template-file main.bicep \
    --parameters environmentName=$ENVIRONMENT \
    --query properties.outputs -o json)

# Extract outputs using Python (available in azure-cli-env)
OPENAI_ENDPOINT=$(echo $DEPLOYMENT_OUTPUT | python3 -c "import sys,json; print(json.load(sys.stdin).get('openAiEndpoint',{}).get('value',''))")
APPSERVICE_URL=$(echo $DEPLOYMENT_OUTPUT | python3 -c "import sys,json; print(json.load(sys.stdin).get('appServiceUrl',{}).get('value',''))")
KEYVAULT_NAME=$(echo $DEPLOYMENT_OUTPUT | python3 -c "import sys,json; print(json.load(sys.stdin).get('keyVaultName',{}).get('value',''))")

echo -e "${GREEN}✓ Deployment complete!${NC}"

echo -e "\n${BLUE}📋 Deployment Summary${NC}"
echo "============================================================"
echo -e "Resource Group: ${GREEN}$RESOURCE_GROUP${NC}"
echo -e "OpenAI Endpoint: ${GREEN}$OPENAI_ENDPOINT${NC}"
echo -e "App Service URL: ${GREEN}$APPSERVICE_URL${NC}"
echo -e "Key Vault: ${GREEN}$KEYVAULT_NAME${NC}"

echo -e "\n${YELLOW}⚠️ Important:${NC}"
echo "1. OpenAI API key is stored in Key Vault"
echo "2. App Service is configured to use Managed Identity"
echo "3. Grant Key Vault access to App Service before deploying code"

echo -e "\n${BLUE}Next Steps:${NC}"
echo "1. Build and publish your .NET app"
echo "2. Configure Key Vault access policy"
echo "3. Deploy to App Service"
