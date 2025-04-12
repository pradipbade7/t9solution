# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy only the .csproj file(s) first to leverage Docker's caching
COPY T9Backend/*.csproj ./T9Backend/

# Restore dependencies
RUN dotnet restore T9Backend/T9Backend.csproj

# Copy the rest of the application files
COPY . .

# Build the application
RUN dotnet publish T9Backend/T9Backend.csproj -c Release -o /out

# Use the official .NET runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published files from the build stage
COPY --from=build /out .

# Expose the port
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "T9Backend.dll"]