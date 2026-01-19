import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function BHWTrainingModal({ isOpen, onClose, training = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    bhwProfileId: '',
    trainingTitle: '',
    description: '',
    trainingProvider: '',
    trainingDate: '',
    trainingEndDate: '',
    trainingType: '',
    status: 'Completed',
    certificateNumber: '',
    certificatePath: '',
    notes: '',
  })

  const { data: bhwProfiles = [] } = useQuery({
    queryKey: ['bhw-profiles'],
    queryFn: () => api.get('/bhw-profiles').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (training) {
      setFormData({
        bhwProfileId: training.bhwProfileId || '',
        trainingTitle: training.trainingTitle || '',
        description: training.description || '',
        trainingProvider: training.trainingProvider || '',
        trainingDate: training.trainingDate ? new Date(training.trainingDate).toISOString().split('T')[0] : '',
        trainingEndDate: training.trainingEndDate ? new Date(training.trainingEndDate).toISOString().split('T')[0] : '',
        trainingType: training.trainingType || '',
        status: training.status || 'Completed',
        certificateNumber: training.certificateNumber || '',
        certificatePath: training.certificatePath || '',
        notes: training.notes || '',
      })
    } else {
      setFormData({
        bhwProfileId: '',
        trainingTitle: '',
        description: '',
        trainingProvider: '',
        trainingDate: new Date().toISOString().split('T')[0],
        trainingEndDate: '',
        trainingType: '',
        status: 'Completed',
        certificateNumber: '',
        certificatePath: '',
        notes: '',
      })
    }
  }, [training, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/bhw-trainings', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['bhw-trainings'])
      toast.success('Training record created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create training record')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/bhw-trainings/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['bhw-trainings'])
      toast.success('Training record updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update training record')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const submitData = {
      ...formData,
      bhwProfileId: parseInt(formData.bhwProfileId),
      trainingDate: new Date(formData.trainingDate),
      trainingEndDate: formData.trainingEndDate ? new Date(formData.trainingEndDate) : null,
    }

    if (training) {
      updateMutation.mutate({ id: training.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{training ? 'Edit Training Record' : 'Add New Training Record'}</h2>
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
            <label >Training Title *</label>
            <input
              type="text"
              required
              value={formData.trainingTitle}
              onChange={(e) => setFormData({ ...formData, trainingTitle: e.target.value })}
            />
          </div>

          <div className="form-group">
            <label>Description</label>
            <textarea
              rows="3"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Training Provider</label>
              <input
                type="text"
                value={formData.trainingProvider}
                onChange={(e) => setFormData({ ...formData, trainingProvider: e.target.value })}
                placeholder="DOH, LGU, NGO, etc."
              />
            </div>
            <div className="form-group">
              <label>Training Type</label>
              <select
                value={formData.trainingType}
                onChange={(e) => setFormData({ ...formData, trainingType: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="Basic">Basic</option>
                <option value="Advanced">Advanced</option>
                <option value="Refresher">Refresher</option>
                <option value="Specialized">Specialized</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label >Training Date *</label>
              <input
                type="date"
                required
                value={formData.trainingDate}
                onChange={(e) => setFormData({ ...formData, trainingDate: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Training End Date</label>
              <input
                type="date"
                value={formData.trainingEndDate}
                onChange={(e) => setFormData({ ...formData, trainingEndDate: e.target.value })}
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
                <option value="Completed">Completed</option>
                <option value="In Progress">In Progress</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>
            <div className="form-group">
              <label>Certificate Number</label>
              <input
                type="text"
                value={formData.certificateNumber}
                onChange={(e) => setFormData({ ...formData, certificateNumber: e.target.value })}
              />
            </div>
          </div>

          <div className="form-group">
            <label>Certificate Path</label>
            <input
              type="text"
              value={formData.certificatePath}
              onChange={(e) => setFormData({ ...formData, certificatePath: e.target.value })}
              placeholder="Path to certificate file"
            />
          </div>

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
              {createMutation.isPending || updateMutation.isPending ? 'Saving...' : training ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

