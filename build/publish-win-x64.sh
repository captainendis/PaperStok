#!/usr/bin/env bash
# Copyright (c) 2026 PaperAxis. All rights reserved.
# This file is part of PaperStok. Unauthorized copying, modification
# or distribution of this file is strictly prohibited.
#
# Portable PaperStok.exe üretir (self-contained, tek dosya, win-x64).
# WPF XAML derlemesi yalnızca Windows üzerinde .NET 8 SDK ile çalışır;
# bu betiği Windows'ta (PowerShell/Git Bash/WSL) çalıştırın.
set -euo pipefail

cd "$(dirname "$0")/.."

dotnet publish src/PaperStok.App/PaperStok.App.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o dist

echo "Portable derleme tamamlandı: dist/PaperStok.exe"
