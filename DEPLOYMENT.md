# 🚀 Deployment Guide

## Architecture Overview

This project uses **Azure Container Apps** with **Azure Container Registry** for a cloud-native, scalable deployment.

```
GitHub Repository
    ↓ Push to main
GitHub Actions (CI/CD)
    ↓ Build & Test
Azure Container Registry (ACR)
    ↓ Store Image
Azure Container Apps (ACA)
    ↓ Run Container
Azure OpenAI Service
```

## ✅ Current Status

| Resource | Status | URL |
|----------|--------|-----|
| Azure OpenAI | ✅ Running | `https://aitesting-openai-dev.openai.azure.com/` |
| Container App | ✅ Running | `https://aitesting-api-dev.happysmoke-8ec83b1a.eastus.azurecontainerapps.io` |
| Container Registry | ✅ Ready | `aitestingreg305.azurecr.io` |

## 🌐 Access Your API

- **Health Check**: `https://aitesting-api-dev.happysmoke-8ec83b1a.eastus.azurecontainerapps.io/health`
- **Swagger UI**: `https://aitesting-api-dev.happysmoke-8ec83b1a.eastus.azurecontainerapps.io/swagger`
- **API Endpoint**: `https://aitesting-api-dev.happysmoke-8ec83b1a.eastus.azurecontainerapps.io/api/TestGeneration/generate`

## 🔧 CI/CD Setup (One-time)

To enable automatic deployments on push:

### 1. Create Azure Service Principal

```bash
az ad sp create-for-rbac \
  --name "github-actions-ai-testing" \
  --role contributor \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/rg-ai-testing-assistant \
  --sdk-auth
```

### 2. Add GitHub Secret

1. Go to your GitHub repository → Settings → Secrets and variables → Actions
2. Click "New repository secret"
3. Name: `AZURE_CREDENTIALS`
4. Value: Paste the JSON output from step 1

### 3. Test the Pipeline

Push to `main` branch and watch the Actions tab!

## 📊 Azure Resources

| Resource | Name | Purpose |
|----------|------|---------|
| Resource Group | `rg-ai-testing-assistant` | Organizes all resources |
| Container App | `aitesting-api-dev` | Runs your API |
| Container Registry | `aitestingreg305` | Stores Docker images |
| OpenAI Service | `aitesting-openai-dev` | GPT-4o model |
| Log Analytics | `aitesting-logs-dev` | Centralized logging |

## 💰 Cost Optimization

- **Container App**: Pay-per-use (scales to 0 when idle)
- **OpenAI**: Pay-per-token (~$0.005 per 1K tokens for GPT-4o)
- **Container Registry**: Basic tier (~$5/month)
- **Estimated Monthly**: $10-20 for light usage

## 🔄 Manual Deployment (if needed)

```bash
# Build and push
az acr build \
  --registry aitestingreg305 \
  --image aitesting-api:v1 \
  --file src/LegalDocManager.Api/Dockerfile .

# Deploy
az containerapp update \
  --name aitesting-api-dev \
  --resource-group rg-ai-testing-assistant \
  --image aitestingreg305.azurecr.io/aitesting-api:v1
```

## 🧪 Testing the API

```bash
# Health check
curl https://aitesting-api-dev.happysmoke-8ec83b1a.eastus.azurecontainerapps.io/health

# Generate tests
curl -X POST \
  https://aitesting-api-dev.happysmoke-8ec83b1a.eastus.azurecontainerapps.io/api/TestGeneration/generate \
  -H "Content-Type: application/json" \
  -d '{
    "requirements": "Test user login with valid and invalid credentials",
    "sessionId": "demo-session-001"
  }'
```

## 📝 Interview Talking Points

**"How did you deploy this?"**

> "I implemented a cloud-native CI/CD pipeline using:
> - **Azure Container Apps** for serverless container hosting
> - **Azure Container Registry** for immutable image storage
> - **GitHub Actions** for automated build, test, and deploy
> - **Infrastructure as Code** with Bicep templates
> 
> This gives us automatic scaling, zero-downtime deployments,
> and full traceability from git commit to production."

## 🚨 Troubleshooting

| Issue | Solution |
|-------|----------|
| Container won't start | Check logs: `az containerapp logs show --name aitesting-api-dev --resource-group rg-ai-testing-assistant` |
| 404 errors | API routes are `/api/TestGeneration/*`, not `/swagger` |
| Slow cold start | Normal for consumption tier - first request may take 5-10 seconds |
| Out of quota | Container Apps use consumption model - no VM quota needed! |
