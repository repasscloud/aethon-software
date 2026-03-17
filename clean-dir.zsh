#!/usr/bin/env zsh

find . -maxdepth 3 -type f -name "*.sln" ! -path "./Aethon.sln" -print -delete

pkill -f "Visual Studio Code" 2>/dev/null

# remove VS Code workspace state
rm -rf .vscode

# remove VS Code per-workspace cache (macOS)
rm -rf ~/Library/Application\ Support/Code/User/workspaceStorage/*

# remove C# / Roslyn / Razor caches
rm -rf ~/.omnisharp
rm -rf ~/.cache/omnisharp 2>/dev/null

# remove solution + build artifacts
rm -rf .vs
find . -name bin -type d -prune -exec rm -rf {} \;
find . -name obj -type d -prune -exec rm -rf {} \;

# restore + build clean
dotnet clean
dotnet restore
dotnet build

code .