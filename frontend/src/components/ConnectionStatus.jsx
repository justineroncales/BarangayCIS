import { useState, useEffect } from 'react'
import { AlertCircle, CheckCircle, X } from 'lucide-react'
import api from '../services/api'

export default function ConnectionStatus() {
  const [isConnected, setIsConnected] = useState(null)
  const [isChecking, setIsChecking] = useState(true)
  const [isDismissed, setIsDismissed] = useState(false)

  useEffect(() => {
    const checkConnection = async () => {
      try {
        // Try to ping the API - use a simple endpoint that doesn't require auth
        // We'll catch the error if it's just auth-related (401) which means server is up
        await api.get('/residents', { 
          params: { search: '' }, 
          timeout: 3000,
          validateStatus: (status) => status < 500 // Accept 401 as "server is up"
        })
        setIsConnected(true)
        setIsChecking(false)
      } catch (error) {
        // If it's a network/connection error, server is down
        if (error.code === 'ERR_NETWORK' || 
            error.code === 'ERR_CONNECTION_REFUSED' || 
            error.message === 'Network Error') {
          setIsConnected(false)
          setIsChecking(false)
        } else if (error.response) {
          // If we got a response (even 401), server is up
          setIsConnected(true)
          setIsChecking(false)
        } else {
          setIsConnected(false)
          setIsChecking(false)
        }
      }
    }

    checkConnection()
    const interval = setInterval(checkConnection, 10000) // Check every 10 seconds

    return () => clearInterval(interval)
  }, [])

  if (isDismissed || isConnected || isChecking) return null

  return (
    <div className="connection-status error">
      <div className="connection-status-content">
        <AlertCircle size={20} />
        <div>
          <strong>API Connection Error</strong>
          <p>Backend server is not running. Please start the API server on http://localhost:5000</p>
          <div className="connection-help">
            <p><strong>To start the backend:</strong></p>
            <ol>
              <li>Open a terminal/command prompt</li>
              <li>Navigate to: <code>backend/BarangayCIS.API</code></li>
              <li>Run: <code>dotnet run</code></li>
            </ol>
            <p>Or use the batch file: <code>backend/start-api.bat</code></p>
          </div>
        </div>
        <button
          className="connection-dismiss"
          onClick={() => setIsDismissed(true)}
          title="Dismiss"
        >
          <X size={18} />
        </button>
      </div>
    </div>
  )
}

