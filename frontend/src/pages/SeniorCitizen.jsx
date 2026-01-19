import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Search, Plus, Edit, Trash2, User, AlertCircle, Heart, Gift } from 'lucide-react'
import { toast } from 'react-hot-toast'
import SeniorCitizenIDModal from '../components/SeniorCitizenIDModal'
import SeniorCitizenBenefitModal from '../components/SeniorCitizenBenefitModal'
import SeniorHealthMonitoringModal from '../components/SeniorHealthMonitoringModal'
import './Pages.css'

export default function SeniorCitizen() {
  const [activeTab, setActiveTab] = useState('ids')
  const [search, setSearch] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editingItem, setEditingItem] = useState(null)
  const queryClient = useQueryClient()

  const tabs = [
    { id: 'ids', label: 'Senior Citizen IDs', icon: User },
    { id: 'benefits', label: 'Benefits', icon: Gift },
    { id: 'monitoring', label: 'Health Monitoring', icon: Heart },
  ]

  // Senior Citizen IDs
  const { data: seniorIds = [] } = useQuery({
    queryKey: ['senior-citizen-ids', search],
    queryFn: () => api.get('/senior-citizen-ids', { params: { search, status: '' } }).then((res) => res.data),
    enabled: activeTab === 'ids',
  })

  // Benefits
  const { data: benefits = [] } = useQuery({
    queryKey: ['senior-citizen-benefits'],
    queryFn: () => api.get('/senior-citizen-benefits').then((res) => res.data),
    enabled: activeTab === 'benefits',
  })

  // Health Monitoring
  const { data: monitorings = [] } = useQuery({
    queryKey: ['senior-health-monitorings'],
    queryFn: () => api.get('/senior-health-monitorings').then((res) => res.data),
    enabled: activeTab === 'monitoring',
  })

  const deleteMutation = useMutation({
    mutationFn: ({ type, id }) => {
      const endpoints = {
        ids: '/senior-citizen-ids',
        benefits: '/senior-citizen-benefits',
        monitoring: '/senior-health-monitorings',
      }
      return api.delete(`${endpoints[type]}/${id}`)
    },
    onSuccess: (_, variables) => {
      const queryKeys = {
        ids: ['senior-citizen-ids'],
        benefits: ['senior-citizen-benefits'],
        monitoring: ['senior-health-monitorings'],
      }
      queryClient.invalidateQueries(queryKeys[variables.type])
      toast.success('Item deleted successfully')
    },
  })

  const handleDelete = (type, id) => {
    if (window.confirm('Are you sure you want to delete this item?')) {
      deleteMutation.mutate({ type, id })
    }
  }

  const renderIDs = () => (
    <div className="table-container">
      <table className="data-table">
        <thead>
          <tr>
            <th>SC Number</th>
            <th>Resident Name</th>
            <th>Application Date</th>
            <th>Issue Date</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {seniorIds.length === 0 ? (
            <tr>
              <td colSpan="6" style={{ textAlign: 'center', padding: '2rem' }}>
                No Senior Citizen IDs found. Click "Add" to create one.
              </td>
            </tr>
          ) : (
            seniorIds.map((scId) => (
              <tr key={scId.id}>
                <td>{scId.seniorCitizenNumber}</td>
                <td>
                  {scId.resident
                    ? `${scId.resident.firstName} ${scId.resident.lastName}`
                    : '-'}
                </td>
                <td>{new Date(scId.applicationDate).toLocaleDateString()}</td>
                <td>{scId.issueDate ? new Date(scId.issueDate).toLocaleDateString() : '-'}</td>
                <td>
                  <span
                    className={`badge ${
                      scId.status === 'Issued' || scId.status === 'Approved'
                        ? 'badge-success'
                        : scId.status === 'Pending'
                        ? 'badge-warning'
                        : 'badge-secondary'
                    }`}
                  >
                    {scId.status}
                  </span>
                </td>
                <td>
                  <div className="action-buttons">
                    <button onClick={() => { setEditingItem(scId); setShowModal(true) }}>
                      <Edit size={16} />
                    </button>
                    <button onClick={() => handleDelete('ids', scId.id)}>
                      <Trash2 size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )

  const renderBenefits = () => (
    <div className="table-container">
      <table className="data-table">
        <thead>
          <tr>
            <th>Senior Citizen</th>
            <th>Benefit Type</th>
            <th>Amount</th>
            <th>Date</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {benefits.length === 0 ? (
            <tr>
              <td colSpan="6" style={{ textAlign: 'center', padding: '2rem' }}>
                No benefits found.
              </td>
            </tr>
          ) : (
            benefits.map((benefit) => (
              <tr key={benefit.id}>
                <td>
                  {benefit.seniorCitizenID?.resident
                    ? `${benefit.seniorCitizenID.resident.firstName} ${benefit.seniorCitizenID.resident.lastName}`
                    : '-'}
                </td>
                <td>{benefit.benefitType}</td>
                <td>{benefit.amount ? `₱${benefit.amount.toLocaleString()}` : '-'}</td>
                <td>{new Date(benefit.benefitDate).toLocaleDateString()}</td>
                <td>
                  <span
                    className={`badge ${
                      benefit.status === 'Disbursed' || benefit.status === 'Approved'
                        ? 'badge-success'
                        : benefit.status === 'Pending'
                        ? 'badge-warning'
                        : 'badge-secondary'
                    }`}
                  >
                    {benefit.status}
                  </span>
                </td>
                <td>
                  <div className="action-buttons">
                    <button onClick={() => { setEditingItem(benefit); setShowModal(true) }}>
                      <Edit size={16} />
                    </button>
                    <button onClick={() => handleDelete('benefits', benefit.id)}>
                      <Trash2 size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )

  const renderMonitoring = () => (
    <div className="table-container">
      <table className="data-table">
        <thead>
          <tr>
            <th>Senior Citizen</th>
            <th>Monitoring Date</th>
            <th>Type</th>
            <th>Blood Pressure</th>
            <th>Attended By</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {monitorings.length === 0 ? (
            <tr>
              <td colSpan="6" style={{ textAlign: 'center', padding: '2rem' }}>
                No health monitoring records found.
              </td>
            </tr>
          ) : (
            monitorings.map((monitoring) => (
              <tr key={monitoring.id}>
                <td>
                  {monitoring.seniorCitizenID?.resident
                    ? `${monitoring.seniorCitizenID.resident.firstName} ${monitoring.seniorCitizenID.resident.lastName}`
                    : '-'}
                </td>
                <td>{new Date(monitoring.monitoringDate).toLocaleDateString()}</td>
                <td>{monitoring.monitoringType}</td>
                <td>{monitoring.bloodPressure || '-'}</td>
                <td>{monitoring.attendedBy || '-'}</td>
                <td>
                  <div className="action-buttons">
                    <button onClick={() => { setEditingItem(monitoring); setShowModal(true) }}>
                      <Edit size={16} />
                    </button>
                    <button onClick={() => handleDelete('monitoring', monitoring.id)}>
                      <Trash2 size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  )

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Senior Citizens</h1>
          <p>Manage Senior Citizen IDs, benefits, and health monitoring</p>
        </div>
        <button className="btn-primary" onClick={() => { setEditingItem(null); setShowModal(true) }}>
          <Plus size={20} />
          Add {tabs.find(t => t.id === activeTab)?.label.split(' ')[0] || 'Item'}
        </button>
      </div>

      <div className="tabs">
        {tabs.map((tab) => {
          const Icon = tab.icon
          return (
            <button
              key={tab.id}
              className={`tab ${activeTab === tab.id ? 'active' : ''}`}
              onClick={() => setActiveTab(tab.id)}
            >
              <Icon size={18} />
              {tab.label}
            </button>
          )
        })}
      </div>

      {activeTab === 'ids' && (
        <div className="search-bar">
          <Search size={20} />
          <input
            type="text"
            placeholder="Search Senior Citizen IDs..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      )}

      {activeTab === 'ids' && renderIDs()}
      {activeTab === 'benefits' && renderBenefits()}
      {activeTab === 'monitoring' && renderMonitoring()}

      {activeTab === 'ids' && (
        <SeniorCitizenIDModal
          isOpen={showModal}
          onClose={() => { setShowModal(false); setEditingItem(null) }}
          seniorId={editingItem}
        />
      )}
      {activeTab === 'benefits' && (
        <SeniorCitizenBenefitModal
          isOpen={showModal}
          onClose={() => { setShowModal(false); setEditingItem(null) }}
          benefit={editingItem}
        />
      )}
      {activeTab === 'monitoring' && (
        <SeniorHealthMonitoringModal
          isOpen={showModal}
          onClose={() => { setShowModal(false); setEditingItem(null) }}
          monitoring={editingItem}
        />
      )}
    </div>
  )
}

