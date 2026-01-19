import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Search, AlertCircle, Edit, Trash2, Eye } from 'lucide-react'
import { toast } from 'react-hot-toast'
import IncidentModal from '../components/IncidentModal'
import IncidentViewModal from '../components/IncidentViewModal'
import './Pages.css'

export default function Incidents() {
  const [filterType, setFilterType] = useState('')
  const [filterStatus, setFilterStatus] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [showViewModal, setShowViewModal] = useState(false)
  const [editingIncident, setEditingIncident] = useState(null)
  const [viewingIncident, setViewingIncident] = useState(null)
  const queryClient = useQueryClient()

  const { data: incidents = [] } = useQuery({
    queryKey: ['incidents', filterType, filterStatus],
    queryFn: () =>
      api
        .get('/incidents', { params: { type: filterType, status: filterStatus } })
        .then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  // Fetch full incident details when viewing
  const { data: incidentDetails } = useQuery({
    queryKey: ['incident', viewingIncident?.id],
    queryFn: () => api.get(`/incidents/${viewingIncident.id}`).then((res) => res.data),
    enabled: !!viewingIncident?.id && showViewModal,
    retry: false,
  })

  const deleteMutation = useMutation({
    mutationFn: (id) => api.delete(`/incidents/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries(['incidents'])
      toast.success('Incident deleted successfully')
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to delete incident')
    },
  })

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Incidents & Blotter</h1>
          <p>Record and manage barangay incidents, complaints, and cases</p>
        </div>
        <button className="btn-primary" onClick={() => setShowModal(true)}>
          <AlertCircle size={20} />
          New Incident
        </button>
      </div>

      <div className="filters">
        <select
          value={filterType}
          onChange={(e) => setFilterType(e.target.value)}
        >
          <option value="">All Types</option>
          <option value="Complaint">Complaint</option>
          <option value="Blotter">Blotter</option>
          <option value="Case">Case</option>
          <option value="IncidentReport">Incident Report</option>
        </select>
        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
        >
          <option value="">All Status</option>
          <option value="Open">Open</option>
          <option value="Under Investigation">Under Investigation</option>
          <option value="Resolved">Resolved</option>
          <option value="Closed">Closed</option>
        </select>
      </div>

      <div className="table-container">
        <table className="data-table">
          <thead>
            <tr>
              <th>Incident #</th>
              <th>Type</th>
              <th>Title</th>
              <th>Date</th>
              <th>Complainant</th>
              <th>Respondent</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {incidents.length === 0 && (
              <tr>
                <td colSpan="8" style={{ textAlign: 'center', padding: '2rem' }}>
                  No incidents found. Click "New Incident" to create one.
                </td>
              </tr>
            )}
            {incidents.map((incident) => (
              <tr key={incident.id}>
                <td>
                  <div style={{ fontWeight: 500, color: 'var(--accent)' }}>
                    {incident.incidentNumber}
                  </div>
                </td>
                <td>{incident.incidentType}</td>
                <td>
                  <div style={{ maxWidth: '300px' }}>
                    <div style={{ fontWeight: 500 }}>{incident.title}</div>
                    {incident.description && (
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.25rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                        {incident.description}
                      </div>
                    )}
                  </div>
                </td>
                <td>{new Date(incident.incidentDate).toLocaleDateString()}</td>
                <td>
                  {incident.complainant ? (
                    <div>
                      <div>{incident.complainant.firstName} {incident.complainant.lastName}</div>
                    </div>
                  ) : (
                    incident.complainantName || '-'
                  )}
                </td>
                <td>
                  {incident.respondent ? (
                    <div>
                      <div>{incident.respondent.firstName} {incident.respondent.lastName}</div>
                    </div>
                  ) : (
                    incident.respondentName || '-'
                  )}
                </td>
                <td>
                  <span className={`badge badge-${incident.status.toLowerCase().replace(' ', '-')}`}>
                    {incident.status}
                  </span>
                </td>
                <td>
                  <div className="action-buttons">
                    <button
                      className="btn-icon"
                      onClick={() => {
                        setViewingIncident(incident)
                        setShowViewModal(true)
                      }}
                      title="View Details"
                    >
                      <Eye size={16} />
                    </button>
                    <button
                      className="btn-icon"
                      onClick={() => {
                        setEditingIncident(incident)
                        setShowModal(true)
                      }}
                      title="Edit"
                    >
                      <Edit size={16} />
                    </button>
                    <button
                      className="btn-icon btn-danger"
                      onClick={() => {
                        if (confirm('Are you sure you want to delete this incident?')) {
                          deleteMutation.mutate(incident.id)
                        }
                      }}
                      title="Delete"
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <IncidentModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false)
          setEditingIncident(null)
        }}
        incident={editingIncident}
      />

      {showViewModal && (
        <IncidentViewModal
          isOpen={showViewModal}
          onClose={() => {
            setShowViewModal(false)
            setViewingIncident(null)
          }}
          incident={incidentDetails || viewingIncident}
        />
      )}
    </div>
  )
}

