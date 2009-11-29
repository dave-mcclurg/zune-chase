@echo off
REM Clean build is not included with Microsoft Visual C# Express Edition
RMDIR /S /Q "ZuneChase\bin"
RMDIR /S /Q "ZuneChase\obj"
RMDIR /S /Q "ZuneChase\Content\bin"
RMDIR /S /Q "ZuneChase\Content\obj"
RMDIR /S /Q "ZuneChase [windows]\bin"
RMDIR /S /Q "ZuneChase [windows]\obj"
RMDIR /S /Q "ZuneChase [windows]\Content\bin"
RMDIR /S /Q "ZuneChase [windows]\Content\obj"
DEL /Q /F "ZuneChase\*.csproj.Debug.cachefile"
DEL /Q /F "ZuneChase\*.csproj.user"
DEL /Q /F "ZuneChase [windows]\*.csproj.Debug.cachefile"
DEL /Q /F "ZuneChase [windows]\*.csproj.user"
