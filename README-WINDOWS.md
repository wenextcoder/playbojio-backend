# PlayBojio Backend - Windows Setup

Quick guide for running PlayBojio backend on Windows with MSSQL.

---

## 🚀 Quick Start (3 Options)

### Option 1: Automated Setup (Recommended)
```powershell
# Right-click setup-windows.ps1 → "Run with PowerShell"
# Or run in PowerShell as Administrator:
.\setup-windows.ps1
```

This script will:
- ✅ Check all prerequisites
- ✅ Configure SQL Server connection
- ✅ Create database
- ✅ Update configuration files
- ✅ Install EF Core tools
- ✅ Run migrations
- ✅ Start the backend

### Option 2: Manual Quick Start
```cmd
# 1. Double-click this file:
start-backend-windows.bat

# Or run in Command Prompt:
cd backend
start-backend-windows.bat
```

### Option 3: Manual Setup
See the complete guide: [WINDOWS_MSSQL_SETUP.md](../WINDOWS_MSSQL_SETUP.md)

---

## 📋 Prerequisites

Before running the scripts, install:

1. **[.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** ⭐ Required
2. **[SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)** ⭐ Required
3. **[SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)** (Optional)
4. **[Visual Studio 2022](https://visualstudio.microsoft.com/)** or **[VS Code](https://code.visualstudio.com/)** (Optional)

---

## 🔧 Configuration

### Default Settings

**SQL Server:**
- Instance: `localhost\SQLEXPRESS`
- Database: `PlayBojio`
- Auth: Windows Authentication

**Backend:**
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

### Custom Configuration

Edit `PlayBojio.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PlayBojio;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Connection String Examples:**

Windows Authentication:
```
Server=localhost\SQLEXPRESS;Database=PlayBojio;Trusted_Connection=True;TrustServerCertificate=True;
```

SQL Authentication:
```
Server=localhost\SQLEXPRESS;Database=PlayBojio;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

---

## 🧪 Testing

### Check Backend is Running

**Browser:**
```
https://localhost:5001/swagger
```

**PowerShell:**
```powershell
Invoke-WebRequest -Uri https://localhost:5001/api/health -SkipCertificateCheck
```

**Command Prompt:**
```cmd
curl https://localhost:5001/api/health
```

### Create Test User

1. Open Swagger UI: `https://localhost:5001/swagger`
2. Find `POST /api/auth/register`
3. Click "Try it out"
4. Use this JSON:
```json
{
  "email": "test@example.com",
  "password": "Test123!@#",
  "displayName": "Test User"
}
```
5. Click "Execute"

---

## 🗄️ Database Management

### View Database

**Using SSMS:**
1. Open SQL Server Management Studio
2. Connect to: `localhost\SQLEXPRESS`
3. Expand: Databases → PlayBojio → Tables

**Using Command Line:**
```cmd
sqlcmd -S localhost\SQLEXPRESS -d PlayBojio -E
```

### Reset Database

```cmd
cd PlayBojio.API
dotnet ef database drop --force
dotnet ef database update
```

### Backup Database

```cmd
sqlcmd -S localhost\SQLEXPRESS -E -Q "BACKUP DATABASE PlayBojio TO DISK='C:\Backups\PlayBojio.bak'"
```

---

## ❌ Common Issues

### SQL Server Not Running

```cmd
# Check service status
sc query "MSSQL$SQLEXPRESS"

# Start service
net start "MSSQL$SQLEXPRESS"
```

### Port Already in Use

```cmd
# Find what's using port 5000
netstat -ano | findstr :5000

# Kill the process (replace <PID>)
taskkill /PID <PID> /F
```

### Cannot Connect to Database

1. Enable TCP/IP in SQL Server Configuration Manager
2. Restart SQL Server service
3. Check Windows Firewall
4. Verify connection string in `appsettings.json`

### Build Errors

```cmd
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

---

## 🔐 Security Notes

### Default Admin Credentials

**⚠️ Change these in production!**

```
Email: admin@playbojio.com
Password: Admin123!@#
```

### JWT Secret Key

Generate a secure key (32+ characters) in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long",
    "Issuer": "PlayBojio",
    "Audience": "PlayBojio"
  }
}
```

---

## 📁 Project Structure

```
backend/
├── PlayBojio.API/                 # Main API project
│   ├── Controllers/               # API endpoints
│   ├── Models/                    # Database models
│   ├── Services/                  # Business logic
│   ├── Data/                      # DbContext
│   ├── Migrations/                # Database migrations
│   ├── appsettings.json          # Configuration
│   └── Program.cs                 # Application entry
├── setup-windows.ps1              # Automated setup script
├── start-backend-windows.bat      # Quick start batch file
└── README-WINDOWS.md              # This file
```

---

## 🎯 Development Workflow

### Daily Development

1. **Start SQL Server** (if not auto-starting):
   ```cmd
   net start "MSSQL$SQLEXPRESS"
   ```

2. **Start Backend**:
   ```cmd
   cd backend
   start-backend-windows.bat
   ```

3. **Make Changes** to code

4. **Hot Reload** is enabled by default
   - Changes to `.cs` files restart automatically
   - Changes to `appsettings.json` restart automatically

### Adding Migrations

```cmd
cd PlayBojio.API

# Create migration
dotnet ef migrations add YourMigrationName

# Apply migration
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

---

## 🚀 Deployment

### Build for Production

```cmd
cd PlayBojio.API
dotnet publish -c Release -o ./publish
```

### Run Published Version

```cmd
cd publish
PlayBojio.API.exe
```

---

## 📊 Useful Commands

### Development

```powershell
# Watch mode (auto-rebuild on changes)
dotnet watch run

# Run with specific environment
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run

# Run tests
dotnet test

# Check for updates
dotnet list package --outdated
```

### Database

```cmd
# List all migrations
dotnet ef migrations list

# Generate SQL script for migration
dotnet ef migrations script

# Drop and recreate database
dotnet ef database drop
dotnet ef database update
```

### SQL Server

```cmd
# Connect to database
sqlcmd -S localhost\SQLEXPRESS -d PlayBojio -E

# List all databases
sqlcmd -S localhost\SQLEXPRESS -E -Q "SELECT name FROM sys.databases;"

# List all tables
sqlcmd -S localhost\SQLEXPRESS -d PlayBojio -E -Q "SELECT name FROM sys.tables;"
```

---

## 🆘 Need Help?

1. **Check the full guide**: [WINDOWS_MSSQL_SETUP.md](../WINDOWS_MSSQL_SETUP.md)
2. **Check backend logs** in the console output
3. **Check SQL Server logs** in Event Viewer
4. **Verify services are running**:
   ```cmd
   sc query "MSSQL$SQLEXPRESS"
   ```

---

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [SQL Server Documentation](https://learn.microsoft.com/en-us/sql/sql-server/)
- [.NET CLI Reference](https://learn.microsoft.com/en-us/dotnet/core/tools/)

---

**Happy Coding! 🎮**
