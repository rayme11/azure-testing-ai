# 🛠️ Development Guide

> **How to work with this project day-to-day**

---

## 📋 Quick Reference

### Activate Development Environment

```bash
# Navigate to project
cd "/Users/raymaldonado/Library/CloudStorage/GoogleDrive-vivachihuahua2004@gmail.com/My Drive/Code/AI_Testing_Assistant"

# Activate both Python and Azure CLI
source scripts/activate-env.sh
```

**You'll see:**
```
🤖 AI Testing Assistant - Environment Setup
================================================
✓ Python virtual environment activated
  Python: Python 3.9.6
  Path: /Users/.../AI_Testing_Assistant/.venv
✓ Azure CLI alias set
✓ .NET aliases set
📁 Working directory: /Users/.../AI_Testing_Assistant

🚀 Ready to code!
```

---

## 🔄 Daily Workflow

### 1. Start Your Day

```bash
# 1. Open terminal and activate environment
source scripts/activate-env.sh

# 2. Pull latest changes (if collaborating)
git pull

# 3. Verify Azure login
az account show

# 4. Build and test
build
test
```

### 2. Working on Features

**For .NET development:**
```bash
# Build
build

# Run specific test
dotnet test --filter "FullyQualifiedName~DocumentTests"

# Run API with hot reload
dotnet watch run --project src/LegalDocManager.Api

# Add migration
dotnet ef migrations add MigrationName --project src/LegalDocManager.Core
```

**For Python/AI components:**
```bash
# Python is already activated from source scripts/activate-env.sh

# Run Python script
python scripts/index_codebase.py

# Run Jupyter notebook
jupyter notebook

# Install new package
pip install package-name
```

### 3. Testing Your Changes

```bash
# Run all tests
test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Run Playwright E2E tests
cd tests/E2E
npm install  # First time only
npx playwright test

# Run specific test
npx playwright test document-flow.spec.ts
```

### 4. Azure Operations

```bash
# Check resource status
az group show --name rg-ai-testing-assistant

# View OpenAI usage (costs money!)
az cognitiveservices account show \
  --name aoai-ai-testing-assistant \
  --resource-group rg-ai-testing-assistant

# Check costs
az consumption usage list --top 10
```

---

## 🐍 Python Environment Deep Dive

### Structure

```
AI_Testing_Assistant/
├── .venv/                    # Python virtual environment
│   ├── bin/                  # Scripts (activate, pip, python)
│   ├── lib/                  # Python packages
│   └── pyvenv.cfg           # Environment config
│
├── requirements.txt          # Python dependencies
└── notebooks/               # Jupyter experiments
```

### Common Python Commands

```bash
# After: source scripts/activate-env.sh

# Check Python path
which python
# Output: /Users/.../AI_Testing_Assistant/.venv/bin/python

# Check installed packages
pip list

# Add new dependency
# 1. Edit requirements.txt
# 2. Install
pip install -r requirements.txt

# Freeze current packages (save state)
pip freeze > requirements.freeze.txt

# Deactivate (when done)
deactivate
```

### Creating Python Scripts

```bash
# Create a new script
mkdir -p scripts
cat > scripts/analyze_tests.py << 'EOF'
#!/usr/bin/env python3
"""Analyze test results and generate reports."""

import json
import sys
from pathlib import Path

def main():
    print("🧪 Test Analysis Tool")
    print("=" * 50)
    # Your code here

if __name__ == "__main__":
    main()
EOF

# Make executable
chmod +x scripts/analyze_tests.py

# Run it
python scripts/analyze_tests.py
```

### Jupyter Notebooks for Experimentation

```bash
# Start Jupyter
jupyter notebook

# Or use VS Code:
# 1. Open .ipynb file
# 2. Select kernel: Python 3.9.6 ('.venv': venv)
```

---

## ☁️ Azure Environment Deep Dive

### Azure CLI Setup

```bash
# After: source scripts/activate-env.sh

# The 'az' command is aliased to use the virtual environment installation
which az
# Output: aliased to ~/azure-cli-env/bin/az

# Login (if not already)
az login

# Set default subscription
az account set --subscription "Your Subscription Name"

# Verify
az account show
```

### Cost Management

```bash
# Set budget alerts
az monitor metrics alert create \
  --name "BudgetAlert-10USD" \
  --resource-group rg-ai-testing-assistant \
  --scopes $(az group show --name rg-ai-testing-assistant --query id -o tsv) \
  --condition "totalcost > 10" \
  --description "Alert when spending exceeds $10"

# Check current costs
az consumption usage list \
  --start-date $(date -v-30d +%Y-%m-%d) \
  --end-date $(date +%Y-%m-%d) \
  | jq '.[] | {date: .usageStart, cost: .pretaxCost}'
```

### Monitoring Resources

```bash
# List all resources
az resource list \
  --resource-group rg-ai-testing-assistant \
  --output table

# Check OpenAI quotas
az cognitiveservices account show \
  --name aoai-ai-testing-assistant \
  --resource-group rg-ai-testing-assistant \
  --query "properties.endpoint"
```

---

## 🧪 Testing Strategy

### Test Pyramid

```
        /\
       /  \
      / E2E \     <- Playwright (few tests, expensive)
     /--------\
    / Integration \ <- API tests, DB tests
   /----------------\
  /     Unit Tests    \ <- xUnit (many tests, fast)
 /-----------------------\
```

### Running Tests

```bash
# Fast feedback loop - Unit tests only
dotnet test tests/Unit --filter "Category!=Slow"

# Before commit - All tests
dotnet test

# CI/CD mode
dotnet test --verbosity minimal --logger trx

# With HTML report
dotnet test --logger "html;LogFileName=test-results.html"
```

### Test Organization

```
tests/
├── Unit/
│   ├── Services/
│   │   └── DocumentServiceTests.cs
│   ├── Validators/
│   │   └── DocumentValidatorTests.cs
│   └── Skills/
│       └── RAGTestGenerationSkillTests.cs
│
├── Integration/
│   ├── Api/
│   │   └── DocumentEndpointsTests.cs
│   └── Database/
│       └── DocumentRepositoryTests.cs
│
└── E2E/
    ├── specs/
    │   └── document-management.spec.ts
    └── pages/
        └── DocumentsPage.ts
```

---

## 🔧 Troubleshooting

### Python Issues

**Problem**: `ModuleNotFoundError` after activating environment
```bash
# Solution: Reinstall requirements
pip install -r requirements.txt --force-reinstall
```

**Problem**: Jupyter kernel not found
```bash
# Install kernel
pip install ipykernel
python -m ipykernel install --user --name=.venv --display-name "Python (.venv)"
```

### Azure Issues

**Problem**: `az login` fails
```bash
# Clear cache and retry
rm -rf ~/.azure
az login
```

**Problem**: Resource not found
```bash
# Verify subscription
az account show
az account list --output table

# Set correct subscription
az account set --subscription "Azure subscription 1"
```

### .NET Issues

**Problem**: Build fails with package restore errors
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore
dotnet restore
```

**Problem**: Port already in use
```bash
# Find and kill process
lsof -ti:7001 | xargs kill -9

# Or use different port
dotnet run --urls "https://localhost:7002"
```

---

## 📊 Development Metrics

Track your progress:

```bash
# Code statistics
cloc src/ tests/

# Test coverage report
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:coveragereport

# Git activity
git log --graph --oneline --all --decorate
```

---

## 🎯 Next Steps Checklist

### Week 1: Foundation
- [ ] Azure resources created
- [ ] Environment activated successfully
- [ ] API runs locally
- [ ] First test passes

### Week 2: AI Skills
- [ ] Semantic Kernel integrated
- [ ] First skill implemented
- [ ] RAG pipeline working
- [ ] Vector store populated

### Week 3: Testing
- [ ] Self-healing mechanism prototyped
- [ ] Impact analysis working
- [ ] Test coverage > 70%

### Week 4: CI/CD
- [ ] Azure DevOps pipeline created
- [ ] Bicep templates deployed
- [ ] Automated testing in CI

---

## 💡 Pro Tips

1. **Use VS Code**: It handles both .NET and Python seamlessly
2. **Keep terminal open**: With environment activated
3. **Commit often**: Small, focused commits
4. **Document experiments**: Use Jupyter notebooks
5. **Monitor costs**: Check Azure spending weekly
6. **Save credentials**: In Key Vault, never in code

---

## 📞 Getting Help

**Azure Issues:**
- Azure Portal: https://portal.azure.com
- Azure Docs: https://docs.microsoft.com/azure
- Status: https://status.azure.com

**Project Issues:**
- Check logs: `logs/` directory
- Run diagnostics: `scripts/diagnose.sh`
- Review: `README.md` and this guide

---

> **Happy coding! 🚀**
