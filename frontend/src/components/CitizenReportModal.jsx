import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function CitizenReportModal({ isOpen, onClose, report = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    reportType: '',
    title: '',
    description: '',
    location: '',
    reporterName: '',
    reporterContact: '',
    status: 'Pending',
    assignedTo: '',
  })

  useEffect(() => {
    if (report) {
      setFormData({
        reportType: report.reportType || '',
        title: report.title || '',
        description: report.description || '',
        location: report.location || '',
        reporterName: report.reporterName || '',
        reporterContact: report.reporterContact || '',
        status: report.status || 'Pending',
        assignedTo: report.assignedTo || '',
      })
    } else {
      setFormData({
        reportType: '',
        title: '',
        description: '',
        location: '',
        reporterName: '',
        reporterContact: '',
        status: 'Pending',
        assignedTo: '',
      })
    }
  }, [report, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/citizenreports', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['citizenreports'])
      toast.success('Report created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create report')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/citizenreports/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['citizenreports'])
      toast.success('Report updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update report')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const data = {
      reportType: formData.reportType,
      title: formData.title,
      description: formData.description || null,
      location: formData.location || null,
      reporterName: formData.reporterName || null,
      reporterContact: formData.reporterContact || null,
      status: formData.status,
      assignedTo: formData.assignedTo || null,
    }

    if (report) {
      updateMutation.mutate({ id: report.id, data })
    } else {
      createMutation.mutate(data)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{report ? 'Edit Report' : 'New Report'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label>
              Report Type <span className="required">*</span>
            </label>
            <select
              value={formData.reportType}
              onChange={(e) => setFormData({ ...formData, reportType: e.target.value })}
              required
            >
              <option value="">Select type</option>
              <option value="Pothole">Pothole</option>
              <option value="Emergency">Emergency</option>
              <option value="Noise">Noise Complaint</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div className="form-group">
            <label>
              Title <span className="required">*</span>
            </label>
            <input
              type="text"
              value={formData.title}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
              required
              placeholder="Report title"
            />
          </div>

          <div className="form-group">
            <label>Description</label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows="3"
              placeholder="Detailed description"
            />
          </div>

          <div className="form-group">
            <label>Location</label>
            <input
              type="text"
              value={formData.location}
              onChange={(e) => setFormData({ ...formData, location: e.target.value })}
              placeholder="Location of the issue"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Reporter Name</label>
              <input
                type="text"
                value={formData.reporterName}
                onChange={(e) => setFormData({ ...formData, reporterName: e.target.value })}
                placeholder="Optional"
              />
            </div>

            <div className="form-group">
              <label>Reporter Contact</label>
              <input
                type="text"
                value={formData.reporterContact}
                onChange={(e) => setFormData({ ...formData, reporterContact: e.target.value })}
                placeholder="Phone number"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>
                Status <span className="required">*</span>
              </label>
              <select
                value={formData.status}
                onChange={(e) => setFormData({ ...formData, status: e.target.value })}
                required
              >
                <option value="Pending">Pending</option>
                <option value="In Progress">In Progress</option>
                <option value="Resolved">Resolved</option>
                <option value="Closed">Closed</option>
              </select>
            </div>

            <div className="form-group">
              <label>Assigned To</label>
              <input
                type="text"
                value={formData.assignedTo}
                onChange={(e) => setFormData({ ...formData, assignedTo: e.target.value })}
                placeholder="Staff name"
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
              disabled={createMutation.isLoading || updateMutation.isLoading}
            >
              {createMutation.isLoading || updateMutation.isLoading
                ? 'Saving...'
                : report
                ? 'Update Report'
                : 'Create Report'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

