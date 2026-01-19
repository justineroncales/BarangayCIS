# Network Access Guide - Barangay CIS

## Quick Reference

The application is configured to allow access from other PCs on the same network.

## Step-by-Step Instructions

### On the Server PC (where the application is installed)

1. **Start the application:**
   - Run `Start-BarangayCIS.bat`
   - The launcher will display the network IP address
   - Example output:
     ```
     Network Access:
       Local: http://localhost:5000
       Network: http://192.168.1.100:5000
     ```

2. **Find the IP address manually (if needed):**
   - Open Command Prompt
   - Run: `ipconfig`
   - Look for "IPv4 Address" under your network adapter
   - Example: `192.168.1.100`

3. **Configure Windows Firewall:**
   - Open Windows Defender Firewall
   - Click "Advanced settings"
   - Click "Inbound Rules" → "New Rule"
   - Select "Port" → Next
   - Select "TCP" and enter port `5000`
   - Select "Allow the connection"
   - Apply to all profiles
   - Name it "Barangay CIS Port 5000"

### On Other PCs (Client PCs)

1. **Ensure you're on the same network:**
   - Both PCs must be connected to the same Wi-Fi or LAN

2. **Open a web browser:**
   - Chrome, Edge, Firefox, etc.

3. **Enter the server IP address:**
   - Type: `http://[SERVER_IP]:5000`
   - Example: `http://192.168.1.100:5000`
   - Press Enter

4. **Login:**
   - Username: `admin`
   - Password: `admin123`

## Troubleshooting

### Cannot connect from other PC

**Problem:** Browser shows "This site can't be reached" or connection timeout

**Solutions:**
1. **Check Windows Firewall:**
   - Ensure port 5000 is allowed (see above)
   - Temporarily disable firewall to test (re-enable after)

2. **Verify server is running:**
   - Check the server PC - is the application running?
   - Can you access `http://localhost:5000` on the server PC?

3. **Check IP address:**
   - Run `ipconfig` on server PC
   - Ensure you're using the correct IP address
   - Try pinging the server: `ping [SERVER_IP]`

4. **Verify same network:**
   - Both PCs must be on the same network
   - Check network settings on both PCs

5. **Check antivirus:**
   - Some antivirus software blocks network connections
   - Temporarily disable to test

### API connection errors

**Problem:** Frontend loads but shows API connection errors

**Solutions:**
1. The frontend should auto-detect network access
2. If errors persist, check browser console (F12)
3. Verify backend is accessible: `http://[SERVER_IP]:5000/api/health`

### Slow performance

**Problem:** Application is slow when accessed over network

**Solutions:**
1. Check network speed/quality
2. Ensure both PCs have good Wi-Fi/LAN connection
3. Close other bandwidth-intensive applications

## Security Notes

⚠️ **Important Security Considerations:**

1. **Trusted Network Only:**
   - Only enable network access on trusted networks
   - Don't use on public Wi-Fi

2. **Firewall:**
   - Keep Windows Firewall enabled
   - Only allow port 5000 for trusted networks

3. **Password:**
   - Change default password immediately
   - Use strong passwords

4. **Production:**
   - For production deployments, consider:
     - VPN access
     - HTTPS/SSL certificates
     - Additional authentication layers

## Testing Network Access

### Quick Test

1. On server PC, run: `Start-BarangayCIS.bat`
2. Note the IP address shown (e.g., `192.168.1.100`)
3. On another PC, open browser
4. Navigate to: `http://192.168.1.100:5000`
5. Should see the login page

### Verify Backend is Accessible

On client PC, open Command Prompt and run:
```cmd
curl http://[SERVER_IP]:5000/api/health
```

Should return: `{"status":"ok","timestamp":"..."}`

## Multiple Users

The application supports multiple concurrent users:
- Multiple PCs can access simultaneously
- All users share the same database
- Changes made by one user are visible to others

## Port Configuration

To change the port (if 5000 is in use):

1. Edit `Backend\appsettings.json`
2. Change port in Kestrel configuration:
   ```json
   "Kestrel": {
     "Endpoints": {
       "Http": {
         "Url": "http://0.0.0.0:5001"
       }
     }
   }
   ```
3. Update firewall rule for new port
4. Access using new port: `http://[SERVER_IP]:5001`

---

**Need Help?** Check the main INSTALLATION-GUIDE.md for more details.
