@echo off
chcp 65001 >nul
echo ===== 1. Restore =====
dotnet restore
if %errorlevel% neq 0 exit /b %errorlevel%

echo ===== 2. Build =====
dotnet build --configuration DebugOpt --no-restore /p:WarningsAsErrors=nullable /m
if %errorlevel% neq 0 exit /b %errorlevel%

echo ===== 3. Content.Tests =====
dotnet test --no-build --configuration DebugOpt Content.Tests/Content.Tests.csproj -- NUnit.ConsoleOut=0
if %errorlevel% neq 0 exit /b %errorlevel%

echo ===== 4. Map proto check =====
python Tools/ADT/check_maps_vs_protos.py
if %errorlevel% neq 0 exit /b %errorlevel%

echo ===== 5. Content.IntegrationTests =====
set DOTNET_gcServer=1
dotnet test --no-build --configuration DebugOpt Content.IntegrationTests/Content.IntegrationTests.csproj -- NUnit.ConsoleOut=0 NUnit.MapWarningTo=Failed

echo ===== Done =====
pause
