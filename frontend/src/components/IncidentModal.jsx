import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function IncidentModal({ isOpen, onClose, incident = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    incidentType: '',
    title: '',
    description: '',
    location: '',
    incidentDate: new Date().toISOString().split('T')[0],
    complainantId: '',
    respondentId: '',
    complainantName: '',
    respondentName: '',
    status: 'Open',
    actionTaken: '',
    resolution: '',
    resolutionDate: '',
    mediationScheduledDate: '',
    assignedTo: '',
    reportedBy: '',
  })

  const { data: residents = [] } = useQuery({
    queryKey: ['residents'],
    queryFn: () => api.get('/residents').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (incident) {
      setFormData({
        incidentType: incident.incidentType || '',
        title: incident.title || '',
        description: incident.description || '',
        location: incident.location || '',
        incidentDate: incident.incidentDate ? new Date(incident.incidentDate).toISOString().split('T')[0] : '',
        complainantId: incident.complainantId?.toString() || '',
        respondentId: incident.respondentId?.toString() || '',
        complainantName: incident.complainantName || '',
        respondentName: incident.respondentName || '',
        status: incident.status || 'Open',
        actionTaken: incident.actionTaken || '',
        resolution: incident.resolution || '',
        resolutionDate: incident.resolutionDate ? new Date(incident.resolutionDate).toISOString().split('T')[0] : '',
        mediationScheduledDate: incident.mediationScheduledDate ? new Date(incident.mediationScheduledDate).toISOString().split('T')[0] : '',
        assignedTo: incident.assignedTo || '',
        reportedBy: incident.reportedBy || '',
      })
    } else {
      setFormData({
        incidentType: '',
        title: '',
        description: '',
        location: '',
        incidentDate: new Date().toISOString().split('T')[0],
        complainantId: '',
        respondentId: '',
        complainantName: '',
        respondentName: '',
        status: 'Open',
        actionTaken: '',
        resolution: '',
        resolutionDate: '',
        mediationScheduledDate: '',
        assignedTo: '',
        reportedBy: '',
      })
    }
  }, [incident, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/incidents', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['incidents'])
      toast.success('Incident created successfully')
      onClose()
    },
    onError: (error) => {
      const errorMessage = error.response?.data?.message || 
                          error.response?.data?.title ||
                          error.response?.data?.error ||
                          error.response?.data?.errors ? 
                            JSON.stringify(error.response.data.errors) : 
                            'Failed to create incident'
      toast.error(errorMessage)
      console.error('Incident creation error:', error.response?.data)
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/incidents/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['incidents'])
      toast.success('Incident updated successfully')
      onClose()
    },
    onError: (error) => {
      const errorMessage = error.response?.data?.message || 
                          error.response?.data?.title ||
                          error.response?.data?.error ||
                          error.response?.data?.errors ? 
                            JSON.stringify(error.response.data.errors) : 
                            'Failed to update incident'
      toast.error(errorMessage)
      console.error('Incident update error:', error.response?.data)
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    
    // Validate required fields
    if (!formData.incidentType || formData.incidentType === '') {
      toast.error('Please select an incident type')
      return
    }

    if (!formData.title || formData.title.trim() === '') {
      toast.error('Please enter a title')
      return
    }

    if (!formData.incidentDate) {
      toast.error('Please select an incident date')
      return
    }

    // Prepare data with proper field names (PascalCase for backend)
    const submitData = {
      incidentType: formData.incidentType,
      incidentDate: new Date(formData.incidentDate).toISOString(),
      title: formData.title,
      description: formData.description || null,
      location: formData.location || null,
      complainantId: formData.complainantId ? parseInt(formData.complainantId) : null,
      respondentId: formData.respondentId ? parseInt(formData.respondentId) : null,
      complainantName: formData.complainantName || null,
      respondentName: formData.respondentName || null,
      status: formData.status,
      actionTaken: formData.actionTaken || null,
      resolution: formData.resolution || null,
      resolutionDate: formData.resolutionDate ? new Date(formData.resolutionDate).toISOString() : null,
      mediationScheduledDate: formData.mediationScheduledDate ? new Date(formData.mediationScheduledDate).toISOString() : null,
      assignedTo: formData.assignedTo || null,
      reportedBy: formData.reportedBy || null,
    }

    if (incident) {
      updateMutation.mutate({ id: incident.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{incident ? 'Edit Incident' : 'New Incident Report'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-row">
            <div className="form-group">
              <label>Incident Type *</label>
              <select
                required
                value={formData.incidentType}
                onChange={(e) => setFormData({ ...formData, incidentType: e.target.value })}
              >
                <option value="">Select Type...</option>
                <option value="Complaint">Complaint</option>
                <option value="Blotter">Blotter</option>
                <option value="Case">Case</option>
                <option value="IncidentReport">Incident Report</option>
              </select>
            </div>
            <div className="form-group">
              <label>Status *</label>
              <select
                required
                value={formData.status}
                onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              >
                <option value="Open">Open</option>
                <option value="Under Investigation">Under Investigation</option>
                <option value="Resolved">Resolved</option>
                <option value="Closed">Closed</option>
              </select>
            </div>
          </div>

          <div className="form-group">
            <label>Title *</label>
            <input
              type="text"
              required
              value={formData.title}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
              placeholder="Brief title of the incident"
            />
          </div>

          <div className="form-group">
            <label>Description</label>
            <textarea
              rows="4"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              placeholder="Detailed description of the incident"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Incident Date *</label>
              <input
                type="date"
                required
                value={formData.incidentDate}
                onChange={(e) => setFormData({ ...formData, incidentDate: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Location</label>
              <input
                type="text"
                value={formData.location}
                onChange={(e) => setFormData({ ...formData, location: e.target.value })}
                placeholder="Where did it happen?"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Complainant</label>
              <select
                value={formData.complainantId}
                onChange={(e) => setFormData({ ...formData, complainantId: e.target.value })}
              >
                <option value="">Select Resident...</option>
                {residents.map((resident) => (
                  <option key={resident.id} value={resident.id}>
                    {resident.firstName} {resident.lastName}
                  </option>
                ))}
              </select>
              <input
                type="text"
                value={formData.complainantName}
                onChange={(e) => setFormData({ ...formData, complainantName: e.target.value })}
                placeholder="Or enter name manually"
                style={{ marginTop: '0.5rem' }}
              />
            </div>
            <div className="form-group">
              <label>Respondent</label>
              <select
                value={formData.respondentId}
                onChange={(e) => setFormData({ ...formData, respondentId: e.target.value })}
              >
                <option value="">Select Resident...</option>
                {residents.map((resident) => (
                  <option key={resident.id} value={resident.id}>
                    {resident.firstName} {resident.lastName}
                  </option>
                ))}
              </select>
              <input
                type="text"
                value={formData.respondentName}
                onChange={(e) => setFormData({ ...formData, respondentName: e.target.value })}
                placeholder="Or enter name manually"
                style={{ marginTop: '0.5rem' }}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Assigned To</label>
              <input
                type="text"
                value={formData.assignedTo}
                onChange={(e) => setFormData({ ...formData, assignedTo: e.target.value })}
                placeholder="Staff member handling this case"
              />
            </div>
            <div className="form-group">
              <label>Reported By</label>
              <input
                type="text"
                value={formData.reportedBy}
                onChange={(e) => setFormData({ ...formData, reportedBy: e.target.value })}
                placeholder="Who reported this incident"
              />
            </div>
          </div>

          <div className="form-group">
            <label>Action Taken</label>
            <textarea
              rows="3"
              value={formData.actionTaken}
              onChange={(e) => setFormData({ ...formData, actionTaken: e.target.value })}
              placeholder="Actions taken regarding this incident"
            />
          </div>

          <div className="form-group">
            <label>Resolution</label>
            <textarea
              rows="3"
              value={formData.resolution}
              onChange={(e) => setFormData({ ...formData, resolution: e.target.value })}
              placeholder="Resolution or outcome"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Resolution Date</label>
              <input
                type="date"
                value={formData.resolutionDate}
                onChange={(e) => setFormData({ ...formData, resolutionDate: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Mediation Scheduled Date</label>
              <input
                type="date"
                value={formData.mediationScheduledDate}
                onChange={(e) => setFormData({ ...formData, mediationScheduledDate: e.target.value })}
              />
            </div>
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
              {createMutation.isPending || updateMutation.isPending
                ? 'Saving...'
                : incident
                ? 'Update'
                : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}


