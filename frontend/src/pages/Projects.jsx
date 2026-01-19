import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Plus, Edit, Trash2, FolderKanban } from 'lucide-react'
import { toast } from 'react-hot-toast'
import ProjectModal from '../components/ProjectModal'
import './Pages.css'

export default function Projects() {
  const [filterStatus, setFilterStatus] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editingProject, setEditingProject] = useState(null)
  const queryClient = useQueryClient()

  const { data: projects = [] } = useQuery({
    queryKey: ['projects', filterStatus],
    queryFn: () =>
      api
        .get('/projects', { params: { status: filterStatus } })
        .then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const deleteMutation = useMutation({
    mutationFn: (id) => api.delete(`/projects/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries(['projects'])
      toast.success('Project deleted successfully')
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to delete project')
    },
  })

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Project Monitoring</h1>
          <p>Track ongoing barangay projects and progress</p>
        </div>
        <button className="btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={20} />
          New Project
        </button>
      </div>

      <div className="filters">
        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
        >
          <option value="">All Status</option>
          <option value="Planning">Planning</option>
          <option value="Ongoing">Ongoing</option>
          <option value="On Hold">On Hold</option>
          <option value="Completed">Completed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
      </div>

      {projects.length === 0 ? (
        <div className="empty-state">
          <p>No projects found. Click "New Project" to create one.</p>
        </div>
      ) : (
        <div className="budget-grid">
          {projects.map((project) => (
            <div key={project.id} className="budget-card">
              <div className="budget-header">
                <div>
                  <h3>{project.projectName}</h3>
                  <span className="badge">{project.status}</span>
                </div>
                <div className="action-buttons">
                  <button
                    className="btn-icon"
                    onClick={() => {
                      setEditingProject(project)
                      setShowModal(true)
                    }}
                    title="Edit Project"
                  >
                    <Edit size={16} />
                  </button>
                  <button
                    className="btn-icon btn-danger"
                    onClick={() => {
                      if (confirm('Are you sure you want to delete this project?')) {
                        deleteMutation.mutate(project.id)
                      }
                    }}
                    title="Delete Project"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              </div>
              <div className="budget-amounts">
                <div className="amount-item">
                  <span className="amount-label">Budget</span>
                  <span className="amount-value">
                    ₱{project.budget.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                  </span>
                </div>
                {project.spentAmount != null && (
                  <div className="amount-item">
                    <span className="amount-label">Spent</span>
                    <span className="amount-value used">
                      ₱{project.spentAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </span>
                  </div>
                )}
                <div className="amount-item">
                  <span className="amount-label">Progress</span>
                  <span className="amount-value remaining">
                    {project.progress}
                  </span>
                </div>
              </div>
              <div className="budget-progress">
                <div className="progress-bar">
                  <div
                    className="progress-fill"
                    style={{
                      width: `${Math.min(parseInt(project.progress) || 0, 100)}%`,
                    }}
                  />
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '0.5rem', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  <span>Start: {new Date(project.startDate).toLocaleDateString()}</span>
                  <span>Target: {new Date(project.targetCompletionDate).toLocaleDateString()}</span>
                </div>
              </div>
              {project.description && (
                <div style={{ marginTop: '1rem', paddingTop: '1rem', borderTop: '1px solid var(--border-color)' }}>
                  <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>{project.description}</p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      <ProjectModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false)
          setEditingProject(null)
        }}
        project={editingProject}
      />
    </div>
  )
}

