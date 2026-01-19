import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function SeniorCitizenIDModal({ isOpen, onClose, seniorId = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    residentId: '',
    applicationDate: '',
    issueDate: '',
    expiryDate: '',
    status: 'Pending',
    requirementsSubmitted: '',
    requirementsMissing: '',
    remarks: '',
    processedBy: '',
    lastValidatedDate: '',
    nextValidationDate: '',
  })

  const { data: residents = [] } = useQuery({
    queryKey: ['residents'],
    queryFn: () => api.get('/residents').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (seniorId) {
      setFormData({
        residentId: seniorId.residentId || '',
        applicationDate: seniorId.applicationDate ? new Date(seniorId.applicationDate).toISOString().split('T')[0] : '',
        issueDate: seniorId.issueDate ? new Date(seniorId.issueDate).toISOString().split('T')[0] : '',
        expiryDate: seniorId.expiryDate ? new Date(seniorId.expiryDate).toISOString().split('T')[0] : '',
        status: seniorId.status || 'Pending',
        requirementsSubmitted: seniorId.requirementsSubmitted || '',
        requirementsMissing: seniorId.requirementsMissing || '',
        remarks: seniorId.remarks || '',
        processedBy: seniorId.processedBy || '',
        lastValidatedDate: seniorId.lastValidatedDate ? new Date(seniorId.lastValidatedDate).toISOString().split('T')[0] : '',
        nextValidationDate: seniorId.nextValidationDate ? new Date(seniorId.nextValidationDate).toISOString().split('T')[0] : '',
      })
    } else {
      setFormData({
        residentId: '',
        applicationDate: new Date().toISOString().split('T')[0],
        issueDate: '',
        expiryDate: '',
        status: 'Pending',
        requirementsSubmitted: '',
        requirementsMissing: '',
        remarks: '',
        processedBy: '',
        lastValidatedDate: '',
        nextValidationDate: '',
      })
    }
  }, [seniorId, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/senior-citizen-ids', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['senior-citizen-ids'])
      toast.success('Senior Citizen ID created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create Senior Citizen ID')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/senior-citizen-ids/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['senior-citizen-ids'])
      toast.success('Senior Citizen ID updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update Senior Citizen ID')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const submitData = {
      ...formData,
      residentId: parseInt(formData.residentId),
      applicationDate: new Date(formData.applicationDate),
      issueDate: formData.issueDate ? new Date(formData.issueDate) : null,
      expiryDate: formData.expiryDate ? new Date(formData.expiryDate) : null,
      lastValidatedDate: formData.lastValidatedDate ? new Date(formData.lastValidatedDate) : null,
      nextValidationDate: formData.nextValidationDate ? new Date(formData.nextValidationDate) : null,
    }

    if (seniorId) {
      updateMutation.mutate({ id: seniorId.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  // Filter residents who are 60+ years old
  const eligibleResidents = residents.filter((r) => {
    const age = Math.floor((new Date() - new Date(r.dateOfBirth)) / (365.25 * 24 * 60 * 60 * 1000))
    return age >= 60
  })

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{seniorId ? 'Edit Senior Citizen ID' : 'Add New Senior Citizen ID'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label >Resident (60+ years old) *</label>
            <select
              required
              value={formData.residentId}
              onChange={(e) => setFormData({ ...formData, residentId: e.target.value })}
            >
              <option value="">Select Resident...</option>
              {eligibleResidents.map((r) => {
                const age = Math.floor((new Date() - new Date(r.dateOfBirth)) / (365.25 * 24 * 60 * 60 * 1000))
                return (
                  <option key={r.id} value={r.id}>
                    {r.firstName} {r.lastName} - {age} years old - {r.address}
                  </option>
                )
              })}
            </select>
            {eligibleResidents.length === 0 && (
              <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', marginTop: '0.5rem' }}>
                No eligible residents found. Residents must be 60 years or older.
              </p>
            )}
          </div>

          <div className="form-row">
            <div className="form-group">
              <label >Application Date *</label>
              <input
                type="date"
                required
                value={formData.applicationDate}
                onChange={(e) => setFormData({ ...formData, applicationDate: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label >Status *</label>
              <select
                required
                value={formData.status}
                onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              >
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Issued">Issued</option>
                <option value="Expired">Expired</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>
          </div>

          {formData.status === 'Issued' && (
            <>
              <div className="form-row">
                <div className="form-group">
                  <label>Issue Date</label>
                  <input
                    type="date"
                    value={formData.issueDate}
                    onChange={(e) => setFormData({ ...formData, issueDate: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Expiry Date</label>
                  <input
                    type="date"
                    value={formData.expiryDate}
                    onChange={(e) => setFormData({ ...formData, expiryDate: e.target.value })}
                  />
                </div>
              </div>
            </>
          )}

          <div className="form-group">
            <label>Requirements Submitted</label>
            <textarea
              rows="2"
              value={formData.requirementsSubmitted}
              onChange={(e) => setFormData({ ...formData, requirementsSubmitted: e.target.value })}
              placeholder="Comma-separated list of submitted requirements"
            />
          </div>

          <div className="form-group">
            <label>Requirements Missing</label>
            <textarea
              rows="2"
              value={formData.requirementsMissing}
              onChange={(e) => setFormData({ ...formData, requirementsMissing: e.target.value })}
              placeholder="Requirements still needed"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Last Validated Date</label>
              <input
                type="date"
                value={formData.lastValidatedDate}
                onChange={(e) => setFormData({ ...formData, lastValidatedDate: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Next Validation Date</label>
              <input
                type="date"
                value={formData.nextValidationDate}
                onChange={(e) => setFormData({ ...formData, nextValidationDate: e.target.value })}
              />
            </div>
          </div>

          <div className="form-group">
            <label>Processed By</label>
            <input
              type="text"
              value={formData.processedBy}
              onChange={(e) => setFormData({ ...formData, processedBy: e.target.value })}
            />
          </div>

          <div className="form-group">
            <label>Remarks</label>
            <textarea
              rows="2"
              value={formData.remarks}
              onChange={(e) => setFormData({ ...formData, remarks: e.target.value })}
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
              {createMutation.isPending || updateMutation.isPending ? 'Saving...' : seniorId ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

