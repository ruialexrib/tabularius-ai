@echo off
setlocal EnableExtensions EnableDelayedExpansion
title Tabularius AI - Auto Update and Run

cd /d "%~dp0"

set "BRANCH=main"
set "CHECK_INTERVAL=10"
set "APP_PROJECT=src\TabulariusAI.Web"
set "APP_PID_FILE=%TEMP%\tabularius-ai.pid"

echo.
echo Tabularius AI development runner
echo Watching origin/%BRANCH% every %CHECK_INTERVAL% seconds.
echo Press Ctrl+C to stop.
echo.

:watch
call :check_update
if errorlevel 2 goto retry
if errorlevel 1 goto rebuild

call :ensure_running
timeout /t %CHECK_INTERVAL% /nobreak >nul
goto watch

:rebuild
echo.
echo ============================================================
echo Repository update detected. Recreating Tabularius AI...
echo ============================================================

call :stop_app

echo [1/4] Updating repository...
git pull --ff-only origin %BRANCH%
if errorlevel 1 goto operation_error

echo [2/4] Restoring dependencies...
dotnet restore
if errorlevel 1 goto operation_error

echo [3/4] Building application...
dotnet build
if errorlevel 1 goto operation_error

echo [4/4] Starting Tabularius AI...
call :start_app
if errorlevel 1 goto operation_error

echo Update complete. Monitoring for new commits...
timeout /t %CHECK_INTERVAL% /nobreak >nul
goto watch

:retry
echo WARNING: Could not check GitHub. Retrying in %CHECK_INTERVAL% seconds...
timeout /t %CHECK_INTERVAL% /nobreak >nul
goto watch

:operation_error
echo.
echo ERROR: Update, restore, build or startup failed.
echo The watcher remains active and will retry when a new commit is detected.
timeout /t %CHECK_INTERVAL% /nobreak >nul
goto watch

:check_update
git fetch --quiet origin %BRANCH%
if errorlevel 1 exit /b 2
for /f %%i in ('git rev-parse HEAD') do set "LOCAL_SHA=%%i"
for /f %%i in ('git rev-parse origin/%BRANCH%') do set "REMOTE_SHA=%%i"
if /i not "!LOCAL_SHA!"=="!REMOTE_SHA!" exit /b 1
exit /b 0

:ensure_running
if not exist "%APP_PID_FILE%" (
    echo Application is not running. Starting it...
    call :start_app
    exit /b
)
set /p APP_PID=<"%APP_PID_FILE%"
tasklist /FI "PID eq !APP_PID!" 2>nul | find "!APP_PID!" >nul
if errorlevel 1 (
    echo Application process stopped. Starting it again...
    del /q "%APP_PID_FILE%" >nul 2>&1
    call :start_app
)
exit /b

:start_app
for /f %%i in ('powershell -NoProfile -Command "$p = Start-Process dotnet -ArgumentList 'run --project %APP_PROJECT% --no-build' -WorkingDirectory '%CD%' -PassThru; $p.Id"') do set "APP_PID=%%i"
if not defined APP_PID exit /b 1
>"%APP_PID_FILE%" echo !APP_PID!
echo Tabularius AI started with PID !APP_PID!.
exit /b 0

:stop_app
if not exist "%APP_PID_FILE%" exit /b 0
set /p APP_PID=<"%APP_PID_FILE%"
echo Stopping current application process !APP_PID!...
taskkill /PID !APP_PID! /T /F >nul 2>&1
del /q "%APP_PID_FILE%" >nul 2>&1
exit /b 0
