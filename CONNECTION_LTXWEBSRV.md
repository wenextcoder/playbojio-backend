# PlayBojio Backend - LTXWEBSRV Connection Setup

Configuration guide for connecting to your LTXWEBSRV SQL Server instance.

---

## 🔐 Your Database Credentials

**Server:** LTXWEBSRV  
**User:** playbojiouser  
**Password:** PbJ12#77Bg  
**Database:** PlayBojio (to be created/verified)

---

## 🔧 Step 1: Update appsettings.json

Navigate to: `backend/PlayBojio.API/appsettings.json`

Replace the `ConnectionStrings` section with:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LTXWEBSRV;Database=PlayBojio;User Id=playbojiouser;Password=PbJ12#77Bg;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False;"
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

### Alternative Connection String Formats

**If server is on same machine:**
```
Server=LTXWEBSRV;Database=PlayBojio;User Id=playbojiouser;Password=PbJ12#77Bg;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False;
```

**If server is on different machine (with IP address):**
```
Server=192.168.1.XX;Database=PlayBojio;User Id=playbojiouser;Password=PbJ12#77Bg;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False;
```

**If using specific port:**
```
Server=LTXWEBSRV,1433;Database=PlayBojio;User Id=playbojiouser;Password=PbJ12#77Bg;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=False;
```

**If SSL/TLS is required:**
```
Server=LTXWEBSRV;Database=PlayBojio;User Id=playbojiouser;Password=PbJ12#77Bg;TrustServerCertificate=False;Encrypt=True;MultipleActiveResultSets=true;
```

---

## 🗄️ Step 2: Create PlayBojio Database

### Option A: Using SQL Server Management Studio (SSMS)

1. **Open SSMS**
2. **Connect to Server:**
   - Server name: `LTXWEBSRV`
   - Authentication: `SQL Server Authentication`
   - Login: `playbojiouser`
   - Password: `PbJ12#77Bg`
   - Click "Connect"

3. **Create Database:**
   - Right-click "Databases" → "New Database"
   - Database name: `PlayBojio`
   - Click "OK"

   **Or run this SQL query:**
   ```sql
   CREATE DATABASE PlayBojio;
   GO
   
   USE PlayBojio;
   GO
   ```

### Option B: Using Command Line (sqlcmd)

```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg

1> CREATE DATABASE PlayBojio;
2> GO
1> QUIT
```

### Option C: Let EF Core Create It Automatically

If the user has CREATE DATABASE permissions, Entity Framework will create it when you run migrations.

---

## ✅ Step 3: Verify Connection

### Test Connection with sqlcmd

```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -d PlayBojio

1> SELECT @@VERSION;
2> GO
1> QUIT
```

**Expected:** Should show SQL Server version information

### Test Connection with PowerShell

```powershell
$connectionString = "Server=LTXWEBSRV;Database=PlayBojio;User Id=playbojiouser;Password=PbJ12#77Bg;TrustServerCertificate=True;Encrypt=False;"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "✅ Connection successful!" -ForegroundColor Green
    $connection.Close()
} catch {
    Write-Host "❌ Connection failed: $_" -ForegroundColor Red
}
```

---

## 🗃️ Step 4: Run Database Migrations

### Navigate to Backend API Project

```cmd
cd C:\path\to\PlayBojio\backend\PlayBojio.API
```

### Check Current Connection String

```cmd
type appsettings.json | findstr "DefaultConnection"
```

Should show your LTXWEBSRV connection string.

### Run Migrations

```cmd
REM Install/Update EF Core Tools (first time only)
dotnet tool install --global dotnet-ef
REM Or update:
dotnet tool update --global dotnet-ef

REM Create initial migration
dotnet ef migrations add InitialCreate

REM Apply to database
dotnet ef database update
```

### Apply Dummy Attendees Migration

```cmd
REM Option 1: Using sqlcmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -d PlayBojio -i "Migrations\add_dummy_attendees.sql"

REM Option 2: Using SSMS
REM 1. Open Migrations\add_dummy_attendees.sql in SSMS
REM 2. Make sure you're connected to PlayBojio database
REM 3. Execute (F5)
```

### Verify Tables Created

```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -d PlayBojio -Q "SELECT name FROM sys.tables ORDER BY name;"
```

**Expected tables:**
- AspNetRoles
- AspNetUsers
- AspNetUserRoles
- Events
- EventAttendees
- Sessions
- SessionAttendees
- Groups
- GroupMembers
- Friends
- FriendRequests
- GroupInvitations
- GroupJoinRequests
- etc.

---

## 🚀 Step 5: Start the Backend

### Using Command Prompt

```cmd
cd C:\path\to\PlayBojio\backend\PlayBojio.API

REM Build and run
dotnet build
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

1. Open `backend/PlayBojio.sln`
2. Set `PlayBojio.API` as startup project
3. Press `F5` to run
4. Browser opens to `https://localhost:5001/swagger`

---

## 🧪 Step 6: Test the Setup

### Open Swagger UI

```
https://localhost:5001/swagger
```

### Create Test User

**In Swagger UI:**
1. Find `POST /api/auth/register`
2. Click "Try it out"
3. Use this JSON:

```json
{
  "email": "test@playbojio.com",
  "password": "Test123!@#",
  "displayName": "Test User"
}
```

4. Click "Execute"
5. Should get 200 response with JWT token

### Login Test

1. Find `POST /api/auth/login`
2. Click "Try it out"
3. Use:

```json
{
  "email": "test@playbojio.com",
  "password": "Test123!@#"
}
```

4. Should receive JWT token

---

## 🔐 Step 7: Create Admin User

### Option A: Via Swagger UI

1. Register a new user (see above)
2. Get the user ID from the response
3. Connect to database and assign Admin role (see below)

### Option B: Via SQL

```sql
-- Connect to database
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -d PlayBojio

-- Create Admin role if doesn't exist
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Admin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
END
GO

-- Find your user
SELECT Id, Email, DisplayName FROM AspNetUsers WHERE Email = 'your-email@example.com';
GO

-- Assign Admin role (replace YOUR_USER_ID with actual ID)
DECLARE @UserId NVARCHAR(450) = 'YOUR_USER_ID_HERE';
DECLARE @RoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin');

IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);
END
GO

-- Verify
SELECT u.Email, r.Name as Role
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Email = 'your-email@example.com';
GO
```

---

## ❌ Common Issues & Solutions

### Issue 1: "Cannot connect to LTXWEBSRV"

**Check if SQL Server is running:**
```cmd
sc query "MSSQLSERVER"
REM Or if named instance:
sc query "MSSQL$INSTANCENAME"
```

**Start if not running:**
```cmd
net start MSSQLSERVER
```

**Check network connectivity:**
```cmd
ping LTXWEBSRV
telnet LTXWEBSRV 1433
```

### Issue 2: "Login failed for user 'playbojiouser'"

**Verify credentials:**
```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg
```

**Check if user has permissions:**
```sql
-- Connect as admin and check
USE PlayBojio;
GO

SELECT dp.name, dp.type_desc
FROM sys.database_principals dp
WHERE dp.name = 'playbojiouser';
GO

-- Grant permissions if needed
ALTER ROLE db_owner ADD MEMBER playbojiouser;
GO
```

### Issue 3: "Cannot open database 'PlayBojio'"

**Check if database exists:**
```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -Q "SELECT name FROM sys.databases;"
```

**Create if missing:**
```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -Q "CREATE DATABASE PlayBojio;"
```

### Issue 4: "A network-related or instance-specific error"

**Enable TCP/IP:**
1. Open SQL Server Configuration Manager
2. Go to: SQL Server Network Configuration → Protocols for [Instance]
3. Enable TCP/IP
4. Restart SQL Server service

**Check SQL Server Browser service:**
```cmd
net start "SQL Server Browser"
```

**Add firewall rule:**
```cmd
netsh advfirewall firewall add rule name="SQL Server" dir=in action=allow protocol=TCP localport=1433
```

### Issue 5: Connection works but migrations fail

**Check user permissions:**
```sql
-- Connect as admin
USE PlayBojio;
GO

-- Grant full permissions
ALTER ROLE db_owner ADD MEMBER playbojiouser;
GO

-- Or specific permissions
GRANT CREATE TABLE TO playbojiouser;
GRANT ALTER TO playbojiouser;
GRANT REFERENCES TO playbojiouser;
GO
```

---

## 🔐 Security Recommendations

### For Development

Your current setup is fine for development:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=LTXWEBSRV;Database=PlayBojio;User Id=playbojiouser;Password=PbJ12#77Bg;TrustServerCertificate=True;Encrypt=False;"
}
```

### For Production

**Use environment variables:**

```cmd
REM Set environment variable
setx ConnectionStrings__DefaultConnection "Server=LTXWEBSRV;Database=PlayBojio;User Id=playbojiouser;Password=PbJ12#77Bg;TrustServerCertificate=True;"
```

**Or use User Secrets (for development):**

```cmd
cd PlayBojio.API

REM Initialize user secrets
dotnet user-secrets init

REM Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=LTXWEBSRV;Database=PlayBojio;User Id=playbojiouser;Password=PbJ12#77Bg;TrustServerCertificate=True;"
```

Then in `appsettings.json`, you can remove the connection string.

---

## 📊 Database Management

### Backup Database

```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -Q "BACKUP DATABASE PlayBojio TO DISK='C:\Backups\PlayBojio.bak'"
```

### Restore Database

```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -Q "RESTORE DATABASE PlayBojio FROM DISK='C:\Backups\PlayBojio.bak' WITH REPLACE"
```

### View All Tables

```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -d PlayBojio -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME;"
```

### Check Database Size

```cmd
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -d PlayBojio -Q "EXEC sp_spaceused;"
```

---

## 🎯 Quick Start Commands

### Full Setup Commands

```cmd
REM 1. Navigate to project
cd C:\path\to\PlayBojio\backend\PlayBojio.API

REM 2. Update appsettings.json with LTXWEBSRV connection string
notepad appsettings.json

REM 3. Install EF Core tools
dotnet tool install --global dotnet-ef

REM 4. Create database (if doesn't exist)
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PlayBojio') CREATE DATABASE PlayBojio;"

REM 5. Run migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

REM 6. Apply dummy attendees migration
sqlcmd -S LTXWEBSRV -U playbojiouser -P PbJ12#77Bg -d PlayBojio -i "Migrations\add_dummy_attendees.sql"

REM 7. Build and run
dotnet build
dotnet run
```

### Daily Development

```cmd
cd C:\path\to\PlayBojio\backend\PlayBojio.API
dotnet run
```

Or double-click: `start-backend-windows.bat`

---

## 📱 Connect Frontend

Update `frontend/.env.local`:

```env
VITE_API_BASE_URL=http://localhost:5000/api
```

Or for HTTPS:

```env
VITE_API_BASE_URL=https://localhost:5001/api
```

---

## ✅ Verification Checklist

- [ ] SQL Server LTXWEBSRV is accessible
- [ ] User playbojiouser can login
- [ ] PlayBojio database exists
- [ ] Connection string in appsettings.json is correct
- [ ] EF Core tools installed
- [ ] Migrations applied successfully
- [ ] All tables created
- [ ] Backend starts without errors
- [ ] Swagger UI accessible at https://localhost:5001/swagger
- [ ] Can register and login users
- [ ] Admin user created and role assigned

---

## 🆘 Need Help?

**Connection String Issues:**
- Double-check server name: `LTXWEBSRV`
- Verify username: `playbojiouser`
- Verify password: `PbJ12#77Bg`
- Check if server is accessible: `ping LTXWEBSRV`
- Test with sqlcmd first before running backend

**Database Issues:**
- Verify PlayBojio database exists
- Check user has db_owner role
- Verify tables are created
- Check for migration errors

**Backend Issues:**
- Check appsettings.json syntax
- Verify all NuGet packages restored
- Check for build errors
- Review console output for errors

---

## 📞 Admin Contact

If you need additional database permissions or access, contact your database administrator who created the playbojiouser account.

---

**Happy Coding! 🚀**
