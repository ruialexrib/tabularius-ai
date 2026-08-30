@echo off
setlocal EnableExtensions EnableDelayedExpansion
title Tabularius AI - Auto Update and Run

cd /d "%~dp0"

set "BRANCH=main"
set "CHECK_INTERVAL=10"
set "APP_PROJECT=src\TabulariusAI.Web"
set "APP_PID_FILE=%TEMP%\tabularius-ai.pid"
set "APP_LOG_FILE=%TEMP%\tabularius-ai.log"
set "APP_URL=http://localhost:5000"
set "STARTUP_TIMEOUT=30"

echo.
echo Tabularius AI development runner
echo Watching origin/%BRANCH% every %CHECK_INTERVAL% seconds.
echo Press Ctrl+C to stop.
echo.

where git >nul 2>&1
if errorlevel 1 (
    echo ERROR: Git was not found in PATH.
    goto fatal_error
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK was not found in PATH.
    goto fatal_error
)

if not exist "%APP_PROJECT%\TabulariusAI.Web.csproj" (
    echo ERROR: Project file was not found at %APP_PROJECT%.
    goto fatal_error
)

call :initial_build
if errorlevel 1 goto fatal_error

call :ensure_running
if errorlevel 1 goto operation_error

:watch
call :check_update
if errorlevel 2 goto retry
if errorlevel 1 goto rebuild

call :ensure_running
if errorlevel 1 goto operation_error
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
dotnet build --no-restore
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
echo Check %APP_LOG_FILE% for application startup errors.
echo The watcher remains active and will retry.
timeout /t %CHECK_INTERVAL% /nobreak >nul
goto watch

:fatal_error
echo.
echo Tabularius AI could not be started.
pause
exit /b 1

:initial_build
echo [startup 1/2] Restoring dependencies...
dotnet restore
if errorlevel 1 exit /b 1
echo [startup 2/2] Building application...
dotnet build --no-restore
if errorlevel 1 exit /b 1
exit /b 0

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
    exit /b !errorlevel!
)
set /p APP_PID=<"%APP_PID_FILE%"
tasklist /FI "PID eq !APP_PID!" 2>nul | find "!APP_PID!" >nul
if errorlevel 1 (
    echo Application process stopped. Starting it again...
    del /q "%APP_PID_FILE%" >nul 2>&1
    call :start_app
    exit /b !errorlevel!
)
exit /b 0

:start_app
set "APP_PID="
del /q "%APP_LOG_FILE%" >nul 2>&1
for /f %%i in ('powershell -NoProfile -Command "$p = Start-Process dotnet -ArgumentList @('run','--project','%APP_PROJECT%','--no-build','--urls','%APP_URL%') -WorkingDirectory '%CD%' -RedirectStandardOutput '%APP_LOG_FILE%' -RedirectStandardError '%APP_LOG_FILE%.err' -PassThru; $p.Id"') do set "APP_PID=%%i"
if not defined APP_PID exit /b 1
>"%APP_PID_FILE%" echo !APP_PID!

echo Waiting for Tabularius AI at %APP_URL%...
for /L %%s in (1,1,%STARTUP_TIMEOUT%) do (
    tasklist /FI "PID eq !APP_PID!" 2>nul | find "!APP_PID!" >nul
    if errorlevel 1 (
        echo ERROR: Application process exited during startup.
        if exist "%APP_LOG_FILE%" type "%APP_LOG_FILE%"
        if exist "%APP_LOG_FILE%.err" type "%APP_LOG_FILE%.err"
        del /q "%APP_PID_FILE%" >nul 2>&1
        exit /b 1
    )
    powershell -NoProfile -Command "try { $r = Invoke-WebRequest -UseBasicParsing -Uri '%APP_URL%' -TimeoutSec 2; exit 0 } catch { if ($_.Exception.Response) { exit 0 } else { exit 1 } }" >nul 2>&1
    if not errorlevel 1 (
        echo Tabularius AI started with PID !APP_PID!.
        echo Opening %APP_URL%...
        start "" "%APP_URL%"
        exit /b 0
    )
    timeout /t 1 /nobreak >nul
)

echo ERROR: Tabularius AI did not become available within %STARTUP_TIMEOUT% seconds.
if exist "%APP_LOG_FILE%" type "%APP_LOG_FILE%"
if exist "%APP_LOG_FILE%.err" type "%APP_LOG_FILE%.err"
call :stop_app
exit /b 1

:stop_app
if not exist "%APP_PID_FILE%" exit /b 0
set /p APP_PID=<"%APP_PID_FILE%"
echo Stopping current application process !APP_PID!...
taskkill /PID !APP_PID! /T /F >nul 2>&1
del /q "%APP_PID_FILE%" >nul 2>&1
exit /b 0
