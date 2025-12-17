# syntax=docker/dockerfile:1

# Base runtime stage (ASP.NET optimized)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080  

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore (cache layer)
COPY QueryApi.csproj .
RUN dotnet restore

# Copy source and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# Final stage
FROM base AS final

# Create non-root user (already exists in aspnet image, but ensure ownership)
RUN chown -R app /app
USER app

# Copy published output
COPY --from=build /app/publish .

# Use non-root port and entry point
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "QueryApi.dll"]