# 🏥 Barangay Pharma System

A pharmacy management system built for barangay health centers.
Handles patient records, medicine inventory, prescriptions, dispensing, refill requests, and audit trails.

## Stack
- **Framework**: ASP.NET Core MVC (.NET 8)
- **Database**: SQL Server 2025 via Entity Framework Core
- **UI**: Bootstrap 5 · Chart.js · DataTables.js
- **Fonts**: DM Sans (headings) · Nunito (body)
- **Theme**: Medical Blue (`#1A6FA3`)

## Setup

### Prerequisites
- .NET 8 SDK
- SQL Server (Express or full) on `localhost\SQLEXPRESS`
- Git

### Local Development

```bash
# 1. Clone the repository
git clone https://github.com/tsugumii21/Final-Project-in-IT-Elective-2-Barangay-Pharma-System-.git
cd Final-Project-in-IT-Elective-2-Barangay-Pharma-System-

# 2. Restore NuGet packages
dotnet restore

# 3. Apply database migrations
dotnet ef database update

# 4. Run the application
dotnet run
```

The app will be available at `https://localhost:5001` (or the port shown in terminal).

## Default Admin Account
On first run, a seed Admin account is created automatically:
- **Email**: `admin@barangaypharma.local`
- **Password**: `Admin@1234`

> ⚠️ Change the admin password immediately after first login in production.

## Roles
| Role | Access |
|------|--------|
| `Admin` | Full system access — users, patients, medicines, reports |
| `Staff` | Patients, medicines, prescriptions, dispensing, refill approvals |
| `Patient` | Own prescriptions and refill requests only |

## Environment Variables
All secrets are managed in `appsettings.json` (not committed). Copy `appsettings.json.example` and fill in your values.

## Running Tests
```bash
dotnet test
```

## Developer
Built with Google Antigravity (Agentic AI) · IT Elective 2 Final Project
