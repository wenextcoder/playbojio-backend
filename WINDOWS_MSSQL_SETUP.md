# PlayBojio Backend Setup on Windows with MSSQL

Complete guide to run PlayBojio backend on Windows using Microsoft SQL Server.

---

## 📋 Prerequisites

### Required Software

1. **Windows 10/11** (64-bit)
2. **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
3. **SQL Server 2019/2022** (Express or Developer Edition)
4. **SQL Server Management Studio (SSMS)** - Optional but recommended
5. **Visual Studio 2022** or **VS Code** - For development

---

## 🗄️ Step 1: Install SQL Server

### Option A: SQL Server Express (Free, Recommended for Development)

1. **Download SQL Server Express:**
   - Go to: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - Click "Download now" under "Express"

2. **Run the Installer:**
   ```
   - Choose "Basic" installation
   - Accept license terms
   - Choose installation location (default is fine)
   - Click "Install"
   ```

3. **Note the Instance Name:**
   - Default instance: `localhost\SQLEXPRESS`
   - Or named instance: `localhost\YOUR_INSTANCE_NAME`

4. **Enable TCP/IP (Important!):**
   - Open "SQL Server Configuration Manager"
   - Navigate to: SQL Server Network Configuration → Protocols for SQLEXPRESS
   - Right-click "TCP/IP" → Enable
   - Restart SQL Server service

### Option B: SQL Server Developer Edition (Free, Full Features)

1. **Download SQL Server Developer:**
   - Same website as Express
   - Click "Download now" under "Developer"

2. **Run the Installer:**
   ```
   - Choose "Custom" installation
   - Select installation path
   - Choose "New SQL Server standalone installation"
   - Select "Developer" edition
   - Accept license terms
   - Choose features (Database Engine Services required)
   - Instance Configuration: Default instance or named instance
   - Server Configuration: Use default accounts
   - Database Engine Configuration:
     * Authentication Mode: Mixed Mode (SQL Server and Windows)
     * Set SA password (e.g., YourStrongPassword123!)
     * Add current user as administrator
   - Complete installation
   ```

---

## 🔧 Step 2: Install SQL Server Management Studio (SSMS)

**Optional but highly recommended for database management**

1. **Download SSMS:**
   - https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
   - Or download from SQL Server installer

2. **Install SSMS:**
   - Run the installer
   - Follow installation wizard
   - No special configuration needed

3. **Test Connection:**
   - Open SSMS
   - Server name: `localhost\SQLEXPRESS` or `localhost`
   - Authentication: Windows Authentication or SQL Server Authentication
   - Click "Connect"

---

## 🛠️ Step 3: Create PlayBojio Database

### Using SSMS (GUI Method)

1. **Open SSMS and connect to your SQL Server instance**

2. **Create Database:**
   ```sql
   -- Right-click "Databases" → New Database
   -- Or run this query:
   
   CREATE DATABASE PlayBojio;
   GO
   ```

3. **Create Login (if using SQL Authentication):**
   ```sql
   -- Security → Logins → Right-click → New Login
   -- Or run this query:
   
   USE [master]
   GO
   
   CREATE LOGIN [playbojio_user] WITH PASSWORD = 'YourStrongPassword123!'
   GO
   
   USE [PlayBojio]
   GO
   
   CREATE USER [playbojio_user] FOR LOGIN [playbojio_user]
   GO
   
   ALTER ROLE [db_owner] ADD MEMBER [playbojio_user]
   GO
   ```

### Using Command Line (sqlcmd)

```cmd
sqlcmd -S localhost\SQLEXPRESS -E

1> CREATE DATABASE PlayBojio;
2> GO
1> QUIT
```

---

## 📝 Step 4: Configure Backend Connection String

### Update appsettings.json

Navigate to: `backend/PlayBojio.API/appsettings.json`

**Option A: Windows Authentication (Recommended for local development)**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PlayBojio;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long-for-jwt-tokens",
    "Issuer": "PlayBojio",
    "Audience": "PlayBojio"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cloudflare": {
    "AccountId": "your-account-id",
    "R2AccessKeyId": "your-access-key",
    "R2SecretAccessKey": "your-secret-key",
    "R2BucketName": "playbojio-uploads",
    "R2PublicUrl": "https://your-custom-domain.com"
  }
}
```

**Option B: SQL Authentication**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PlayBojio;User Id=playbojio_user;Password=YourStrongPassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long-for-jwt-tokens",
    "Issuer": "PlayBojio",
    "Audience": "PlayBojio"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cloudflare": {
    "AccountId": "your-account-id",
    "R2AccessKeyId": "your-access-key",
    "R2SecretAccessKey": "your-secret-key",
    "R2BucketName": "playbojio-uploads",
    "R2PublicUrl": "https://your-custom-domain.com"
  }
}
```

### Connection String Formats

**Default Instance (no SQLEXPRESS):**
```
Server=localhost;Database=PlayBojio;Trusted_Connection=True;TrustServerCertificate=True;
```

**Named Instance:**
```
Server=localhost\SQLEXPRESS;Database=PlayBojio;Trusted_Connection=True;TrustServerCertificate=True;
```

**Remote Server:**
```
Server=192.168.1.100;Database=PlayBojio;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

**Using IP Address:**
```
Server=127.0.0.1,1433;Database=PlayBojio;Trusted_Connection=True;TrustServerCertificate=True;
```

---

## 🗃️ Step 5: Run Database Migrations

### Check Current Setup

1. **Open Command Prompt or PowerShell as Administrator**

2. **Navigate to the backend API project:**
   ```cmd
   cd C:\path\to\PlayBojio\backend\PlayBojio.API
   ```

3. **Verify .NET SDK is installed:**
   ```cmd
   dotnet --version
   ```
   Should show: `8.0.x`

4. **Install/Update EF Core Tools:**
   ```cmd
   dotnet tool install --global dotnet-ef
   
   REM Or update if already installed:
   dotnet tool update --global dotnet-ef
   ```

### Create Initial Migration

```cmd
cd C:\path\to\PlayBojio\backend\PlayBojio.API

REM Create initial migration
dotnet ef migrations add InitialCreate

REM Apply migration to database
dotnet ef database update
```

### Apply Dummy Attendees Migration

```cmd
REM The SQL script is already in: backend/PlayBojio.API/Migrations/add_dummy_attendees.sql

REM Option 1: Run via SSMS
REM 1. Open SSMS
REM 2. Connect to your database
REM 3. Open the add_dummy_attendees.sql file
REM 4. Execute the script

REM Option 2: Run via sqlcmd
sqlcmd -S localhost\SQLEXPRESS -d PlayBojio -E -i "Migrations\add_dummy_attendees.sql"

REM Option 3: Run via dotnet ef
REM Create a new migration for dummy attendees
dotnet ef migrations add AddDummyAttendees
dotnet ef database update
```

### Verify Database Tables

**Using SSMS:**
1. Open SSMS
2. Connect to your server
3. Expand: Databases → PlayBojio → Tables
4. You should see tables like:
   - `AspNetUsers`
   - `Events`
   - `Sessions`
   - `Groups`
   - `EventAttendees`
   - `SessionAttendees`
   - etc.

**Using sqlcmd:**
```cmd
sqlcmd -S localhost\SQLEXPRESS -d PlayBojio -E -Q "SELECT name FROM sys.tables ORDER BY name;"
```

---

## 🚀 Step 6: Run the Backend

### Using Command Line

```cmd
cd C:\path\to\PlayBojio\backend\PlayBojio.API

REM Restore packages
dotnet restore

REM Build the project
dotnet build

REM Run the backend
dotnet run
```

**Expected Output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Using Visual Studio 2022

1. **Open Solution:**
   - Open `backend/PlayBojio.sln` in Visual Studio

2. **Set Startup Project:**
   - Right-click `PlayBojio.API` → "Set as Startup Project"

3. **Check Connection String:**
   - Open `appsettings.json` and verify connection string

4. **Run:**
   - Press `F5` (Debug) or `Ctrl+F5` (Run without debugging)
   - Browser will open to Swagger UI: `https://localhost:5001/swagger`

### Using VS Code

1. **Open Folder:**
   ```
   File → Open Folder → Select backend/PlayBojio.API
   ```

2. **Install C# Extension:**
   - Install "C# Dev Kit" extension by Microsoft

3. **Run:**
   - Press `F5`
   - Or use terminal: `dotnet run`

---

## 🧪 Step 7: Test the Backend

### Check API Health

**Browser:**
```
https://localhost:5001/swagger
```

**Command Line:**
```cmd
curl https://localhost:5001/api/health
```

**PowerShell:**
```powershell
Invoke-WebRequest -Uri https://localhost:5001/api/health -SkipCertificateCheck
```

### Create Admin User

**Option 1: Using Swagger UI**
1. Open `https://localhost:5001/swagger`
2. Find `POST /api/auth/register`
3. Click "Try it out"
4. Use this JSON:
```json
{
  "email": "admin@playbojio.com",
  "password": "Admin123!@#",
  "displayName": "Admin User"
}
```
5. Execute
6. Copy the token from response

**Option 2: Using PowerShell**
```powershell
$body = @{
    email = "admin@playbojio.com"
    password = "Admin123!@#"
    displayName = "Admin User"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/auth/register" `
                  -Method POST `
                  -Body $body `
                  -ContentType "application/json" `
                  -SkipCertificateCheck
```

### Assign Admin Role (SQL)

```sql
-- Using SSMS or sqlcmd
USE PlayBojio;
GO

-- Find the user ID
SELECT Id, Email, DisplayName FROM AspNetUsers WHERE Email = 'admin@playbojio.com';
GO

-- Find the Admin role ID
SELECT Id, Name FROM AspNetRoles WHERE Name = 'Admin';
GO

-- If Admin role doesn't exist, create it
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
END
GO

-- Assign Admin role to user (replace YOUR_USER_ID and ADMIN_ROLE_ID)
INSERT INTO AspNetUserRoles (UserId, RoleId)
VALUES ('YOUR_USER_ID', 'ADMIN_ROLE_ID');
GO
```

---

## 📁 Project Structure

```
backend/
├── PlayBojio.API/
│   ├── Controllers/          # API endpoints
│   ├── Models/              # Database models
│   ├── DTOs/                # Data transfer objects
│   ├── Services/            # Business logic
│   ├── Data/                # DbContext
│   ├── Migrations/          # Database migrations
│   ├── appsettings.json     # Configuration
│   └── Program.cs           # Application entry point
└── PlayBojio.sln            # Solution file
```

---

## 🔧 Common Issues & Solutions

### Issue 1: "A network-related or instance-specific error"

**Solution:**
```cmd
REM 1. Check SQL Server is running
services.msc
REM Find "SQL Server (SQLEXPRESS)" and ensure it's Running

REM 2. Enable TCP/IP
REM Open SQL Server Configuration Manager
REM SQL Server Network Configuration → Protocols for SQLEXPRESS
REM Right-click TCP/IP → Enable
REM Restart SQL Server service

REM 3. Check Windows Firewall
netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433
```

### Issue 2: "Login failed for user"

**Solution:**
```sql
-- Using Windows Authentication in SSMS
USE [master]
GO

-- Create login if it doesn't exist
CREATE LOGIN [playbojio_user] WITH PASSWORD = 'YourStrongPassword123!'
GO

USE [PlayBojio]
GO

-- Create user for the login
CREATE USER [playbojio_user] FOR LOGIN [playbojio_user]
GO

-- Grant permissions
ALTER ROLE [db_owner] ADD MEMBER [playbojio_user]
GO
```

### Issue 3: "Cannot open database requested by the login"

**Solution:**
```cmd
REM Verify database exists
sqlcmd -S localhost\SQLEXPRESS -E -Q "SELECT name FROM sys.databases;"

REM If PlayBojio doesn't exist, create it
sqlcmd -S localhost\SQLEXPRESS -E -Q "CREATE DATABASE PlayBojio;"
```

### Issue 4: Port 5000/5001 already in use

**Solution:**
```cmd
REM Find what's using the port
netstat -ano | findstr :5000

REM Kill the process (replace PID)
taskkill /PID <PID> /F

REM Or change the port in launchSettings.json:
REM backend/PlayBojio.API/Properties/launchSettings.json
```

### Issue 5: CORS errors when connecting frontend

**Solution:**
Update `Program.cs` to allow your frontend URL:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Before app.Run()
app.UseCors("AllowFrontend");
```

---

## 🔐 Environment Variables (Optional)

Create `backend/PlayBojio.API/.env` file:

```env
ConnectionStrings__DefaultConnection=Server=localhost\SQLEXPRESS;Database=PlayBojio;Trusted_Connection=True;TrustServerCertificate=True;
Jwt__Key=your-super-secret-key-min-32-characters-long-for-jwt-tokens
Jwt__Issuer=PlayBojio
Jwt__Audience=PlayBojio
Cloudflare__AccountId=your-account-id
Cloudflare__R2AccessKeyId=your-access-key
Cloudflare__R2SecretAccessKey=your-secret-key
Cloudflare__R2BucketName=playbojio-uploads
Cloudflare__R2PublicUrl=https://your-custom-domain.com
```

Then install and use `dotenv` package (optional).

---

## 📊 Database Management Tips

### Backup Database

```cmd
REM Using sqlcmd
sqlcmd -S localhost\SQLEXPRESS -E -Q "BACKUP DATABASE PlayBojio TO DISK='C:\Backups\PlayBojio.bak'"
```

**Using SSMS:**
1. Right-click database → Tasks → Back Up
2. Choose backup type and destination
3. Click OK

### Restore Database

```cmd
sqlcmd -S localhost\SQLEXPRESS -E -Q "RESTORE DATABASE PlayBojio FROM DISK='C:\Backups\PlayBojio.bak' WITH REPLACE"
```

### Reset Database

```cmd
cd C:\path\to\PlayBojio\backend\PlayBojio.API

REM Drop database
dotnet ef database drop --force

REM Recreate and migrate
dotnet ef database update
```

---

## 🎯 Quick Start Script

Create `backend/start-backend.bat`:

```batch
@echo off
echo Starting PlayBojio Backend with MSSQL...
echo.

REM Check if SQL Server is running
sc query "MSSQL$SQLEXPRESS" | find "RUNNING"
if errorlevel 1 (
    echo SQL Server is not running. Starting...
    net start "MSSQL$SQLEXPRESS"
)

REM Navigate to API project
cd /d "%~dp0PlayBojio.API"

REM Restore packages
echo Restoring packages...
dotnet restore

REM Build project
echo Building project...
dotnet build

REM Run migrations
echo Applying migrations...
dotnet ef database update

REM Start the backend
echo Starting backend...
dotnet run

pause
```

Double-click `start-backend.bat` to run!

---

## 📱 Connect Frontend to Local Backend

Update `frontend/.env.local`:

```env
VITE_API_BASE_URL=https://localhost:5001/api
```

Or in `frontend/src/lib/api.ts`:

```typescript
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001/api';
```

---

## 🎉 You're All Set!

Your PlayBojio backend should now be running on Windows with MSSQL!

**Access Points:**
- Backend API: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`
- Database: `localhost\SQLEXPRESS` (PlayBojio)

**Default Credentials:**
- Admin Email: `admin@playbojio.com`
- Admin Password: `Admin123!@#`

---

## 📚 Additional Resources

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [SQL Server Documentation](https://learn.microsoft.com/en-us/sql/sql-server/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)

---

## 💡 Need Help?

If you encounter any issues:

1. Check the "Common Issues & Solutions" section above
2. Review the backend logs in the console
3. Check SQL Server logs in Event Viewer
4. Verify all services are running
5. Double-check connection strings

**Happy Coding! 🚀**
