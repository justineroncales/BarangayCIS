import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function BHWVisitLogModal({ isOpen, onClose, visitLog = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    bhwProfileId: '',
    residentId: '',
    visitedPersonName: '',
    address: '',
    visitDate: '',
    visitType: '',
    visitPurpose: '',
    findings: '',
    actionsTaken: '',
    recommendations: '',
    referralStatus: '',
    referralNotes: '',
    notes: '',
  })

  const { data: bhwProfiles = [] } = useQuery({
    queryKey: ['bhw-profiles'],
    queryFn: () => api.get('/bhw-profiles').then((res) => res.data),
    enabled: isOpen,
  })

  const { data: residents = [] } = useQuery({
    queryKey: ['residents'],
    queryFn: () => api.get('/residents').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (visitLog) {
      setFormData({
        bhwProfileId: visitLog.bhwProfileId || '',
        residentId: visitLog.residentId || '',
        visitedPersonName: visitLog.visitedPersonName || '',
        address: visitLog.address || '',
        visitDate: visitLog.visitDate ? new Date(visitLog.visitDate).toISOString().split('T')[0] : '',
        visitType: visitLog.visitType || '',
        visitPurpose: visitLog.visitPurpose || '',
        findings: visitLog.findings || '',
        actionsTaken: visitLog.actionsTaken || '',
        recommendations: visitLog.recommendations || '',
        referralStatus: visitLog.referralStatus || '',
        referralNotes: visitLog.referralNotes || '',
        notes: visitLog.notes || '',
      })
    } else {
      setFormData({
        bhwProfileId: '',
        residentId: '',
        visitedPersonName: '',
        address: '',
        visitDate: new Date().toISOString().split('T')[0],
        visitType: '',
        visitPurpose: '',
        findings: '',
        actionsTaken: '',
        recommendations: '',
        referralStatus: '',
        referralNotes: '',
        notes: '',
      })
    }
  }, [visitLog, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/bhw-visit-logs', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['bhw-visit-logs'])
      toast.success('Visit log created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create visit log')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/bhw-visit-logs/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['bhw-visit-logs'])
      toast.success('Visit log updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update visit log')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const submitData = {
      ...formData,
      bhwProfileId: parseInt(formData.bhwProfileId),
      residentId: formData.residentId ? parseInt(formData.residentId) : null,
      visitDate: new Date(formData.visitDate),
    }

    if (visitLog) {
      updateMutation.mutate({ id: visitLog.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{visitLog ? 'Edit Visit Log' : 'Add New Visit Log'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-row">
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
              <label>Resident (Optional)</label>
              <select
                value={formData.residentId}
                onChange={(e) => setFormData({ ...formData, residentId: e.target.value })}
              >
                <option value="">None - Non-resident visit</option>
                {residents.map((r) => (
                  <option key={r.id} value={r.id}>
                    {r.firstName} {r.lastName} - {r.address}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {!formData.residentId && (
            <div className="form-row">
              <div className="form-group">
                <label>Visited Person Name</label>
                <input
                  type="text"
                  value={formData.visitedPersonName}
                  onChange={(e) => setFormData({ ...formData, visitedPersonName: e.target.value })}
                />
              </div>
              <div className="form-group">
                <label>Address</label>
                <input
                  type="text"
                  value={formData.address}
                  onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                />
              </div>
            </div>
          )}

          <div className="form-row">
            <div className="form-group">
              <label >Visit Date *</label>
              <input
                type="date"
                required
                value={formData.visitDate}
                onChange={(e) => setFormData({ ...formData, visitDate: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label >Visit Type *</label>
              <select
                required
                value={formData.visitType}
                onChange={(e) => setFormData({ ...formData, visitType: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="Home Visit">Home Visit</option>
                <option value="Health Check">Health Check</option>
                <option value="Family Planning">Family Planning</option>
                <option value="Vaccination">Vaccination</option>
                <option value="Follow-up">Follow-up</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>

          <div className="form-group">
            <label>Visit Purpose</label>
            <textarea
              rows="2"
              value={formData.visitPurpose}
              onChange={(e) => setFormData({ ...formData, visitPurpose: e.target.value })}
            />
          </div>

          <div className="form-group">
            <label>Findings</label>
            <textarea
              rows="3"
              value={formData.findings}
              onChange={(e) => setFormData({ ...formData, findings: e.target.value })}
              placeholder="Health findings, observations..."
            />
          </div>

          <div className="form-group">
            <label>Actions Taken</label>
            <textarea
              rows="3"
              value={formData.actionsTaken}
              onChange={(e) => setFormData({ ...formData, actionsTaken: e.target.value })}
              placeholder="What was done during the visit..."
            />
          </div>

          <div className="form-group">
            <label>Recommendations</label>
            <textarea
              rows="2"
              value={formData.recommendations}
              onChange={(e) => setFormData({ ...formData, recommendations: e.target.value })}
              placeholder="Recommendations for follow-up..."
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Referral Status</label>
              <select
                value={formData.referralStatus}
                onChange={(e) => setFormData({ ...formData, referralStatus: e.target.value })}
              >
                <option value="">None</option>
                <option value="Referred to Health Center">Referred to Health Center</option>
                <option value="Referred to Hospital">Referred to Hospital</option>
              </select>
            </div>
          </div>

          {formData.referralStatus && (
            <div className="form-group">
              <label>Referral Notes</label>
              <textarea
                rows="2"
                value={formData.referralNotes}
                onChange={(e) => setFormData({ ...formData, referralNotes: e.target.value })}
              />
            </div>
          )}

          <div className="form-group">
            <label>Notes</label>
            <textarea
              rows="2"
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
              {createMutation.isPending || updateMutation.isPending ? 'Saving...' : visitLog ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

