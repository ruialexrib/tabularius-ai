@echo off
setlocal EnableExtensions

cd /d "%~dp0"
set "PROJECT=src\TabulariusAI.Web\TabulariusAI.Web.csproj"
set "OUTPUT=artifacts\publish\win-x64"

echo.
echo Tabularius AI - Windows local publish
echo =====================================
echo.

if exist "%OUTPUT%" rmdir /s /q "%OUTPUT%"

echo [1/2] Restoring dependencies...
dotnet restore "%PROJECT%" -r win-x64
if errorlevel 1 goto error

echo [2/2] Publishing self-contained application...
dotnet publish "%PROJECT%" -c Release -r win-x64 --self-contained true -o "%OUTPUT%" --no-restore /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 goto error

echo.
echo Publish complete.
echo Executable: %OUTPUT%\TabulariusAI.Web.exe
echo Database:   %OUTPUT%\data\tabularius.db ^(created on first run^)
echo.
exit /b 0

:error
echo.
echo ERROR: The Windows local publish failed.
exit /b 1
