@description('Environment name (dev, staging, prod)')
param environmentName string = 'dev'

@description('Azure region for most resources')
param location string = resourceGroup().location

@description('Azure region for OpenAI (must support GPT models)')
param openAiLocation string = 'westus3'

@description('Project name prefix')
param projectName string = 'aitesting'

// Resource naming
var uniqueSuffix = take(uniqueString(resourceGroup().id), 8)
var openAiName = '${projectName}-openai-${environmentName}'
var keyVaultName = 'kv${uniqueSuffix}'
var appServiceName = '${projectName}-api-${environmentName}'
var appServicePlanName = '${projectName}-plan-${environmentName}'
var logAnalyticsName = '${projectName}-logs-${environmentName}'
var appInsightsName = '${projectName}-insights-${environmentName}'

// Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// Azure OpenAI
resource openAi 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: openAiName
  location: openAiLocation
  kind: 'OpenAI'
  sku: {
    name: 'S0'
  }
  properties: {
    customSubDomainName: openAiName
    publicNetworkAccess: 'Enabled'
  }
}

// GPT-4o Deployment - stable, widely available
resource gpt4oDeployment 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: openAi
  name: 'gpt-4o'
  sku: {
    name: 'Standard'
    capacity: 1
  }
  properties: {
    model: {
      format: 'OpenAI'
      name: 'gpt-4o'
      version: '2024-11-20'
    }
  }
}

// Azure Functions - Consumption plan (no VM quota required)
resource functionApp 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceName
  location: location
  kind: 'functionapp'
  properties: {
    httpsOnly: true
    siteConfig: {
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'AzureOpenAI__Endpoint'
          value: openAi.properties.endpoint
        }
        {
          name: 'AzureOpenAI__DeploymentName'
          value: 'gpt-4o'
        }
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enabledForTemplateDeployment: true
  }
}

// Store OpenAI Key in Key Vault
resource openAiKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'AzureOpenAI--ApiKey'
  properties: {
    value: openAi.listKeys().key1
  }
}

// Outputs
output openAiEndpoint string = openAi.properties.endpoint
output appServiceUrl string = 'https://${functionApp.properties.defaultHostName}'
output keyVaultName string = keyVault.name
output appInsightsName string = appInsights.name
