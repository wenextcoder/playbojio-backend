# Deploy PlayBojio Backend to Render.com

## Prerequisites
- GitHub repository (✅ Already done: wenextcoder/playbojio-backend)
- Render.com account (free tier available)

---

## Option 1: PostgreSQL on Render (Recommended - Free Tier)

### Step 1: Update Backend for PostgreSQL Support

1. **Update the .csproj file** to include PostgreSQL provider:

```bash
cd /Volumes/hosting/PlayBojio/backend/PlayBojio.API
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

2. **Update Program.cs** to support both SQL Server and PostgreSQL:

Find this line:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Replace with:
```csharp
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider == "PostgreSQL")
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});
```

3. **Add using statement** at the top of Program.cs:
```csharp
using Npgsql.EntityFrameworkCore.PostgreSQL;
```

4. **Commit and push changes**:
```bash
git add .
git commit -m "Add PostgreSQL support for Render deployment"
git push origin main
```

---

### Step 2: Create PostgreSQL Database on Render

1. Go to https://render.com/
2. Sign up or Log in
3. Click **"New +"** → **"PostgreSQL"**
4. Configure:
   - **Name**: `playbojio-db`
   - **Database**: `playbojio`
   - **User**: (auto-generated)
   - **Region**: Choose closest to your users
   - **PostgreSQL Version**: 15 or latest
   - **Plan**: Free (or paid if needed)
5. Click **"Create Database"**
6. Wait for it to provision (1-2 minutes)
7. **Copy the Internal Database URL** - you'll need this!

---

### Step 3: Create Web Service on Render

1. Click **"New +"** → **"Web Service"**
2. Connect your GitHub account
3. Select repository: **wenextcoder/playbojio-backend**
4. Configure:

**Basic Settings:**
- **Name**: `playbojio-api` (will be your subdomain)
- **Region**: Same as database
- **Branch**: `main`
- **Root Directory**: Leave blank or `PlayBojio.API`
- **Runtime**: Docker *OR* .NET

**For Docker:**
- **Build Command**: Leave blank (uses Dockerfile)
- **Start Command**: Leave blank (uses Dockerfile CMD)

**For Native .NET:**
- **Build Command**: 
  ```bash
  cd PlayBojio.API && dotnet publish -c Release -o out
  ```
- **Start Command**:
  ```bash
  cd PlayBojio.API/out && ./PlayBojio.API
  ```

**Environment:**
- **Instance Type**: Free (or paid)

5. Click **"Advanced"** to add environment variables

---

### Step 4: Add Environment Variables

Add these environment variables:

```bash
# Database
DatabaseProvider=PostgreSQL
ConnectionStrings__DefaultConnection=<PASTE_INTERNAL_DATABASE_URL_HERE>

# JWT
Jwt__Key=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
Jwt__Issuer=PlayBojio
Jwt__Audience=PlayBojioUsers

# Cloudflare R2
R2__AccessKeyId=3ed02f17d6b7eeb4e90c266f3b3db183
R2__SecretAccessKey=d3fc1afb2469fadc6e90e97ab80258e26f9ea2457398169f4602a44f86532750
R2__AccountId=4eead0c25116171055250a85d63ff571
R2__BucketName=playbojio
R2__PublicUrl=https://pub-6d1016920e484aa78f1134593cbe2362.r2.dev

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:5000
```

**Note:** Get the `ConnectionStrings__DefaultConnection` from your PostgreSQL database page on Render (Internal Database URL)

---

### Step 5: Create Dockerfile (if using Docker build)

Create `Dockerfile` in the `backend` folder:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PlayBojio.API/PlayBojio.API.csproj", "PlayBojio.API/"]
RUN dotnet restore "PlayBojio.API/PlayBojio.API.csproj"
COPY . .
WORKDIR "/src/PlayBojio.API"
RUN dotnet build "PlayBojio.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlayBojio.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "PlayBojio.API.dll"]
```

Commit and push:
```bash
git add Dockerfile
git commit -m "Add Dockerfile for Render deployment"
git push origin main
```

---

### Step 6: Deploy!

1. Click **"Create Web Service"**
2. Render will automatically:
   - Clone your repo
   - Build the application
   - Start the server
3. Watch the logs for any errors
4. Once deployed, you'll get a URL like: `https://playbojio-api.onrender.com`

---

### Step 7: Run Database Migrations

**Option A: Via Local Connection**

1. Update your local `appsettings.json` temporarily with Render PostgreSQL connection:
```bash
cd PlayBojio.API
dotnet ef database update
```

**Option B: Via Render Shell (Paid plans only)**

1. Go to your web service
2. Click **"Shell"** tab
3. Run:
```bash
cd PlayBojio.API
dotnet ef database update
```

**Option C: Automatic Migration on Startup**

Add this to `Program.cs` before `app.Run()`:

```csharp
// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
```

Commit and redeploy.

---

## Option 2: External SQL Server (If you prefer SQL Server)

1. Use SQL Server from:
   - Azure SQL Database
   - Windows Server you already have
   - Any cloud SQL Server

2. Set environment variable:
```bash
DatabaseProvider=SqlServer
ConnectionStrings__DefaultConnection=Server=your-server;Database=PlayBojio;User Id=...;Password=...;
```

3. Make sure SQL Server accepts external connections
4. Update firewall rules to allow Render IPs

---

## Testing Your Deployment

1. Visit your API URL: `https://your-app.onrender.com/swagger`
2. You should see Swagger documentation
3. Test the `/api/auth/register` endpoint
4. Check logs in Render dashboard if issues occur

---

## Update Frontend to Use Render API

In your frontend `.env` or `.env.production`:

```env
VITE_API_URL=https://playbojio-api.onrender.com
```

Rebuild frontend:
```bash
npm run build
```

---

## Common Issues & Solutions

### Issue 1: Build Fails
**Solution:** Check build logs, ensure all NuGet packages restore correctly

### Issue 2: Database Connection Fails
**Solution:** 
- Verify connection string format
- Use Internal Database URL (not external)
- Check database is in same region

### Issue 3: Migrations Not Applied
**Solution:** Run migrations manually or add auto-migration code

### Issue 4: App Crashes on Startup
**Solution:**
- Check environment variables are set correctly
- View logs in Render dashboard
- Ensure `ASPNETCORE_URLS` is set to `http://0.0.0.0:5000`

### Issue 5: CORS Errors
**Solution:** Update `Program.cs` CORS policy to include your frontend domain

---

## Render Free Tier Limitations

- ⚠️ **Spins down after 15 minutes** of inactivity
- ⚠️ **Cold starts** take 30-60 seconds
- ✅ **750 hours/month** of runtime
- ✅ **Free PostgreSQL** with 1GB storage
- ✅ **Free SSL/HTTPS**

**Tip:** Upgrade to paid plan ($7/month) to keep service always on

---

## Monitoring & Logs

1. Go to your service in Render dashboard
2. Click **"Logs"** tab to see real-time logs
3. Click **"Metrics"** to see CPU/Memory usage
4. Set up **Alerts** for downtime notifications

---

## Auto-Deploy on Git Push

✅ Render automatically deploys when you push to `main` branch!

To disable:
1. Go to service settings
2. Turn off "Auto-Deploy"

---

## Custom Domain (Optional)

1. Go to service **"Settings"**
2. Click **"Custom Domain"**
3. Add your domain: `api.playbojio.com`
4. Update DNS records as instructed
5. Render provides free SSL certificate

---

## Cost Estimate

**Free Tier:**
- Web Service: Free (with limitations)
- PostgreSQL: Free (1GB)
- **Total: $0/month**

**Paid (Always On):**
- Web Service: $7/month (Starter)
- PostgreSQL: Free or $7/month (Starter)
- **Total: $7-14/month**

---

## Next Steps After Deployment

1. ✅ Test all API endpoints
2. ✅ Update frontend API URL
3. ✅ Set up monitoring/alerts
4. ✅ Configure custom domain
5. ✅ Set up automated backups
6. ✅ Add API documentation
7. ✅ Monitor performance

---

## Support

- Render Docs: https://render.com/docs
- Render Community: https://community.render.com/
- Status: https://status.render.com/

