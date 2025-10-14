# ENSEK Meter Reading System

A full-stack solution for processing and validating meter readings from CSV files.

## Technology Stack

**Backend:**
- .NET 8.0 Web API
- Entity Framework Core 8.0
- SQL Server LocalDB
- xUnit

**Frontend:**
- Next.js 15 (App Router)
- TypeScript & React 18
- Tailwind CSS

## Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- SQL Server LocalDB

### 1. Setup Database & API

```bash
dotnet ef migrations add InitialCreate --project ENSEK.API
dotnet ef database update --project ENSEK.API

dotnet run --project ENSEK.API
```

API available at:
- **HTTP:** http://localhost:5000
- **Swagger:** http://localhost:5000/swagger

### 2. Run Web Client

```bash
cd ENSEK.Web
npm install
npm run dev
```

Web client available at: **http://localhost:3000**

### 3. Run Tests

```bash
dotnet test
```
