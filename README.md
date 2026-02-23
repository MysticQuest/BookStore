# BookStore

A .NET 9 web application for managing a book inventory.

## Tech Stack

- **Backend:** ASP.NET Core 9 Web API
- **Frontend:** Blazor WebAssembly
- **Database:** SQLite with Entity Framework Core
- **Background Jobs:** Hangfire
- **Real-time Updates:** SignalR

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Windows (for PowerShell startup script) or any OS for manual startup

## Getting Started

### Quick Start (Windows)

```powershell
.\start.ps1
```

This starts both servers and opens the browser automatically:
- **Swagger UI:** http://localhost:5029/swagger
- **Blazor Client:** http://localhost:5227

Press `Ctrl+C` to stop all servers.

### Manual Start

```bash
# Terminal 1 - API
cd src/BookStore.Api
dotnet run

# Terminal 2 - Client
cd src/BookStore.Client
dotnet run
```

### Docker

```bash
docker-compose up --build
```

The application will be available at http://localhost

To stop the containers:

```bash
docker-compose down -v
```

Data is persisted in a Docker volume (`bookstore-data`).

## Project Structure

```
src/
├── BookStore.Api/            # REST API + SignalR hub
├── BookStore.Application/    # DTOs, interfaces, business logic contracts
├── BookStore.Domain/         # Entity models
├── BookStore.Infrastructure/ # EF Core, repositories, services
└── BookStore.Client/         # Blazor WebAssembly UI
```

## License

MIT
