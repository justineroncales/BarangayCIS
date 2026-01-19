# Barangay Citizen Information System (CIS)

A comprehensive desktop-like web application for managing barangay operations, built with C# ASP.NET Core backend and React frontend with Electron wrapper.

## Features

### 1. Citizen Information System (CIS)
- Centralized database of all residents
- Full resident profiles with detailed information
- Household records management
- Age, employment, voter status tracking
- Quick search functionality
- Offline-ready (sync when online)

### 2. Digital Certificate Issuance System
- Barangay Clearance
- Indigency Certificate
- Residency Certificate
- Business Permits
- Barangay ID printing
- QR code authentication
- Online request + SMS pickup notification
- Auto-logging for transparency

### 3. Incident & Blotter Recording System
- Digital recording of complaints, blotter, cases, and incident reports
- Action tracking
- Mediation/hustisya system scheduling
- Audit log
- Image attachment support
- Secure user roles

### 4. Financial & Budget Tracker
- Fund allocation
- Budget tracking
- Expense monitoring
- COA-friendly reports
- Inventory of equipment and supplies

### 5. Project Monitoring System
- Track ongoing projects
- Contractor management
- Budget tracking
- Target dates and progress reporting
- Before/after photos

### 6. Health Center & Medical Records
- Patient records
- Vaccination tracking
- Medicine inventory
- Maternal health monitoring
- Senior/PWD checkup logs

### 7. Citizen Assistance App
- Report issues (potholes, emergencies, noise complaints)
- Track status of reports
- Push notifications for updates

### 8. Event & Announcement System
- Announcements
- Disaster alerts
- Schedules (clean-up drives, feeding programs, SK events, vaccination)
- QR code bulletin board support

### 9. Disaster Response & Emergency Mapping
- Evacuation centers mapping
- Flood-prone areas
- Hazard zones
- Real-time reports during calamities
- Evacuee and relief goods tracking

### 10. Staff Task & Scheduling System
- Staff attendance tracking
- Task assignments
- Patrol schedules
- Barangay Tanod shift monitoring

### 11. Asset & Inventory System
- Track all barangay-owned items
- Borrowing/return log
- Maintenance schedule

### 12. Business Permit Pre-Assessment Tool
- Checklist of requirements
- Pre-assessment
- Step-by-step guidance
- Annual renewal reminders

### 13. Online Suggestion Box
- Anonymous feedback system
- Auto-categorize reports
- Response management

## Technology Stack

### Backend
- **C# ASP.NET Core 8.0** - Web API
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT Authentication** - Security
- **Swagger** - API Documentation

### Frontend
- **React 18** - UI Framework
- **Vite** - Build Tool
- **React Router** - Routing
- **TanStack Query** - Data Fetching
- **Zustand** - State Management
- **Electron** - Desktop Wrapper
- **Lucide React** - Icons

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB or full instance)

### Backend Setup

1. Navigate to the backend directory:
```bash
cd backend/BarangayCIS.API
```

2. Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string"
  }
}
```

3. (Optional) If you already have an existing `BarangayCIS` database that was created before the BHW/Senior Citizen modules were added, run the upgrade script to add the new tables:
```bash
sqlcmd -S "(localdb)\mssqllocaldb" -d BarangayCIS -i backend/BarangayCIS.API/Scripts/AddBhwAndSeniorTables.sql
```
Adjust the `-S`/`-d` parameters if you are not using LocalDB.

4. Restore packages and run:
```bash
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5000`

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend
```

2. Install dependencies:
```bash
npm install
```

3. Run in development mode:
```bash
npm run dev
```

4. Run with Electron (desktop app):
```bash
npm run electron:dev
```

### Default Login Credentials

You'll need to create an admin user first. You can do this by:
1. Registering through the API (requires admin role)
2. Or manually inserting a user in the database

Default credentials (if seeded):
- Username: `admin`
- Password: `admin123`

## Project Structure

```
.
├── backend/
│   ├── BarangayCIS.API/
│   │   ├── Controllers/      # API Controllers
│   │   ├── Models/          # Data Models
│   │   ├── Services/        # Business Logic
│   │   ├── Data/            # DbContext
│   │   └── Program.cs       # Startup
│   └── BarangayCIS.sln
├── frontend/
│   ├── src/
│   │   ├── components/      # React Components
│   │   ├── pages/           # Page Components
│   │   ├── services/        # API Services
│   │   ├── store/           # State Management
│   │   └── App.jsx
│   ├── electron/            # Electron Configuration
│   └── package.json
└── README.md
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - Register new user (Admin only)
- `GET /api/auth/me` - Get current user

### Residents
- `GET /api/residents` - Get all residents (with search)
- `GET /api/residents/{id}` - Get resident by ID
- `POST /api/residents` - Create resident
- `PUT /api/residents/{id}` - Update resident
- `DELETE /api/residents/{id}` - Delete resident

### Certificates
- `GET /api/certificates` - Get all certificates
- `GET /api/certificates/{id}` - Get certificate by ID
- `POST /api/certificates` - Create certificate
- `PUT /api/certificates/{id}` - Update certificate
- `POST /api/certificates/{id}/generate-qr` - Generate QR code

### Incidents
- `GET /api/incidents` - Get all incidents
- `GET /api/incidents/{id}` - Get incident by ID
- `POST /api/incidents` - Create incident
- `PUT /api/incidents/{id}` - Update incident

### Financial
- `GET /api/budgets` - Get all budgets
- `POST /api/budgets` - Create budget
- `GET /api/expenses/budget/{budgetId}` - Get expenses by budget
- `POST /api/expenses` - Create expense
- `GET /api/inventory` - Get inventory items

## Building for Production

### Backend
```bash
cd backend/BarangayCIS.API
dotnet publish -c Release
```

### Frontend (Electron)
```bash
cd frontend
npm run build
npm run electron:build
```

## Development Notes

- The application uses JWT tokens for authentication
- All API endpoints (except login) require authentication
- The database is created automatically on first run using `EnsureCreated()`
- For production, use migrations instead of `EnsureCreated()`

## License

This project is created for barangay management purposes.

## Support

For issues and questions, please contact the development team.


