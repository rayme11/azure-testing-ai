#!/bin/bash
# =============================================================================
# AI Testing Assistant - Environment Activation Script
# =============================================================================
# This script activates both the Python virtual environment and sets up
# convenient aliases for working with the project.
#
# Usage:
#   source scripts/activate-env.sh
# =============================================================================

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🤖 AI Testing Assistant - Environment Setup${NC}"
echo "================================================"

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJECT_ROOT="$( cd "$SCRIPT_DIR/.." && pwd )"

# Activate Python virtual environment
if [ -f "$PROJECT_ROOT/.venv/bin/activate" ]; then
    source "$PROJECT_ROOT/.venv/bin/activate"
    echo -e "${GREEN}✓ Python virtual environment activated${NC}"
    echo "  Python: $(python --version)"
    echo "  Path: $PROJECT_ROOT/.venv"
else
    echo -e "${RED}✗ Python virtual environment not found${NC}"
    echo "  Run: python3 -m venv .venv"
    return 1
fi

# Set up Azure CLI alias
alias az='~/azure-cli-env/bin/az'
echo -e "${GREEN}✓ Azure CLI alias set${NC}"
echo "  Usage: az [command]"

# Set up .NET aliases
alias build='dotnet build'
alias test='dotnet test'
alias run='dotnet run --project src/LegalDocManager.Api'
echo -e "${GREEN}✓ .NET aliases set${NC}"
echo "  build - Build solution"
echo "  test  - Run all tests"
echo "  run   - Run API"

# Set up Python aliases
alias py='python'
alias pytest='python -m pytest'
echo -e "${GREEN}✓ Python aliases set${NC}"

# Change to project root
cd "$PROJECT_ROOT"
echo ""
echo -e "${YELLOW}📁 Working directory: $(pwd)${NC}"
echo ""
echo -e "${BLUE}Available commands:${NC}"
echo "  az              - Azure CLI"
echo "  build           - Build .NET solution"
echo "  test            - Run .NET tests"
echo "  run             - Run API"
echo "  py              - Python interpreter"
echo "  pytest          - Run Python tests"
echo "  deactivate      - Exit Python environment"
echo ""
echo -e "${GREEN}🚀 Ready to code!${NC}"
