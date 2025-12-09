# PlayBojio Backend API

Board game community platform backend built with .NET 8 and ASP.NET Core.

## Features

- 🎮 **Session Management** - Create and join board game sessions
- 📅 **Event System** - Organize recurring game events
- 👥 **Group Features** - Create groups with admin controls
- 👤 **User Profiles** - Profile pictures and customization
- 🔒 **Authentication** - JWT-based secure authentication
- 🚫 **Blacklist System** - Host management tools
- 👫 **Friend System** - Connect with other players
- 🔍 **Advanced Search** - Search sessions and events with filters
- 🖼️ **Media Storage** - Cloudflare R2 integration for images
- 📊 **Pagination** - Efficient data loading
- 🔐 **Visibility Controls** - Public, Group Only, and Invite Only options

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

### 1. Clone the Repository

```bash
git clone git@github.com:wenextcoder/playbojio-backend.git
cd playbojio-backend
```

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

### 4. Run Database Migrations

```bash
cd PlayBojio.API

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Run migrations
dotnet ef database update
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at:
- **HTTP**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user

### Sessions
- `GET /api/sessions` - List sessions (with filters)
- `GET /api/sessions/{id}` - Get session details
- `GET /api/sessions/slug/{slug}` - Get session by slug
- `POST /api/sessions` - Create session
- `PUT /api/sessions/{id}` - Update session
- `DELETE /api/sessions/{id}` - Cancel session
- `POST /api/sessions/{id}/join` - Join session
- `POST /api/sessions/{id}/leave` - Leave session
- `GET /api/sessions/my-sessions` - Get user's sessions

### Events
- `GET /api/events` - List events (with filters)
- `GET /api/events/{id}` - Get event details
- `GET /api/events/slug/{slug}` - Get event by slug
- `POST /api/events` - Create event
- `PUT /api/events/{id}` - Update event
- `DELETE /api/events/{id}` - Cancel event
- `POST /api/events/{id}/join` - Join event
- `POST /api/events/{id}/leave` - Leave event
- `GET /api/events/my-events` - Get user's events

### Groups
- `GET /api/groups` - List all groups
- `GET /api/groups/my-groups` - Get user's groups
- `GET /api/groups/{id}` - Get group details
- `POST /api/groups` - Create group
- `PUT /api/groups/{id}` - Update group
- `DELETE /api/groups/{id}` - Delete group
- `POST /api/groups/{id}/join` - Join group
- `POST /api/groups/{id}/leave` - Leave group
- `GET /api/groups/{id}/events` - Get group events
- `GET /api/groups/{id}/sessions` - Get group sessions

### Profile
- `GET /api/profile` - Get user profile
- `PUT /api/profile` - Update profile
- `POST /api/profile/picture` - Update profile picture

### Upload
- `POST /api/upload/image` - Upload image to R2

## Search & Filtering

### Session Filters
- `fromDate` - Filter by start date
- `location` - Filter by location
- `gameType` - Filter by game tags
- `availableOnly` - Show only sessions with available slots
- `newbieFriendly` - Show newbie-friendly sessions
- `searchText` - Search in title, game, description, location
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 30)

### Event Filters
- `fromDate` - Filter by start date
- `location` - Filter by location
- `searchText` - Search in name, description, location
- `page` - Page number
- `pageSize` - Items per page

## Development

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

Update database:
```bash
dotnet ef database update
```

Rollback migration:
```bash
dotnet ef database update PreviousMigrationName
```

### Running Tests

```bash
dotnet test
```

### Building for Production

```bash
dotnet publish -c Release -o ./publish
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

## Deployment

### Windows Server

See [WINDOWS_SERVER_DEPLOYMENT.md](../WINDOWS_SERVER_DEPLOYMENT.md) for detailed instructions.

Quick steps:
1. Install .NET 8 SDK
2. Install SQL Server
3. Clone repository
4. Configure `appsettings.json`
5. Run migrations: `dotnet ef database update`
6. Publish: `dotnet publish -c Release`
7. Set up as Windows Service using NSSM or IIS

### Docker

```bash
docker-compose up -d
```

### Azure App Service

See [HOSTING_GUIDE.md](../HOSTING_GUIDE.md) for cloud deployment options.

## API Documentation

Once the application is running, visit:
- **Swagger UI**: http://localhost:5000/swagger
- **OpenAPI JSON**: http://localhost:5000/swagger/v1/swagger.json

## Features in Detail

### Slug-based URLs
Sessions and events support SEO-friendly URLs:
- `/sessions/catan-night-settlers-cafe` instead of `/sessions/123`
- `/events/monthly-game-night` instead of `/events/456`

### Visibility Controls
Three visibility levels:
- **Public**: Listed on public pages, anyone can join
- **Group Only**: Only visible to selected group members
- **Invite Only**: Hidden from listings, requires invite link

### Event Sessions
Sessions can be linked to events. Users must join the parent event before joining event sessions.

### Image Upload
Images are stored on Cloudflare R2 (S3-compatible storage) for:
- Profile pictures
- Session featured images
- Event featured images
- Group profile pictures and cover images

## Security

- JWT-based authentication
- Password hashing with ASP.NET Core Identity
- CORS configuration for frontend
- SQL injection prevention via EF Core
- Input validation on all endpoints

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature-name`
3. Commit changes: `git commit -am 'Add feature'`
4. Push to branch: `git push origin feature-name`
5. Submit a pull request

## License

MIT License - see LICENSE file for details

## Support

For issues and questions:
- GitHub Issues: https://github.com/wenextcoder/playbojio-backend/issues
- Email: support@playbojio.com

## Related Projects

- **Frontend**: [PlayBojio Frontend](https://github.com/wenextcoder/playbojio-frontend) (React + TypeScript)

---

Built with ❤️ for the board gaming community

