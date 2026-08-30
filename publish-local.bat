@echo off
setlocal EnableExtensions

cd /d "%~dp0"
set "PROJECT=src\TabulariusAI.Web\TabulariusAI.Web.csproj"
set "VERSION=0.1.0"
set "RELEASE_NAME=TabulariusAI-%VERSION%-win-x64"
set "OUTPUT=artifacts\release\%RELEASE_NAME%"
set "ZIP=artifacts\release\%RELEASE_NAME%.zip"

echo.
echo Tabularius AI %VERSION% - Windows local release
echo =================================================
echo.

if exist "artifacts\release" rmdir /s /q "artifacts\release"
mkdir "%OUTPUT%"
if errorlevel 1 goto error

echo [1/4] Restoring dependencies...
dotnet restore "%PROJECT%" -r win-x64
if errorlevel 1 goto error

echo [2/4] Publishing self-contained application...
dotnet publish "%PROJECT%" -c Release -r win-x64 --self-contained true -o "%OUTPUT%" --no-restore /p:Version=%VERSION% /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 goto error

echo [3/4] Preparing clean local data directories...
if exist "%OUTPUT%\data\tabularius.db" del /q "%OUTPUT%\data\tabularius.db"
if exist "%OUTPUT%\data\tabularius.db-shm" del /q "%OUTPUT%\data\tabularius.db-shm"
if exist "%OUTPUT%\data\tabularius.db-wal" del /q "%OUTPUT%\data\tabularius.db-wal"
if exist "%OUTPUT%\logs" rmdir /s /q "%OUTPUT%\logs"
if not exist "%OUTPUT%\data" mkdir "%OUTPUT%\data"
mkdir "%OUTPUT%\logs"
if errorlevel 1 goto error

echo [4/4] Creating distribution ZIP...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%OUTPUT%\*' -DestinationPath '%ZIP%' -Force"
if errorlevel 1 goto error

echo.
echo Release package complete.
echo Folder:     %OUTPUT%
echo Executable: %OUTPUT%\TabulariusAI.Web.exe
echo ZIP:        %ZIP%
echo Database:   %OUTPUT%\data\tabularius.db ^(created on first run^)
echo.
exit /b 0

:error
echo.
echo ERROR: The Windows local release package failed.
exit /b 1
