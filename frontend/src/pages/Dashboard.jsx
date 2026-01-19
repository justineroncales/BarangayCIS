import { useQuery } from '@tanstack/react-query'
import api from '../services/api'
import {
  Users,
  FileText,
  AlertCircle,
  TrendingUp,
  Activity,
} from 'lucide-react'
import './Pages.css'

export default function Dashboard() {
  const { data: residents = [] } = useQuery({
    queryKey: ['residents'],
    queryFn: () => api.get('/residents').then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const { data: certificates = [] } = useQuery({
    queryKey: ['certificates'],
    queryFn: () => api.get('/certificates').then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const { data: incidents = [] } = useQuery({
    queryKey: ['incidents'],
    queryFn: () => api.get('/incidents').then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const stats = [
    {
      label: 'Total Residents',
      value: residents?.length || 0,
      icon: Users,
      color: 'blue',
    },
    {
      label: 'Certificates Issued',
      value: certificates?.length || 0,
      icon: FileText,
      color: 'green',
    },
    {
      label: 'Active Incidents',
      value: incidents?.filter((i) => i.status === 'Open')?.length || 0,
      icon: AlertCircle,
      color: 'red',
    },
    {
      label: 'Pending Requests',
      value: certificates?.filter((c) => c.status === 'Pending')?.length || 0,
      icon: Activity,
      color: 'yellow',
    },
  ]

  return (
    <div className="dashboard">
      <div className="page-header">
        <h1>Dashboard</h1>
        <p>Welcome to Barangay Citizen Information System</p>
      </div>

      <div className="stats-grid">
        {stats.map((stat) => {
          const Icon = stat.icon
          return (
            <div key={stat.label} className={`stat-card stat-${stat.color}`}>
              <div className="stat-icon">
                <Icon size={32} />
              </div>
              <div className="stat-content">
                <div className="stat-value">{stat.value}</div>
                <div className="stat-label">{stat.label}</div>
              </div>
            </div>
          )
        })}
      </div>

      <div className="dashboard-content">
        <div className="dashboard-section">
          <h2>Recent Activities</h2>
          <div className="activity-list">
            <div className="activity-item">
              <Activity size={16} />
              <span>System initialized successfully</span>
              <span className="activity-time">Just now</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

