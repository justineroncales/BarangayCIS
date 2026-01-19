import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import { useAuthStore } from '../store/authStore'
import ConnectionStatus from './ConnectionStatus'
import './Layout.css'
import {
  LayoutDashboard,
  Users,
  FileText,
  AlertCircle,
  Wallet,
  FolderKanban,
  Heart,
  Bell,
  Calendar,
  Briefcase,
  Package,
  FileCheck,
  MessageSquare,
  MapPin,
  LogOut,
  Menu,
  X,
  Stethoscope,
  UserCheck,
  BarChart3,
} from 'lucide-react'
import { useState } from 'react'

const menuItems = [
  { path: '/', icon: LayoutDashboard, label: 'Dashboard' },
  { path: '/residents', icon: Users, label: 'Residents' },
  { path: '/certificates', icon: FileText, label: 'Certificates' },
  { path: '/incidents', icon: AlertCircle, label: 'Incidents' },
  { path: '/financial', icon: Wallet, label: 'Financial' },
  { path: '/projects', icon: FolderKanban, label: 'Projects' },
  { path: '/health', icon: Heart, label: 'Health Center' },
  { path: '/bhw', icon: Stethoscope, label: 'BHW' },
  { path: '/senior-citizen', icon: UserCheck, label: 'Senior Citizens' },
  { path: '/reports', icon: FileText, label: 'Citizen Reports' },
  { path: '/report-builder', icon: BarChart3, label: 'Report Builder' },
  { path: '/announcements', icon: Bell, label: 'Announcements' },
  { path: '/staff', icon: Calendar, label: 'Staff & Tasks' },
  { path: '/inventory', icon: Package, label: 'Inventory' },
  { path: '/business-permits', icon: Briefcase, label: 'Business Permits' },
  { path: '/suggestions', icon: MessageSquare, label: 'Suggestion Box' },
  { path: '/disaster', icon: MapPin, label: 'Disaster Response' },
]

export default function Layout() {
  const navigate = useNavigate()
  const location = useLocation()
  const { user, logout } = useAuthStore()
  const [sidebarOpen, setSidebarOpen] = useState(true)

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="layout">
      {/* Sidebar */}
      <aside className={`sidebar ${sidebarOpen ? 'open' : 'closed'}`}>
        <div className="sidebar-header">
          <h2>Barangay CIS</h2>
          <button
            className="sidebar-toggle"
            onClick={() => setSidebarOpen(!sidebarOpen)}
          >
            {sidebarOpen ? <X size={20} /> : <Menu size={20} />}
          </button>
        </div>

        <nav className="sidebar-nav">
          {menuItems.map((item) => {
            const Icon = item.icon
            const isActive = location.pathname === item.path
            return (
              <button
                key={item.path}
                className={`nav-item ${isActive ? 'active' : ''}`}
                onClick={() => navigate(item.path)}
              >
                <Icon size={20} />
                {sidebarOpen && <span>{item.label}</span>}
              </button>
            )
          })}
        </nav>

        <div className="sidebar-footer">
          <div className="user-info">
            {sidebarOpen && (
              <>
                <div className="user-name">{user?.fullName || user?.username}</div>
                <div className="user-role">{user?.role}</div>
              </>
            )}
          </div>
          <button className="logout-btn" onClick={handleLogout}>
            <LogOut size={20} />
            {sidebarOpen && <span>Logout</span>}
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <main className="main-content">
        <ConnectionStatus />
        <div className="content-wrapper">
          <Outlet />
        </div>
      </main>
    </div>
  )
}

