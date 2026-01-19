import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function SeniorHealthMonitoringModal({ isOpen, onClose, monitoring = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    seniorCitizenIDId: '',
    monitoringDate: '',
    monitoringType: '',
    bloodPressure: '',
    bloodSugar: '',
    weight: '',
    height: '',
    bmi: '',
    healthFindings: '',
    complaints: '',
    medications: '',
    recommendations: '',
    referralStatus: '',
    referralNotes: '',
    attendedBy: '',
    notes: '',
    nextCheckupDate: '',
  })

  const { data: seniorIds = [] } = useQuery({
    queryKey: ['senior-citizen-ids'],
    queryFn: () => api.get('/senior-citizen-ids').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (monitoring) {
      setFormData({
        seniorCitizenIDId: monitoring.seniorCitizenIDId || '',
        monitoringDate: monitoring.monitoringDate ? new Date(monitoring.monitoringDate).toISOString().split('T')[0] : '',
        monitoringType: monitoring.monitoringType || '',
        bloodPressure: monitoring.bloodPressure || '',
        bloodSugar: monitoring.bloodSugar || '',
        weight: monitoring.weight || '',
        height: monitoring.height || '',
        bmi: monitoring.bmi || '',
        healthFindings: monitoring.healthFindings || '',
        complaints: monitoring.complaints || '',
        medications: monitoring.medications || '',
        recommendations: monitoring.recommendations || '',
        referralStatus: monitoring.referralStatus || '',
        referralNotes: monitoring.referralNotes || '',
        attendedBy: monitoring.attendedBy || '',
        notes: monitoring.notes || '',
        nextCheckupDate: monitoring.nextCheckupDate ? new Date(monitoring.nextCheckupDate).toISOString().split('T')[0] : '',
      })
    } else {
      setFormData({
        seniorCitizenIDId: '',
        monitoringDate: new Date().toISOString().split('T')[0],
        monitoringType: '',
        bloodPressure: '',
        bloodSugar: '',
        weight: '',
        height: '',
        bmi: '',
        healthFindings: '',
        complaints: '',
        medications: '',
        recommendations: '',
        referralStatus: '',
        referralNotes: '',
        attendedBy: '',
        notes: '',
        nextCheckupDate: '',
      })
    }
  }, [monitoring, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/senior-health-monitorings', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['senior-health-monitorings'])
      toast.success('Health monitoring record created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create health monitoring record')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/senior-health-monitorings/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['senior-health-monitorings'])
      toast.success('Health monitoring record updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update health monitoring record')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const submitData = {
      ...formData,
      seniorCitizenIDId: parseInt(formData.seniorCitizenIDId),
      monitoringDate: new Date(formData.monitoringDate),
      nextCheckupDate: formData.nextCheckupDate ? new Date(formData.nextCheckupDate) : null,
    }

    if (monitoring) {
      updateMutation.mutate({ id: monitoring.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{monitoring ? 'Edit Health Monitoring' : 'Add New Health Monitoring'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-row">
            <div className="form-group">
              <label >Senior Citizen *</label>
              <select
                required
                value={formData.seniorCitizenIDId}
                onChange={(e) => setFormData({ ...formData, seniorCitizenIDId: e.target.value })}
              >
                <option value="">Select Senior Citizen...</option>
                {seniorIds.map((sc) => (
                  <option key={sc.id} value={sc.id}>
                    {sc.seniorCitizenNumber} - {sc.resident?.firstName} {sc.resident?.lastName}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label >Monitoring Date *</label>
              <input
                type="date"
                required
                value={formData.monitoringDate}
                onChange={(e) => setFormData({ ...formData, monitoringDate: e.target.value })}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label >Monitoring Type *</label>
              <select
                required
                value={formData.monitoringType}
                onChange={(e) => setFormData({ ...formData, monitoringType: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="Regular Checkup">Regular Checkup</option>
                <option value="Vaccination">Vaccination</option>
                <option value="Health Screening">Health Screening</option>
                <option value="Follow-up">Follow-up</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div className="form-group">
              <label>Attended By</label>
              <input
                type="text"
                value={formData.attendedBy}
                onChange={(e) => setFormData({ ...formData, attendedBy: e.target.value })}
                placeholder="BHW, Nurse, Doctor name"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Blood Pressure</label>
              <input
                type="text"
                value={formData.bloodPressure}
                onChange={(e) => setFormData({ ...formData, bloodPressure: e.target.value })}
                placeholder="e.g., 120/80"
              />
            </div>
            <div className="form-group">
              <label>Blood Sugar</label>
              <input
                type="text"
                value={formData.bloodSugar}
                onChange={(e) => setFormData({ ...formData, bloodSugar: e.target.value })}
                placeholder="Fasting/Random"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Weight (kg)</label>
              <input
                type="text"
                value={formData.weight}
                onChange={(e) => setFormData({ ...formData, weight: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Height (cm)</label>
              <input
                type="text"
                value={formData.height}
                onChange={(e) => setFormData({ ...formData, height: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>BMI</label>
              <input
                type="text"
                value={formData.bmi}
                onChange={(e) => setFormData({ ...formData, bmi: e.target.value })}
              />
            </div>
          </div>

          <div className="form-group">
            <label>Health Findings</label>
            <textarea
              rows="3"
              value={formData.healthFindings}
              onChange={(e) => setFormData({ ...formData, healthFindings: e.target.value })}
              placeholder="General health observations"
            />
          </div>

          <div className="form-group">
            <label>Complaints</label>
            <textarea
              rows="2"
              value={formData.complaints}
              onChange={(e) => setFormData({ ...formData, complaints: e.target.value })}
              placeholder="Health complaints from senior"
            />
          </div>

          <div className="form-group">
            <label>Current Medications</label>
            <textarea
              rows="2"
              value={formData.medications}
              onChange={(e) => setFormData({ ...formData, medications: e.target.value })}
            />
          </div>

          <div className="form-group">
            <label>Recommendations</label>
            <textarea
              rows="2"
              value={formData.recommendations}
              onChange={(e) => setFormData({ ...formData, recommendations: e.target.value })}
              placeholder="Health recommendations"
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
            <div className="form-group">
              <label>Next Checkup Date</label>
              <input
                type="date"
                value={formData.nextCheckupDate}
                onChange={(e) => setFormData({ ...formData, nextCheckupDate: e.target.value })}
              />
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
              {createMutation.isPending || updateMutation.isPending ? 'Saving...' : monitoring ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

