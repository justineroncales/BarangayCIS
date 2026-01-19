import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function StaffTaskModal({ isOpen, onClose, task = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    taskName: '',
    description: '',
    assignedTo: '',
    priority: 'Normal',
    status: 'Pending',
    dueDate: '',
  })

  useEffect(() => {
    if (task) {
      setFormData({
        taskName: task.taskName || '',
        description: task.description || '',
        assignedTo: task.assignedTo || '',
        priority: task.priority || 'Normal',
        status: task.status || 'Pending',
        dueDate: task.dueDate ? new Date(task.dueDate).toISOString().split('T')[0] : '',
      })
    } else {
      setFormData({
        taskName: '',
        description: '',
        assignedTo: '',
        priority: 'Normal',
        status: 'Pending',
        dueDate: '',
      })
    }
  }, [task, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/stafftasks', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['stafftasks'])
      toast.success('Task created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create task')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/stafftasks/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['stafftasks'])
      toast.success('Task updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update task')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const data = {
      taskName: formData.taskName,
      description: formData.description || null,
      assignedTo: formData.assignedTo,
      priority: formData.priority,
      status: formData.status,
      dueDate: formData.dueDate || null,
    }

    if (task) {
      updateMutation.mutate({ id: task.id, data })
    } else {
      createMutation.mutate(data)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{task ? 'Edit Task' : 'New Task'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label>
              Task Name <span className="required">*</span>
            </label>
            <input
              type="text"
              value={formData.taskName}
              onChange={(e) => setFormData({ ...formData, taskName: e.target.value })}
              required
              placeholder="Task name"
            />
          </div>

          <div className="form-group">
            <label>Description</label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows="3"
              placeholder="Task description"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>
                Assigned To <span className="required">*</span>
              </label>
              <input
                type="text"
                value={formData.assignedTo}
                onChange={(e) => setFormData({ ...formData, assignedTo: e.target.value })}
                required
                placeholder="Staff name"
              />
            </div>

            <div className="form-group">
              <label>
                Priority <span className="required">*</span>
              </label>
              <select
                value={formData.priority}
                onChange={(e) => setFormData({ ...formData, priority: e.target.value })}
                required
              >
                <option value="Low">Low</option>
                <option value="Normal">Normal</option>
                <option value="High">High</option>
                <option value="Urgent">Urgent</option>
              </select>
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
                <option value="Completed">Completed</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>

            <div className="form-group">
              <label>Due Date</label>
              <input
                type="date"
                value={formData.dueDate}
                onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
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
                : task
                ? 'Update Task'
                : 'Create Task'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

