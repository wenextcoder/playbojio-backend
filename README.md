# PlayBojio Backend API

Board game community platform backend built with .NET 8 and ASP.NET Core.


## Tech Stack

- **.NET 8** - Latest .NET framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Primary database
- **JWT Authentication** - Secure token-based auth
- **Cloudflare R2** - S3-compatible object storage
- **Swagger/OpenAPI** - API documentation

## Project Structure

```
PlayBojio.API/
├── Controllers/        # API endpoints
├── Services/          # Business logic
├── Models/            # Database entities
├── DTOs/              # Data transfer objects
├── Data/              # DbContext and configurations
├── Utils/             # Helper utilities (SlugHelper, etc.)
├── Migrations/        # EF Core migrations
└── Program.cs         # Application entry point
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (Express or higher)
- [Cloudflare R2 Account](https://www.cloudflare.com/products/r2/) (for image storage)

## Getting Started


### 2. Configure Database

**Option A: Using Docker (Recommended for Development)**

```bash
# Start SQL Server and CloudBeaver
docker-compose up -d

# SQL Server will be available at: localhost:1433
# CloudBeaver (DB Manager) at: http://localhost:8080
```

**Option B: Using Local SQL Server**

1. Install SQL Server Express
2. Update connection string in `appsettings.json`

### 3. Configure Application Settings

Copy the example configuration:

```bash
cd PlayBojio.API
cp appsettings.Example.json appsettings.json
```

Edit `appsettings.json` and update:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PlayBojio;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "PlayBojio",
    "Audience": "PlayBojioUsers"
  },
  "R2": {
    "AccessKeyId": "your-r2-access-key",
    "SecretAccessKey": "your-r2-secret-key",
    "AccountId": "your-account-id",
    "BucketName": "your-bucket-name",
    "PublicUrl": "https://your-r2-public-url.r2.dev"
  }
}
```

## Environment Variables

For production deployment, use environment variables instead of `appsettings.json`:

```bash
# Connection String
ConnectionStrings__DefaultConnection="Server=..."

# JWT
Jwt__Key="..."
Jwt__Issuer="PlayBojio"
Jwt__Audience="PlayBojioUsers"

# Cloudflare R2
R2__AccessKeyId="..."
R2__SecretAccessKey="..."
R2__AccountId="..."
R2__BucketName="..."
R2__PublicUrl="..."

# ASP.NET Core
ASPNETCORE_ENVIRONMENT="Production"
ASPNETCORE_URLS="http://0.0.0.0:5000"
```

