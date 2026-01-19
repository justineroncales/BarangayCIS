import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function AnnouncementModal({ isOpen, onClose, announcement = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    title: '',
    content: '',
    type: 'General',
    priority: 'Normal',
    eventDate: '',
    eventEndDate: '',
    location: '',
    status: 'Active',
    isPublished: false,
  })

  useEffect(() => {
    if (announcement) {
      setFormData({
        title: announcement.title || '',
        content: announcement.content || '',
        type: announcement.type || 'General',
        priority: announcement.priority || 'Normal',
        eventDate: announcement.eventDate ? new Date(announcement.eventDate).toISOString().split('T')[0] : '',
        eventEndDate: announcement.eventEndDate ? new Date(announcement.eventEndDate).toISOString().split('T')[0] : '',
        location: announcement.location || '',
        status: announcement.status || 'Active',
        isPublished: announcement.isPublished || false,
      })
    } else {
      setFormData({
        title: '',
        content: '',
        type: 'General',
        priority: 'Normal',
        eventDate: '',
        eventEndDate: '',
        location: '',
        status: 'Active',
        isPublished: false,
      })
    }
  }, [announcement, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/announcements', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['announcements'])
      toast.success('Announcement created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create announcement')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/announcements/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['announcements'])
      toast.success('Announcement updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update announcement')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const data = {
      title: formData.title,
      content: formData.content || null,
      type: formData.type,
      priority: formData.priority,
      eventDate: formData.eventDate || null,
      eventEndDate: formData.eventEndDate || null,
      location: formData.location || null,
      status: formData.status,
      isPublished: formData.isPublished,
    }

    if (announcement) {
      updateMutation.mutate({ id: announcement.id, data })
    } else {
      createMutation.mutate(data)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{announcement ? 'Edit Announcement' : 'New Announcement'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label>
              Title <span className="required">*</span>
            </label>
            <input
              type="text"
              value={formData.title}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
              required
              placeholder="Announcement title"
            />
          </div>

          <div className="form-group">
            <label>Content</label>
            <textarea
              value={formData.content}
              onChange={(e) => setFormData({ ...formData, content: e.target.value })}
              rows="5"
              placeholder="Announcement content"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>
                Type <span className="required">*</span>
              </label>
              <select
                value={formData.type}
                onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                required
              >
                <option value="General">General</option>
                <option value="Disaster">Disaster Alert</option>
                <option value="Event">Event</option>
                <option value="Other">Other</option>
              </select>
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
              <label>Event Date</label>
              <input
                type="date"
                value={formData.eventDate}
                onChange={(e) => setFormData({ ...formData, eventDate: e.target.value })}
              />
            </div>

            <div className="form-group">
              <label>Event End Date</label>
              <input
                type="date"
                value={formData.eventEndDate}
                onChange={(e) => setFormData({ ...formData, eventEndDate: e.target.value })}
                min={formData.eventDate}
              />
            </div>
          </div>

          <div className="form-group">
            <label>Location</label>
            <input
              type="text"
              value={formData.location}
              onChange={(e) => setFormData({ ...formData, location: e.target.value })}
              placeholder="Event location"
            />
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
                <option value="Active">Active</option>
                <option value="Expired">Expired</option>
                <option value="Archived">Archived</option>
              </select>
            </div>

            <div className="form-group">
              <label>
                <input
                  type="checkbox"
                  checked={formData.isPublished}
                  onChange={(e) => setFormData({ ...formData, isPublished: e.target.checked })}
                  style={{ marginRight: '0.5rem' }}
                />
                Published
              </label>
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
                : announcement
                ? 'Update Announcement'
                : 'Create Announcement'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

