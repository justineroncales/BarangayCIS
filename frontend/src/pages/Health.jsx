import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Plus, Edit, Trash2, Heart } from 'lucide-react'
import { toast } from 'react-hot-toast'
import MedicalRecordModal from '../components/MedicalRecordModal'
import './Pages.css'

export default function Health() {
  const [filterType, setFilterType] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editingRecord, setEditingRecord] = useState(null)
  const queryClient = useQueryClient()

  const { data: records = [] } = useQuery({
    queryKey: ['medical-records', filterType],
    queryFn: () =>
      api
        .get('/medical-records', { params: { type: filterType } })
        .then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const deleteMutation = useMutation({
    mutationFn: (id) => api.delete(`/medical-records/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries(['medical-records'])
      toast.success('Medical record deleted successfully')
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to delete medical record')
    },
  })

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Health Center</h1>
          <p>Manage medical records, vaccinations, and medicine inventory</p>
        </div>
        <button className="btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={20} />
          New Record
        </button>
      </div>

      <div className="filters">
        <select
          value={filterType}
          onChange={(e) => setFilterType(e.target.value)}
        >
          <option value="">All Types</option>
          <option value="Checkup">Checkup</option>
          <option value="Vaccination">Vaccination</option>
          <option value="Treatment">Treatment</option>
          <option value="Other">Other</option>
        </select>
      </div>

      {records.length === 0 ? (
        <div className="empty-state">
          <p>No medical records found. Click "New Record" to create one.</p>
        </div>
      ) : (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Resident</th>
                <th>Type</th>
                <th>Date</th>
                <th>Diagnosis</th>
                <th>Attended By</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
            {records.map((record) => (
              <tr key={record.id}>
                <td>
                  {record.resident ? (
                    `${record.resident.firstName} ${record.resident.lastName}`
                  ) : (
                    '-'
                  )}
                </td>
                <td>{record.recordType}</td>
                <td>{new Date(record.recordDate).toLocaleDateString()}</td>
                <td>{record.diagnosis || '-'}</td>
                <td>{record.attendedBy || '-'}</td>
                <td>
                  <div className="action-buttons">
                    <button
                      className="btn-icon"
                      onClick={() => {
                        setEditingRecord(record)
                        setShowModal(true)
                      }}
                      title="Edit"
                    >
                      <Edit size={16} />
                    </button>
                    <button
                      className="btn-icon btn-danger"
                      onClick={() => {
                        if (confirm('Are you sure you want to delete this medical record?')) {
                          deleteMutation.mutate(record.id)
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

      <MedicalRecordModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false)
          setEditingRecord(null)
        }}
        record={editingRecord}
      />
    </div>
  )
}

