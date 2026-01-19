# Setup Guide - Barangay CIS

## Quick Start

### 1. Backend Setup

```bash
# Navigate to backend
cd backend/BarangayCIS.API

# Restore NuGet packages
dotnet restore

# Update connection string in appsettings.json
# Default uses LocalDB: Server=(localdb)\mssqllocaldb;Database=BarangayCIS;...

# Run the API
dotnet run
```

The API will start on `http://localhost:5000` (or the port shown in console).

**Default Admin Credentials:**
- Username: `admin`
- Password: `admin123`

### 2. Frontend Setup

```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Run development server
npm run dev
```

The frontend will be available at `http://localhost:5173`

### 3. Run as Desktop App (Electron)

```bash
# In the frontend directory
npm run electron:dev
```

This will:
1. Start the Vite dev server
2. Launch Electron window
3. Connect to the backend API

## Database Setup

The application uses SQL Server. You have two options:

### Option 1: LocalDB (Default)
LocalDB is included with Visual Studio and SQL Server Express. The default connection string uses LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=BarangayCIS;Trusted_Connection=True;MultipleActiveResultSets=true
```

### Option 2: Full SQL Server
Update `appsettings.json` with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=BarangayCIS;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

## First Run

1. Start the backend API first
2. The database will be created automatically on first run
3. An admin user will be seeded automatically
4. Start the frontend
5. Login with admin credentials

## Troubleshooting

### Backend Issues

**Port already in use:**
- Change the port in `launchSettings.json` or use `--urls` flag:
  ```bash
  dotnet run --urls "http://localhost:5001"
  ```

**Database connection error:**
- Ensure SQL Server/LocalDB is running
- Check connection string in `appsettings.json`
- For LocalDB, verify it's installed: `sqllocaldb info`

**CORS errors:**
- Check `Program.cs` CORS configuration
- Ensure frontend URL is in allowed origins

### Frontend Issues

**npm install fails:**
- Clear cache: `npm cache clean --force`
- Delete `node_modules` and `package-lock.json`, then reinstall

**Electron won't start:**
- Ensure Node.js 18+ is installed
- Try: `npm install electron --save-dev`

**API connection errors:**
- Verify backend is running on correct port
- Check `src/services/api.js` baseURL matches backend
- Check CORS settings in backend

## Production Build

### Backend
```bash
cd backend/BarangayCIS.API
dotnet publish -c Release -o ./publish
```

### Frontend (Electron)
```bash
cd frontend
npm run build
npm run electron:build
```

## Development Tips

1. **API Testing:** Use Swagger UI at `http://localhost:5000/swagger` when backend is running
2. **Hot Reload:** Both frontend (Vite) and backend (dotnet watch) support hot reload
3. **Database Changes:** Currently uses `EnsureCreated()`. For production, use migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

## Next Steps

1. Create additional user accounts through the admin panel
2. Add resident data
3. Configure certificate templates
4. Set up budget allocations
5. Customize according to your barangay's needs


