import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Plus, Edit, Trash2, Bell } from 'lucide-react'
import { toast } from 'react-hot-toast'
import AnnouncementModal from '../components/AnnouncementModal'
import './Pages.css'

export default function Announcements() {
  const [filterType, setFilterType] = useState('')
  const [filterStatus, setFilterStatus] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editingAnnouncement, setEditingAnnouncement] = useState(null)
  const queryClient = useQueryClient()

  const { data: announcements = [] } = useQuery({
    queryKey: ['announcements', filterType, filterStatus],
    queryFn: () =>
      api
        .get('/announcements', { params: { type: filterType, status: filterStatus } })
        .then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const deleteMutation = useMutation({
    mutationFn: (id) => api.delete(`/announcements/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries(['announcements'])
      toast.success('Announcement deleted successfully')
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to delete announcement')
    },
  })

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Announcements</h1>
          <p>Create and manage barangay announcements and events</p>
        </div>
        <button className="btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={20} />
          New Announcement
        </button>
      </div>

      <div className="filters">
        <select
          value={filterType}
          onChange={(e) => setFilterType(e.target.value)}
        >
          <option value="">All Types</option>
          <option value="General">General</option>
          <option value="Disaster">Disaster Alert</option>
          <option value="Event">Event</option>
          <option value="Other">Other</option>
        </select>
        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
        >
          <option value="">All Status</option>
          <option value="Active">Active</option>
          <option value="Expired">Expired</option>
          <option value="Archived">Archived</option>
        </select>
      </div>

      {announcements.length === 0 ? (
        <div className="empty-state">
          <p>No announcements found. Click "New Announcement" to create one.</p>
        </div>
      ) : (
        <div className="budget-grid">
          {announcements.map((announcement) => (
            <div key={announcement.id} className="budget-card">
              <div className="budget-header">
                <div>
                  <h3>{announcement.title}</h3>
                  <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem' }}>
                    <span className="badge">{announcement.type}</span>
                    <span className={`badge badge-${announcement.priority.toLowerCase()}`}>
                      {announcement.priority}
                    </span>
                    {announcement.isPublished && (
                      <span className="badge badge-success">Published</span>
                    )}
                  </div>
                </div>
                <div className="action-buttons">
                  <button
                    className="btn-icon"
                    onClick={() => {
                      setEditingAnnouncement(announcement)
                      setShowModal(true)
                    }}
                    title="Edit"
                  >
                    <Edit size={16} />
                  </button>
                  <button
                    className="btn-icon btn-danger"
                    onClick={() => {
                      if (confirm('Are you sure you want to delete this announcement?')) {
                        deleteMutation.mutate(announcement.id)
                      }
                    }}
                    title="Delete"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              </div>
              {announcement.content && (
                <div style={{ marginBottom: '1rem' }}>
                  <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', whiteSpace: 'pre-wrap' }}>
                    {announcement.content.length > 200
                      ? `${announcement.content.substring(0, 200)}...`
                      : announcement.content}
                  </p>
                </div>
              )}
              <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                {announcement.eventDate && (
                  <div>Event: {new Date(announcement.eventDate).toLocaleDateString()}</div>
                )}
                {announcement.location && <div>Location: {announcement.location}</div>}
                <div style={{ marginTop: '0.5rem' }}>
                  Created: {new Date(announcement.createdAt).toLocaleDateString()}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      <AnnouncementModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false)
          setEditingAnnouncement(null)
        }}
        announcement={editingAnnouncement}
      />
    </div>
  )
}

