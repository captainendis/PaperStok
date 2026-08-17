# Copyright (c) 2026 PaperAxis. All rights reserved.
# This file is part of PaperStok. Unauthorized copying, modification
# or distribution of this file is strictly prohibited.
#
# Portable PaperStok.exe uretir (self-contained, tek dosya, win-x64).
$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")

dotnet publish src/PaperStok.App/PaperStok.App.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o dist

Write-Host "Portable derleme tamamlandi: dist/PaperStok.exe"
