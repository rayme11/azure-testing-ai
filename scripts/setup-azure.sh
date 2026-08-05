#!/bin/bash
# =============================================================================
# AI Testing Assistant - Azure Setup Script
# =============================================================================
# Automates the creation of Azure resources for the project.
#
# Prerequisites:
#   - Azure CLI installed and logged in (az login)
#   - Active Azure subscription
#
# Usage:
#   ./scripts/setup-azure.sh
# =============================================================================

set -e  # Exit on error

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Configuration
RESOURCE_GROUP="rg-ai-testing-assistant"
LOCATION="eastus"
OPENAI_NAME="aoai-ai-testing-assistant"
KEYVAULT_NAME="kv-ai-testing-$(date +%s | tail -c 6)"

# Azure CLI path
AZ="~/azure-cli-env/bin/az"

echo -e "${BLUE}🚀 Setting up Azure resources for AI Testing Assistant${NC}"
echo "============================================================"

# Verify Azure CLI
if ! command -v $AZ &> /dev/null; then
    echo -e "${RED}✗ Azure CLI not found${NC}"
    echo "Please run: source scripts/activate-env.sh"
    exit 1
fi

# Check login
echo -e "\n${YELLOW}🔐 Checking Azure login...${NC}"
$AZ account show &> /dev/null || {
    echo -e "${RED}✗ Not logged in. Running 'az login'...${NC}"
    $AZ login
}

SUBSCRIPTION=$($AZ account show --query name -o tsv)
echo -e "${GREEN}✓ Logged in as: $SUBSCRIPTION${NC}"

# Create Resource Group
echo -e "\n${YELLOW}📦 Creating Resource Group...${NC}"
$AZ group create \
    --name $RESOURCE_GROUP \
    --location $LOCATION \
    --tags environment=training project=ai-testing cost-center=learning \
    --output none

echo -e "${GREEN}✓ Resource Group created: $RESOURCE_GROUP${NC}"

# Create Azure OpenAI
echo -e "\n${YELLOW}🤖 Creating Azure OpenAI Service...${NC}"
$AZ cognitiveservices account create \
    --name $OPENAI_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --kind OpenAI \
    --sku S0 \
    --output none

echo -e "${GREEN}✓ OpenAI Service created: $OPENAI_NAME${NC}"

# Deploy GPT-3.5 model
echo -e "\n${YELLOW}📝 Deploying GPT-3.5 model...${NC}"
$AZ cognitiveservices account deployment create \
    --name $OPENAI_NAME \
    --resource-group $RESOURCE_GROUP \
    --deployment-name gpt-35-turbo \
    --model-name gpt-35-turbo \
    --model-version "0613" \
    --model-format OpenAI \
    --sku-capacity 1 \
    --sku-name Standard \
    --output none

echo -e "${GREEN}✓ Model deployed: gpt-35-turbo${NC}"

# Create Key Vault
echo -e "\n${YELLOW}🔑 Creating Key Vault...${NC}"
$AZ keyvault create \
    --name $KEYVAULT_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --enable-rbac-authorization true \
    --output none

echo -e "${GREEN}✓ Key Vault created: $KEYVAULT_NAME${NC}"

# Get credentials
echo -e "\n${YELLOW}📋 Retrieving credentials...${NC}"
ENDPOINT=$($AZ cognitiveservices account show \
    --name $OPENAI_NAME \
    --resource-group $RESOURCE_GROUP \
    --query properties.endpoint -o tsv)

API_KEY=$($AZ cognitiveservices account keys list \
    --name $OPENAI_NAME \
    --resource-group $RESOURCE_GROUP \
    --query key1 -o tsv)

# Store in Key Vault
echo -e "\n${YELLOW}🔒 Storing secrets in Key Vault...${NC}"
$AZ keyvault secret set \
    --vault-name $KEYVAULT_NAME \
    --name AzureOpenAI-Endpoint \
    --value "$ENDPOINT" \
    --output none

$AZ keyvault secret set \
    --vault-name $KEYVAULT_NAME \
    --name AzureOpenAI-ApiKey \
    --value "$API_KEY" \
    --output none

echo -e "${GREEN}✓ Secrets stored in Key Vault${NC}"

# Summary
echo ""
echo -e "${GREEN}============================================================${NC}"
echo -e "${GREEN}✅ Azure Setup Complete!${NC}"
echo -e "${GREEN}============================================================${NC}"
echo ""
echo -e "${BLUE}Resource Group:${NC} $RESOURCE_GROUP"
echo -e "${BLUE}Location:${NC} $LOCATION"
echo -e "${BLUE}OpenAI Service:${NC} $OPENAI_NAME"
echo -e "${BLUE}Key Vault:${NC} $KEYVAULT_NAME"
echo ""
echo -e "${YELLOW}⚠️  Save these credentials securely:${NC}"
echo -e "${BLUE}Endpoint:${NC} $ENDPOINT"
echo -e "${BLUE}API Key:${NC} [Stored in Key Vault: $KEYVAULT_NAME]"
echo ""
echo -e "${GREEN}Next steps:${NC}"
echo "  1. Save the endpoint and API key"
echo "  2. Configure user secrets: dotnet user-secrets set ..."
echo "  3. Run the application: dotnet run"
echo ""
