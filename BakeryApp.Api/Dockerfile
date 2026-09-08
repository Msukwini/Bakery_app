# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["BakeryApp.Api/BakeryApp.Api.csproj", "BakeryApp.Api/"]
COPY ["BakeryApp.Core/BakeryApp.Core.csproj", "BakeryApp.Core/"]
COPY ["BakeryApp.Infrastructure/BakeryApp.Infrastructure.csproj", "BakeryApp.Infrastructure/"]

RUN dotnet restore "BakeryApp.Api/BakeryApp.Api.csproj"

# Copy full source code and publish release build
COPY . .
WORKDIR "/src/BakeryApp.Api"
RUN dotnet publish "BakeryApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create directory for SQLite persistent database
RUN mkdir -p /app/data

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BakeryApp.Api.dll"]