## Overview

The T9 Word Matcher converts numeric input from a T9 keypad (like old mobile phones) into possible matching words. Users can type digit sequences and see matching words in real-time. For example, typing "4663" could match "home" or "good".

## Technology Stack

- **Frontend**: React 19 with Vite
- **Backend**: ASP.NET Core 8.0
- **Containerization**: Docker
- **API Documentation**: Swagger

## Development Setup

### Prerequisites

- Node.js 18+
- .NET 8 SDK
- Docker Desktop (for containerization)

## Setup for new developers

1. Unzip the solution
2. Restore backend packages: `cd T9Backend && dotnet restore`
3. Install frontend dependencies: `cd T9Frontend && npm install`
4. Follow the development or Docker instructions above to run the application


## Integrated Development (Backend and Frontend at different endpoint of the same webservice)
To run the full application in development mode:

1. Build the frontend to the backend's wwwroot directory:
```bash
cd T9Solution/T9Frontend
npm install
npm run build
```

2. Run the backend with the built frontend:
```bash
cd T9Solution/T9Backend
dotnet run
```
This will serve both the API and the built frontend from the backend's server. Access the full application at http://localhost:5019/

## Individual Development

### Frontend Development

```bash
cd T9Solution/T9Frontend
npm install
npm run dev
```
This will start the frontend development server in a separate port.

### Backend Development
```bash
cd T9Solution/T9Backend
dotnet run
```
This starts the ASP.NET Core server at http://localhost:5019, which serves the API endpoints.



## Docker Setup
The application uses a multi-stage Docker build to create a single container that serves both frontend and backend.

Building and Running with Docker
```bash
cd T9Solution
docker build -t t9wordmatcher:latest .
docker run -d -p 8080:80 --name t9app t9wordmatcher:latest
```

OR

```bash
cd T9Solution
docker-compose up -d
```


This builds the Docker image with the tag `t9wordmatcher:latest` and runs a container named `t9app`. 
The containerized application will be accessible at http://localhost:8080.

## Configuration
The application supports environment-specific configuration through appsettings.json:
```json
{
  "T9": {
    "DictionaryPath": "Data/words.txt"
  },
  "RateLimit": {
    "RequestsPerWindow": 100,
    "WindowSizeInSeconds": 60,
    "Enabled": true
  }
}
```

## API Documentation
When running in Development mode, Swagger documentation is available at:

Local development: http://localhost:5019/swagger
Docker container: http://localhost:8080/swagger

The main API endpoint is:
```bash
GET /api/words/match?digits=4663
```

Parameters:
digits: The T9 digit sequence to match (required)
Add "0" at the end for strict matching mode

```json
{
  "input": "4663",
  "strict": false,
  "matches": ["good", "home", "hood", ...]
}
```

# Common Commands

## Development mode (separate servers)
```bash
npm run dev        # Start frontend dev server
dotnet run         # Start backend server
```
## Build frontend to backend wwwroot
```bash
npm run build
```

## Docker commands

```bash

# Build image
docker build -t t9wordmatcher .                         

# Run container
docker run -d -p 8080:80 --name t9app t9wordmatcher     

# Alternative: Run with Docker Compose
docker-compose up -d                                    

# View container logs
docker logs t9app                                       

# List running containers
docker-compose ps                                       

# Stop and remove container
docker-compose down                                     

```