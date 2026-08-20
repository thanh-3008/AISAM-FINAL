# AISAM (AI-powered Social Media Advertising Platform)

This repository contains the source code for the AISAM platform, an AI-powered social media management and advertising platform for SMBs.

## Project Structure

- `AISAM-BE/`: Backend API (.NET 9, PostgreSQL, EF Core, integration with Gemini/OpenAI).
- `AISAM-FE/`: Frontend Web Dashboard (Next.js, Tailwind CSS, TypeScript).
- `AISAM-MB/`: Mobile App (Flutter).

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18+)
- [Flutter SDK](https://flutter.dev/docs/get-started/install) (for mobile)
- PostgreSQL Database

## Getting Started

### 1. Backend (AISAM-BE)

Navigate to the backend directory:
```bash
cd AISAM-BE
```

**Configuration:**
Copy `.env.example` to `.env` or set up `appsettings.Development.json` with your database connection strings, JWT secret, and AI API keys.

**Run the API:**
```bash
dotnet restore AISAM.API/AISAM.API.csproj
dotnet build AISAM.API/AISAM.API.csproj
dotnet run --project AISAM.API/AISAM.API.csproj
```

**Run Tests:**
```bash
dotnet test tests/AISAM.IntegrationTests/AISAM.IntegrationTests.csproj
```

### 2. Frontend (AISAM-FE)

Navigate to the frontend directory:
```bash
cd AISAM-FE
```

**Configuration:**
Set up `.env.local` using the keys required by the frontend application (e.g. `NEXT_PUBLIC_API_URL`).

**Install Dependencies & Run:**
```bash
npm install
npm run dev
```
The dashboard will be available at `http://localhost:3000`.

### 3. Mobile (AISAM-MB)

Navigate to the mobile directory:
```bash
cd AISAM-MB
```

**Install Dependencies & Run:**
```bash
flutter pub get
flutter run
```

## Deployment Tools

Deployment scripts and tools (if any) are located within each component directory or managed via CI/CD. Currently, production deployments rely on server-side `.env` files and `systemd` or Docker.
