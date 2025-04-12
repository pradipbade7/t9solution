# Frontend build stage
FROM node:18-alpine AS frontend
WORKDIR /app
# Cache dependencies separately
COPY T9Frontend/package*.json ./
RUN npm ci --quiet
# Copy source and build
COPY T9Frontend/ ./
ENV NODE_ENV=production
# Set environment variable for Docker build
ENV DOCKER_BUILD=true
# Run the build
RUN npm run build

# Backend build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend
WORKDIR /app
# Copy and restore as distinct layers
COPY T9Backend/*.csproj ./
RUN dotnet restore
# Copy everything else
COPY T9Backend/ ./
# Clear existing wwwroot if exists
RUN rm -rf ./wwwroot/* || true
# IMPORTANT: Copy frontend build from the correct location
COPY --from=frontend /app/dist/ ./wwwroot/
# Build with Release configuration
RUN dotnet publish -c Release -o out

# Make sure the Data directory and files are included in the publish output
RUN mkdir -p /app/out/Data
COPY T9Backend/Data/*.txt /app/out/Data/

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
# Copy built app from backend stage
COPY --from=backend /app/out ./

# Verify the data file exists (for debugging)
RUN ls -la /app/Data

# Runtime configuration
ENV ASPNETCORE_URLS=http://+:80

# ENV ASPNETCORE_ENVIRONMENT=Production
# Update this line in your Dockerfile to use Development
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 80
ENTRYPOINT ["dotnet", "T9Backend.dll"]