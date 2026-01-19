import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Plus, Edit, Trash2, Calendar } from 'lucide-react'
import { toast } from 'react-hot-toast'
import StaffTaskModal from '../components/StaffTaskModal'
import './Pages.css'

export default function Staff() {
  const [filterStatus, setFilterStatus] = useState('')
  const [filterAssignedTo, setFilterAssignedTo] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editingTask, setEditingTask] = useState(null)
  const queryClient = useQueryClient()

  const { data: tasks = [] } = useQuery({
    queryKey: ['stafftasks', filterStatus, filterAssignedTo],
    queryFn: () =>
      api
        .get('/stafftasks', { params: { status: filterStatus, assignedTo: filterAssignedTo } })
        .then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const deleteMutation = useMutation({
    mutationFn: (id) => api.delete(`/stafftasks/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries(['stafftasks'])
      toast.success('Task deleted successfully')
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to delete task')
    },
  })

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Staff & Tasks</h1>
          <p>Manage staff schedules, tasks, and attendance</p>
        </div>
        <button className="btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={20} />
          New Task
        </button>
      </div>

      <div className="filters">
        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
        >
          <option value="">All Status</option>
          <option value="Pending">Pending</option>
          <option value="In Progress">In Progress</option>
          <option value="Completed">Completed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
        <input
          type="text"
          placeholder="Filter by assigned staff"
          value={filterAssignedTo}
          onChange={(e) => setFilterAssignedTo(e.target.value)}
        />
      </div>

      {tasks.length === 0 ? (
        <div className="empty-state">
          <p>No tasks found. Click "New Task" to create one.</p>
        </div>
      ) : (
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Task Name</th>
                <th>Assigned To</th>
                <th>Priority</th>
                <th>Status</th>
                <th>Due Date</th>
                <th>Created</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
            {tasks.map((task) => (
              <tr key={task.id}>
                <td>
                  <div style={{ fontWeight: 500 }}>{task.taskName}</div>
                  {task.description && (
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                      {task.description.length > 50 ? `${task.description.substring(0, 50)}...` : task.description}
                    </div>
                  )}
                </td>
                <td>{task.assignedTo}</td>
                <td>
                  <span className={`badge badge-${task.priority.toLowerCase()}`}>
                    {task.priority}
                  </span>
                </td>
                <td>
                  <span className={`badge badge-${task.status.toLowerCase().replace(' ', '-')}`}>
                    {task.status}
                  </span>
                </td>
                <td>{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : '-'}</td>
                <td>{new Date(task.createdAt).toLocaleDateString()}</td>
                <td>
                  <div className="action-buttons">
                    <button
                      className="btn-icon"
                      onClick={() => {
                        setEditingTask(task)
                        setShowModal(true)
                      }}
                      title="Edit"
                    >
                      <Edit size={16} />
                    </button>
                    <button
                      className="btn-icon btn-danger"
                      onClick={() => {
                        if (confirm('Are you sure you want to delete this task?')) {
                          deleteMutation.mutate(task.id)
                        }
                      }}
                      title="Delete"
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            </tbody>
          </table>
        </div>
      )}

      <StaffTaskModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false)
          setEditingTask(null)
        }}
        task={editingTask}
      />
    </div>
  )
}

