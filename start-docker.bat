@echo off
setlocal EnableExtensions

title Tabularius AI - Docker
cd /d "%~dp0"

echo.
echo Tabularius AI - Docker Server
echo ============================
echo.

where docker >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker was not found.
    echo Install or start Docker Desktop and try again.
    goto error
)

docker info >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker is not running.
    echo Start Docker Desktop and try again.
    goto error
)

if not exist ".env" (
    if not exist ".env.example" (
        echo ERROR: .env.example was not found.
        goto error
    )

    copy /y ".env.example" ".env" >nul
    echo A new .env file was created from .env.example.
    echo.
    echo IMPORTANT: Edit .env and replace the example SQL Server password.
    echo Then run start-docker.bat again.
    echo.
    start "" notepad ".env"
    exit /b 0
)

findstr /C:"ChangeThis_StrongPassword_123!" ".env" >nul 2>&1
if not errorlevel 1 (
    echo ERROR: The example database password is still configured in .env.
    echo Replace it with a strong private password before starting the stack.
    start "" notepad ".env"
    goto error
)

echo [1/3] Building and starting containers...
docker compose up -d --build
if errorlevel 1 goto error

echo.
echo [2/3] Container status:
docker compose ps
if errorlevel 1 goto error

echo.
echo [3/3] Opening Tabularius AI...
timeout /t 3 /nobreak >nul
start "" "http://localhost:8080"

echo.
echo Tabularius AI is running at http://localhost:8080
echo.
echo Useful commands:
echo   docker compose ps
echo   docker compose logs -f tabularius-ai-web
echo   docker compose down
echo.
exit /b 0

:error
echo.
echo Docker startup failed.
echo Review the messages above for details.
exit /b 1
