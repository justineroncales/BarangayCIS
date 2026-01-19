import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function BHWAssignmentModal({ isOpen, onClose, assignment = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    bhwProfileId: '',
    zoneName: '',
    zoneDescription: '',
    coverageArea: '',
    assignmentDate: '',
    endDate: '',
    status: 'Active',
    notes: '',
    assignedBy: '',
  })

  const { data: bhwProfiles = [] } = useQuery({
    queryKey: ['bhw-profiles'],
    queryFn: () => api.get('/bhw-profiles').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (assignment) {
      setFormData({
        bhwProfileId: assignment.bhwProfileId || '',
        zoneName: assignment.zoneName || '',
        zoneDescription: assignment.zoneDescription || '',
        coverageArea: assignment.coverageArea || '',
        assignmentDate: assignment.assignmentDate ? new Date(assignment.assignmentDate).toISOString().split('T')[0] : '',
        endDate: assignment.endDate ? new Date(assignment.endDate).toISOString().split('T')[0] : '',
        status: assignment.status || 'Active',
        notes: assignment.notes || '',
        assignedBy: assignment.assignedBy || '',
      })
    } else {
      setFormData({
        bhwProfileId: '',
        zoneName: '',
        zoneDescription: '',
        coverageArea: '',
        assignmentDate: new Date().toISOString().split('T')[0],
        endDate: '',
        status: 'Active',
        notes: '',
        assignedBy: '',
      })
    }
  }, [assignment, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/bhw-assignments', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['bhw-assignments'])
      toast.success('BHW Assignment created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create assignment')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/bhw-assignments/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['bhw-assignments'])
      toast.success('BHW Assignment updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update assignment')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const submitData = {
      ...formData,
      bhwProfileId: parseInt(formData.bhwProfileId),
      assignmentDate: new Date(formData.assignmentDate),
      endDate: formData.endDate ? new Date(formData.endDate) : null,
    }

    if (assignment) {
      updateMutation.mutate({ id: assignment.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{assignment ? 'Edit BHW Assignment' : 'Add New BHW Assignment'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label >BHW *</label>
            <select
              required
              value={formData.bhwProfileId}
              onChange={(e) => setFormData({ ...formData, bhwProfileId: e.target.value })}
            >
              <option value="">Select BHW...</option>
              {bhwProfiles.map((bhw) => (
                <option key={bhw.id} value={bhw.id}>
                  {bhw.bhwNumber} - {bhw.firstName} {bhw.lastName}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label >Zone Name (Purok/Sitio) *</label>
            <input
              type="text"
              required
              value={formData.zoneName}
              onChange={(e) => setFormData({ ...formData, zoneName: e.target.value })}
              placeholder="e.g., Purok 1, Sitio Maligaya"
            />
          </div>

          <div className="form-group">
            <label>Zone Description</label>
            <textarea
              rows="2"
              value={formData.zoneDescription}
              onChange={(e) => setFormData({ ...formData, zoneDescription: e.target.value })}
            />
          </div>

          <div className="form-group">
            <label>Coverage Area</label>
            <input
              type="text"
              value={formData.coverageArea}
              onChange={(e) => setFormData({ ...formData, coverageArea: e.target.value })}
              placeholder="Specific streets/areas"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label >Assignment Date *</label>
              <input
                type="date"
                required
                value={formData.assignmentDate}
                onChange={(e) => setFormData({ ...formData, assignmentDate: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>End Date</label>
              <input
                type="date"
                value={formData.endDate}
                onChange={(e) => setFormData({ ...formData, endDate: e.target.value })}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label >Status *</label>
              <select
                required
                value={formData.status}
                onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              >
                <option value="Active">Active</option>
                <option value="Completed">Completed</option>
                <option value="Transferred">Transferred</option>
              </select>
            </div>
            <div className="form-group">
              <label>Assigned By</label>
              <input
                type="text"
                value={formData.assignedBy}
                onChange={(e) => setFormData({ ...formData, assignedBy: e.target.value })}
              />
            </div>
          </div>

          <div className="form-group">
            <label>Notes</label>
            <textarea
              rows="3"
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
            />
          </div>

          <div className="modal-actions">
            <button type="button" className="btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button
              type="submit"
              className="btn-primary"
              disabled={createMutation.isPending || updateMutation.isPending}
            >
              {createMutation.isPending || updateMutation.isPending ? 'Saving...' : assignment ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

