# PlayBojio Backend Setup Script for Windows with MSSQL
# Run this script as Administrator

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PlayBojio Backend Setup for Windows" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "⚠️  This script requires Administrator privileges!" -ForegroundColor Yellow
    Write-Host "Please right-click and select 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit
}

# Step 1: Check .NET SDK
Write-Host "Step 1: Checking .NET SDK..." -ForegroundColor Green
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found!" -ForegroundColor Red
    Write-Host "Please download and install .NET 8.0 SDK from:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    pause
    exit
}

# Step 2: Check SQL Server
Write-Host ""
Write-Host "Step 2: Checking SQL Server..." -ForegroundColor Green
$sqlServices = Get-Service -Name "MSSQL*" -ErrorAction SilentlyContinue
if ($sqlServices) {
    Write-Host "✅ SQL Server found" -ForegroundColor Green
    
    # Start SQL Server if not running
    $sqlExpress = Get-Service -Name "MSSQL`$SQLEXPRESS" -ErrorAction SilentlyContinue
    if ($sqlExpress) {
        if ($sqlExpress.Status -ne 'Running') {
            Write-Host "Starting SQL Server Express..." -ForegroundColor Yellow
            Start-Service "MSSQL`$SQLEXPRESS"
            Write-Host "✅ SQL Server Express started" -ForegroundColor Green
        } else {
            Write-Host "✅ SQL Server Express is running" -ForegroundColor Green
        }
    }
} else {
    Write-Host "❌ SQL Server not found!" -ForegroundColor Red
    Write-Host "Please download and install SQL Server Express from:" -ForegroundColor Yellow
    Write-Host "https://www.microsoft.com/en-us/sql-server/sql-server-downloads" -ForegroundColor Yellow
    pause
    exit
}

# Step 3: Get database configuration
Write-Host ""
Write-Host "Step 3: Database Configuration" -ForegroundColor Green
Write-Host ""

$serverName = Read-Host "Enter SQL Server instance name (default: localhost\SQLEXPRESS)"
if ([string]::IsNullOrWhiteSpace($serverName)) {
    $serverName = "localhost\SQLEXPRESS"
}

$authType = Read-Host "Use Windows Authentication? (Y/N, default: Y)"
if ([string]::IsNullOrWhiteSpace($authType) -or $authType -eq "Y" -or $authType -eq "y") {
    $useWindowsAuth = $true
    $connectionString = "Server=$serverName;Database=PlayBojio;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
} else {
    $useWindowsAuth = $false
    $sqlUser = Read-Host "Enter SQL Server username (default: playbojio_user)"
    if ([string]::IsNullOrWhiteSpace($sqlUser)) {
        $sqlUser = "playbojio_user"
    }
    $sqlPassword = Read-Host "Enter SQL Server password" -AsSecureString
    $sqlPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlPassword))
    $connectionString = "Server=$serverName;Database=PlayBojio;User Id=$sqlUser;Password=$sqlPasswordPlain;TrustServerCertificate=True;MultipleActiveResultSets=true"
}

Write-Host "✅ Connection string configured" -ForegroundColor Green

# Step 4: Create database
Write-Host ""
Write-Host "Step 4: Creating PlayBojio database..." -ForegroundColor Green
try {
    if ($useWindowsAuth) {
        sqlcmd -S $serverName -E -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PlayBojio') CREATE DATABASE PlayBojio;" -b
    } else {
        sqlcmd -S $serverName -U $sqlUser -P $sqlPasswordPlain -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PlayBojio') CREATE DATABASE PlayBojio;" -b
    }
    Write-Host "✅ Database created/verified" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create database: $_" -ForegroundColor Red
    Write-Host "Please create the database manually using SSMS" -ForegroundColor Yellow
}

# Step 5: Update appsettings.json
Write-Host ""
Write-Host "Step 5: Updating appsettings.json..." -ForegroundColor Green
$appSettingsPath = "$PSScriptRoot\PlayBojio.API\appsettings.json"

if (Test-Path $appSettingsPath) {
    $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
    $appSettings.ConnectionStrings.DefaultConnection = $connectionString
    
    # Generate JWT key if not set
    if ($appSettings.Jwt.Key -eq "your-super-secret-key-change-this-in-production") {
        $jwtKey = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
        $appSettings.Jwt.Key = $jwtKey
        Write-Host "✅ Generated new JWT key" -ForegroundColor Green
    }
    
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath
    Write-Host "✅ appsettings.json updated" -ForegroundColor Green
} else {
    Write-Host "⚠️  appsettings.json not found at $appSettingsPath" -ForegroundColor Yellow
}

# Step 6: Install EF Core tools
Write-Host ""
Write-Host "Step 6: Installing Entity Framework Core tools..." -ForegroundColor Green
try {
    dotnet tool install --global dotnet-ef --ignore-failed-sources 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ EF Core tools installed" -ForegroundColor Green
    } else {
        dotnet tool update --global dotnet-ef --ignore-failed-sources
        Write-Host "✅ EF Core tools updated" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  Could not install EF Core tools: $_" -ForegroundColor Yellow
}

# Step 7: Navigate to API project
Write-Host ""
Write-Host "Step 7: Setting up backend project..." -ForegroundColor Green
$apiPath = "$PSScriptRoot\PlayBojio.API"
Set-Location $apiPath

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Cyan
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Packages restored" -ForegroundColor Green
} else {
    Write-Host "⚠️  Package restore had warnings" -ForegroundColor Yellow
}

# Build project
Write-Host "Building project..." -ForegroundColor Cyan
dotnet build --configuration Release
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Project built successfully" -ForegroundColor Green
} else {
    Write-Host "❌ Build failed" -ForegroundColor Red
    pause
    exit
}

# Step 8: Run migrations
Write-Host ""
Write-Host "Step 8: Running database migrations..." -ForegroundColor Green
try {
    # Check if migrations exist
    $migrationsFolder = "$apiPath\Migrations"
    if (Test-Path $migrationsFolder) {
        Write-Host "Applying existing migrations..." -ForegroundColor Cyan
        dotnet ef database update
        Write-Host "✅ Migrations applied" -ForegroundColor Green
    } else {
        Write-Host "Creating initial migration..." -ForegroundColor Cyan
        dotnet ef migrations add InitialCreate
        dotnet ef database update
        Write-Host "✅ Initial migration created and applied" -ForegroundColor Green
    }
    
    # Apply dummy attendees migration
    $dummyMigrationPath = "$apiPath\Migrations\add_dummy_attendees.sql"
    if (Test-Path $dummyMigrationPath) {
        Write-Host "Applying dummy attendees migration..." -ForegroundColor Cyan
        if ($useWindowsAuth) {
            sqlcmd -S $serverName -d PlayBojio -E -i $dummyMigrationPath -b
        } else {
            sqlcmd -S $serverName -d PlayBojio -U $sqlUser -P $sqlPasswordPlain -i $dummyMigrationPath -b
        }
        Write-Host "✅ Dummy attendees migration applied" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  Migration warning: $_" -ForegroundColor Yellow
    Write-Host "You may need to run migrations manually" -ForegroundColor Yellow
}

# Step 9: Create admin user
Write-Host ""
Write-Host "Step 9: Admin User Setup" -ForegroundColor Green
$createAdmin = Read-Host "Create admin user now? (Y/N, default: Y)"
if ([string]::IsNullOrWhiteSpace($createAdmin) -or $createAdmin -eq "Y" -or $createAdmin -eq "y") {
    Write-Host "The admin user will be created after the backend starts." -ForegroundColor Cyan
    Write-Host "Default credentials will be:" -ForegroundColor Cyan
    Write-Host "  Email: admin@playbojio.com" -ForegroundColor Yellow
    Write-Host "  Password: Admin123!@#" -ForegroundColor Yellow
}

# Step 10: Configure Windows Firewall
Write-Host ""
Write-Host "Step 10: Configuring Windows Firewall..." -ForegroundColor Green
try {
    # Allow SQL Server
    $sqlRule = Get-NetFirewallRule -DisplayName "SQL Server" -ErrorAction SilentlyContinue
    if (-not $sqlRule) {
        New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow | Out-Null
        Write-Host "✅ Firewall rule added for SQL Server" -ForegroundColor Green
    }
    
    # Allow ASP.NET Core
    $apiRule = Get-NetFirewallRule -DisplayName "ASP.NET Core Backend" -ErrorAction SilentlyContinue
    if (-not $apiRule) {
        New-NetFirewallRule -DisplayName "ASP.NET Core Backend" -Direction Inbound -Protocol TCP -LocalPort 5000,5001 -Action Allow | Out-Null
        Write-Host "✅ Firewall rule added for ASP.NET Core" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  Could not configure firewall: $_" -ForegroundColor Yellow
    Write-Host "You may need to configure firewall rules manually" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✅ Setup Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration Summary:" -ForegroundColor Cyan
Write-Host "  • SQL Server: $serverName" -ForegroundColor White
Write-Host "  • Database: PlayBojio" -ForegroundColor White
Write-Host "  • Authentication: $(if ($useWindowsAuth) {'Windows'} else {'SQL Server'})" -ForegroundColor White
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Start the backend:" -ForegroundColor White
Write-Host "     cd PlayBojio.API" -ForegroundColor Yellow
Write-Host "     dotnet run" -ForegroundColor Yellow
Write-Host ""
Write-Host "  2. Open browser to:" -ForegroundColor White
Write-Host "     https://localhost:5001/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "  3. Create admin user via Swagger UI:" -ForegroundColor White
Write-Host "     POST /api/auth/register" -ForegroundColor Yellow
Write-Host "     Email: admin@playbojio.com" -ForegroundColor Yellow
Write-Host "     Password: Admin123!@#" -ForegroundColor Yellow
Write-Host ""

$startNow = Read-Host "Start the backend now? (Y/N)"
if ($startNow -eq "Y" -or $startNow -eq "y") {
    Write-Host ""
    Write-Host "Starting backend..." -ForegroundColor Green
    Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
    Write-Host ""
    dotnet run
} else {
    Write-Host ""
    Write-Host "To start the backend later, run:" -ForegroundColor Cyan
    Write-Host "  cd $apiPath" -ForegroundColor Yellow
    Write-Host "  dotnet run" -ForegroundColor Yellow
}

pause
