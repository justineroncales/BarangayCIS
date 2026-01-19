import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function ProjectModal({ isOpen, onClose, project = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    projectName: '',
    description: '',
    contractor: '',
    budget: '',
    startDate: '',
    targetCompletionDate: '',
    status: 'Planning',
    progress: '0%',
    notes: '',
  })

  useEffect(() => {
    if (project) {
      setFormData({
        projectName: project.projectName || '',
        description: project.description || '',
        contractor: project.contractor || '',
        budget: project.budget || '',
        startDate: project.startDate ? new Date(project.startDate).toISOString().split('T')[0] : '',
        targetCompletionDate: project.targetCompletionDate ? new Date(project.targetCompletionDate).toISOString().split('T')[0] : '',
        status: project.status || 'Planning',
        progress: project.progress || '0%',
        notes: project.notes || '',
      })
    } else {
      const today = new Date().toISOString().split('T')[0]
      setFormData({
        projectName: '',
        description: '',
        contractor: '',
        budget: '',
        startDate: today,
        targetCompletionDate: '',
        status: 'Planning',
        progress: '0%',
        notes: '',
      })
    }
  }, [project, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/projects', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['projects'])
      toast.success('Project created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create project')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/projects/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['projects'])
      toast.success('Project updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update project')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()

    const startDate = new Date(formData.startDate)
    const targetDate = new Date(formData.targetCompletionDate)
    
    if (targetDate <= startDate) {
      toast.error('Target completion date must be after start date')
      return
    }

    const data = {
      projectName: formData.projectName,
      description: formData.description || null,
      contractor: formData.contractor || null,
      budget: parseFloat(formData.budget),
      startDate: formData.startDate,
      targetCompletionDate: formData.targetCompletionDate,
      status: formData.status,
      progress: formData.progress,
      notes: formData.notes || null,
    }

    if (project) {
      updateMutation.mutate({ id: project.id, data })
    } else {
      createMutation.mutate(data)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{project ? 'Edit Project' : 'New Project'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label>
              Project Name <span className="required">*</span>
            </label>
            <input
              type="text"
              value={formData.projectName}
              onChange={(e) => setFormData({ ...formData, projectName: e.target.value })}
              required
              placeholder="e.g., Road Construction - Main Street"
            />
          </div>

          <div className="form-group">
            <label>Description</label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows="3"
              placeholder="Project description and details"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Contractor</label>
              <input
                type="text"
                value={formData.contractor}
                onChange={(e) => setFormData({ ...formData, contractor: e.target.value })}
                placeholder="Contractor name"
              />
            </div>

            <div className="form-group">
              <label>
                Budget (₱) <span className="required">*</span>
              </label>
              <input
                type="number"
                step="0.01"
                min="0.01"
                value={formData.budget}
                onChange={(e) => setFormData({ ...formData, budget: e.target.value })}
                required
                placeholder="0.00"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>
                Start Date <span className="required">*</span>
              </label>
              <input
                type="date"
                value={formData.startDate}
                onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                required
              />
            </div>

            <div className="form-group">
              <label>
                Target Completion Date <span className="required">*</span>
              </label>
              <input
                type="date"
                value={formData.targetCompletionDate}
                onChange={(e) => setFormData({ ...formData, targetCompletionDate: e.target.value })}
                required
                min={formData.startDate}
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
                <option value="Planning">Planning</option>
                <option value="Ongoing">Ongoing</option>
                <option value="On Hold">On Hold</option>
                <option value="Completed">Completed</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>

            <div className="form-group">
              <label>
                Progress <span className="required">*</span>
              </label>
              <input
                type="text"
                value={formData.progress}
                onChange={(e) => setFormData({ ...formData, progress: e.target.value })}
                required
                placeholder="e.g., 50%"
              />
            </div>
          </div>

          <div className="form-group">
            <label>Notes</label>
            <textarea
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              rows="3"
              placeholder="Additional notes or remarks"
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
                : project
                ? 'Update Project'
                : 'Create Project'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

