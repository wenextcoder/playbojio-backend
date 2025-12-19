@echo off
REM PlayBojio Backend Quick Start for Windows
REM Double-click this file to start the backend

echo ========================================
echo PlayBojio Backend - Quick Start
echo ========================================
echo.

REM Check if SQL Server is running
echo Checking SQL Server...
sc query "MSSQL$SQLEXPRESS" | find "RUNNING" >nul
if errorlevel 1 (
    echo SQL Server is not running. Attempting to start...
    net start "MSSQL$SQLEXPRESS" 2>nul
    if errorlevel 1 (
        echo WARNING: Could not start SQL Server Express
        echo Please start it manually or check the service name
        echo.
    ) else (
        echo SQL Server started successfully!
    )
) else (
    echo SQL Server is running!
)
echo.

REM Navigate to API directory
cd /d "%~dp0PlayBojio.API"

REM Check if directory exists
if not exist "PlayBojio.API.csproj" (
    echo ERROR: PlayBojio.API project not found!
    echo Make sure this file is in the backend folder
    pause
    exit /b 1
)

REM Restore packages (only if needed)
if not exist "bin" (
    echo Restoring packages...
    dotnet restore
    echo.
)

REM Build project
echo Building project...
dotnet build --configuration Release --no-restore
if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    echo Check the error messages above
    pause
    exit /b 1
)
echo Build successful!
echo.

REM Apply migrations
echo Checking database migrations...
dotnet ef database update --no-build
echo.

REM Start the backend
echo ========================================
echo Starting PlayBojio Backend...
echo ========================================
echo.
echo Backend will be available at:
echo   - HTTP:  http://localhost:5000
echo   - HTTPS: https://localhost:5001
echo   - Swagger: https://localhost:5001/swagger
echo.
echo Press Ctrl+C to stop the backend
echo ========================================
echo.

dotnet run --no-build

pause
