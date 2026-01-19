import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Plus, Edit, Trash2, AlertCircle } from 'lucide-react'
import { toast } from 'react-hot-toast'
import CitizenReportModal from '../components/CitizenReportModal'
import './Pages.css'

export default function Reports() {
  const [filterType, setFilterType] = useState('')
  const [filterStatus, setFilterStatus] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editingReport, setEditingReport] = useState(null)
  const queryClient = useQueryClient()

  const { data: reports = [] } = useQuery({
    queryKey: ['citizenreports', filterType, filterStatus],
    queryFn: () =>
      api
        .get('/citizenreports', { params: { type: filterType, status: filterStatus } })
        .then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const deleteMutation = useMutation({
    mutationFn: (id) => api.delete(`/citizenreports/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries(['citizenreports'])
      toast.success('Report deleted successfully')
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to delete report')
    },
  })

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Citizen Reports</h1>
          <p>Manage citizen assistance requests and reports</p>
        </div>
        <button className="btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={20} />
          New Report
        </button>
      </div>

      <div className="filters">
        <select
          value={filterType}
          onChange={(e) => setFilterType(e.target.value)}
        >
          <option value="">All Types</option>
          <option value="Pothole">Pothole</option>
          <option value="Emergency">Emergency</option>
          <option value="Noise">Noise</option>
          <option value="Other">Other</option>
        </select>
        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
        >
          <option value="">All Status</option>
          <option value="Pending">Pending</option>
          <option value="In Progress">In Progress</option>
          <option value="Resolved">Resolved</option>
          <option value="Closed">Closed</option>
        </select>
      </div>

      {reports.length === 0 ? (
        <div className="empty-state">
          <p>No reports found. Click "New Report" to create one.</p>
        </div>
      ) : (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Type</th>
                <th>Title</th>
                <th>Location</th>
                <th>Reporter</th>
                <th>Status</th>
                <th>Assigned To</th>
                <th>Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
            {reports.map((report) => (
              <tr key={report.id}>
                <td>{report.reportType}</td>
                <td>
                  <div style={{ fontWeight: 500 }}>{report.title}</div>
                </td>
                <td>{report.location || '-'}</td>
                <td>{report.reporterName || '-'}</td>
                <td>
                  <span className={`badge badge-${report.status.toLowerCase().replace(' ', '-')}`}>
                    {report.status}
                  </span>
                </td>
                <td>{report.assignedTo || '-'}</td>
                <td>{new Date(report.createdAt).toLocaleDateString()}</td>
                <td>
                  <div className="action-buttons">
                    <button
                      className="btn-icon"
                      onClick={() => {
                        setEditingReport(report)
                        setShowModal(true)
                      }}
                      title="Edit"
                    >
                      <Edit size={16} />
                    </button>
                    <button
                      className="btn-icon btn-danger"
                      onClick={() => {
                        if (confirm('Are you sure you want to delete this report?')) {
                          deleteMutation.mutate(report.id)
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
      )}

      <CitizenReportModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false)
          setEditingReport(null)
        }}
        report={editingReport}
      />
    </div>
  )
}

