# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PlayBojio.API/PlayBojio.API.csproj", "PlayBojio.API/"]
RUN dotnet restore "PlayBojio.API/PlayBojio.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/PlayBojio.API"
RUN dotnet build "PlayBojio.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PlayBojio.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000

# Copy published files
COPY --from=publish /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PlayBojio.API.dll"]

