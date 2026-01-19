# Barangay CIS Desktop Application

A native Windows desktop application for the Barangay Citizen Information System, built with WPF (.NET 8).

## Features

This desktop application provides the same functionality as the web frontend:

- **Residents Management** - Manage resident records and household information
- **Certificates** - Issue and manage barangay certificates
- **Incidents** - Record and track incidents and complaints
- **Financial** - Budget tracking and financial management
- **Projects** - Project monitoring and tracking
- **Health** - Health records and medical information
- **BHW** - Barangay Health Worker management
- **Senior Citizen** - Senior citizen records and benefits
- **Reports** - Generate various reports
- **Announcements** - Manage announcements
- **Staff** - Staff management and scheduling
- **Inventory** - Inventory management
- **Business Permits** - Business permit processing
- **Suggestions** - Suggestion box management
- **Disaster** - Disaster response management

## Prerequisites

- .NET 8.0 SDK
- Windows 10/11
- Backend API running on `http://localhost:5000`

## Building

```bash
cd backend/BarangayCIS.Desktop
dotnet build
```

## Running

```bash
cd backend/BarangayCIS.Desktop
dotnet run
```

Or build and run from Visual Studio.

## Configuration

The desktop app connects to the backend API at `http://localhost:5000/api` by default. This can be configured in the `ApiClient` class.

## Architecture

- **MVVM Pattern** - Uses CommunityToolkit.Mvvm for view models
- **API Client** - Centralized HTTP client for backend communication
- **Authentication** - JWT token-based authentication with token persistence
- **Views** - Modular view components for each feature

## Project Structure

```
BarangayCIS.Desktop/
├── Views/              # XAML views for UI
├── ViewModels/         # View models (MVVM)
├── Models/             # Data models and DTOs
├── Services/           # API client and services
├── Converters/         # Value converters for XAML
└── Properties/         # Application settings
```

## Notes

- The desktop app uses the same backend API as the web frontend
- Authentication tokens are persisted in user settings
- All views are currently placeholder implementations that can be expanded with full functionality
