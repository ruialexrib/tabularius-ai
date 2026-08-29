@echo off
title Tabularius AI - Update, Build and Run

cd /d "%~dp0"

echo.
echo [1/4] Updating repository...
git pull
if errorlevel 1 goto error

echo.
echo [2/4] Restoring dependencies...
dotnet restore
if errorlevel 1 goto error

echo.
echo [3/4] Building application...
dotnet build
if errorlevel 1 goto error

echo.
echo [4/4] Starting Tabularius AI...
dotnet run --project src\TabulariusAI.Web
if errorlevel 1 goto error
goto end

:error
echo.
echo ERROR: The operation failed.
pause
exit /b 1

:end
pause
