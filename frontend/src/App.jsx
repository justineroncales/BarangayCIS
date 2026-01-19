import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import { useAuthStore } from './store/authStore'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import Layout from './components/Layout'
import Residents from './pages/Residents'
import Certificates from './pages/Certificates'
import Incidents from './pages/Incidents'
import Financial from './pages/Financial'
import Projects from './pages/Projects'
import Health from './pages/Health'
import Reports from './pages/Reports'
import ReportBuilder from './pages/ReportBuilder'
import Announcements from './pages/Announcements'
import Staff from './pages/Staff'
import Inventory from './pages/Inventory'
import BusinessPermits from './pages/BusinessPermits'
import Suggestions from './pages/Suggestions'
import Disaster from './pages/Disaster'
import BHW from './pages/BHW'
import SeniorCitizen from './pages/SeniorCitizen'
import CertificatePrint from './pages/CertificatePrint'

function PrivateRoute({ children }) {
  const { isAuthenticated } = useAuthStore()
  return isAuthenticated ? children : <Navigate to="/login" />
}

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route
          path="/certificates/:id/print"
          element={
            <PrivateRoute>
              <CertificatePrint />
            </PrivateRoute>
          }
        />
        <Route
          path="/"
          element={
            <PrivateRoute>
              <Layout />
            </PrivateRoute>
          }
        >
          <Route index element={<Dashboard />} />
          <Route path="residents" element={<Residents />} />
          <Route path="certificates" element={<Certificates />} />
          <Route path="incidents" element={<Incidents />} />
          <Route path="financial" element={<Financial />} />
          <Route path="projects" element={<Projects />} />
          <Route path="health" element={<Health />} />
          <Route path="bhw" element={<BHW />} />
          <Route path="senior-citizen" element={<SeniorCitizen />} />
          <Route path="reports" element={<Reports />} />
          <Route path="report-builder" element={<ReportBuilder />} />
          <Route path="announcements" element={<Announcements />} />
          <Route path="staff" element={<Staff />} />
          <Route path="inventory" element={<Inventory />} />
          <Route path="business-permits" element={<BusinessPermits />} />
          <Route path="suggestions" element={<Suggestions />} />
          <Route path="disaster" element={<Disaster />} />
        </Route>
      </Routes>
    </Router>
  )
}

export default App


