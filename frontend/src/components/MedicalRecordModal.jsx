import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function MedicalRecordModal({ isOpen, onClose, record = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    residentId: '',
    recordType: '',
    recordDate: '',
    diagnosis: '',
    symptoms: '',
    treatment: '',
    prescription: '',
    attendedBy: '',
    notes: '',
  })

  const { data: residents = [] } = useQuery({
    queryKey: ['residents'],
    queryFn: () => api.get('/residents').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (record) {
      setFormData({
        residentId: record.residentId || '',
        recordType: record.recordType || '',
        recordDate: record.recordDate ? new Date(record.recordDate).toISOString().split('T')[0] : '',
        diagnosis: record.diagnosis || '',
        symptoms: record.symptoms || '',
        treatment: record.treatment || '',
        prescription: record.prescription || '',
        attendedBy: record.attendedBy || '',
        notes: record.notes || '',
      })
    } else {
      const today = new Date().toISOString().split('T')[0]
      setFormData({
        residentId: '',
        recordType: 'Checkup',
        recordDate: today,
        diagnosis: '',
        symptoms: '',
        treatment: '',
        prescription: '',
        attendedBy: '',
        notes: '',
      })
    }
  }, [record, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/medical-records', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['medical-records'])
      toast.success('Medical record created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create medical record')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/medical-records/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['medical-records'])
      toast.success('Medical record updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update medical record')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const data = {
      residentId: parseInt(formData.residentId),
      recordType: formData.recordType,
      recordDate: formData.recordDate,
      diagnosis: formData.diagnosis || null,
      symptoms: formData.symptoms || null,
      treatment: formData.treatment || null,
      prescription: formData.prescription || null,
      attendedBy: formData.attendedBy || null,
      notes: formData.notes || null,
    }

    if (record) {
      updateMutation.mutate({ id: record.id, data })
    } else {
      createMutation.mutate(data)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{record ? 'Edit Medical Record' : 'New Medical Record'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-row">
            <div className="form-group">
              <label>
                Resident <span className="required">*</span>
              </label>
              <select
                value={formData.residentId}
                onChange={(e) => setFormData({ ...formData, residentId: e.target.value })}
                required
              >
                <option value="">Select resident</option>
                {residents.map((r) => (
                  <option key={r.id} value={r.id}>
                    {r.firstName} {r.lastName}
                  </option>
                ))}
              </select>
            </div>

            <div className="form-group">
              <label>
                Record Type <span className="required">*</span>
              </label>
              <select
                value={formData.recordType}
                onChange={(e) => setFormData({ ...formData, recordType: e.target.value })}
                required
              >
                <option value="Checkup">Checkup</option>
                <option value="Vaccination">Vaccination</option>
                <option value="Treatment">Treatment</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>
                Record Date <span className="required">*</span>
              </label>
              <input
                type="date"
                value={formData.recordDate}
                onChange={(e) => setFormData({ ...formData, recordDate: e.target.value })}
                required
              />
            </div>

            <div className="form-group">
              <label>Attended By</label>
              <input
                type="text"
                value={formData.attendedBy}
                onChange={(e) => setFormData({ ...formData, attendedBy: e.target.value })}
                placeholder="Doctor/Nurse name"
              />
            </div>
          </div>

          <div className="form-group">
            <label>Diagnosis</label>
            <input
              type="text"
              value={formData.diagnosis}
              onChange={(e) => setFormData({ ...formData, diagnosis: e.target.value })}
              placeholder="Diagnosis"
            />
          </div>

          <div className="form-group">
            <label>Symptoms</label>
            <textarea
              value={formData.symptoms}
              onChange={(e) => setFormData({ ...formData, symptoms: e.target.value })}
              rows="3"
              placeholder="Symptoms observed"
            />
          </div>

          <div className="form-group">
            <label>Treatment</label>
            <textarea
              value={formData.treatment}
              onChange={(e) => setFormData({ ...formData, treatment: e.target.value })}
              rows="3"
              placeholder="Treatment provided"
            />
          </div>

          <div className="form-group">
            <label>Prescription</label>
            <textarea
              value={formData.prescription}
              onChange={(e) => setFormData({ ...formData, prescription: e.target.value })}
              rows="3"
              placeholder="Prescribed medications"
            />
          </div>

          <div className="form-group">
            <label>Notes</label>
            <textarea
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              rows="2"
              placeholder="Additional notes"
            />
          </div>

          <div className="modal-actions">
            <button type="button" className="btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button
              type="submit"
              className="btn-primary"
              disabled={createMutation.isLoading || updateMutation.isLoading}
            >
              {createMutation.isLoading || updateMutation.isLoading
                ? 'Saving...'
                : record
                ? 'Update Record'
                : 'Create Record'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

