import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Search, Plus, Edit, Trash2, User, AlertCircle, Eye, Upload } from 'lucide-react'
import { toast } from 'react-hot-toast'
import ResidentModal from '../components/ResidentModal'
import ResidentViewModal from '../components/ResidentViewModal'
import Pagination from '../components/Pagination'
import './Pages.css'

export default function Residents() {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [showModal, setShowModal] = useState(false)
  const [showViewModal, setShowViewModal] = useState(false)
  const [editingResident, setEditingResident] = useState(null)
  const [viewingResident, setViewingResident] = useState(null)
  const queryClient = useQueryClient()

  const { data: pagedData, error, isLoading } = useQuery({
    queryKey: ['residents', search, page, pageSize],
    queryFn: () =>
      api.get('/residents', { 
        params: { search, page, pageSize } 
      }).then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const residents = pagedData?.data || []
  const totalPages = pagedData?.totalPages || 0
  const totalCount = pagedData?.totalCount || 0

  // Reset to page 1 when search changes
  const handleSearchChange = (value) => {
    setSearch(value)
    setPage(1)
  }

  const deleteMutation = useMutation({
    mutationFn: ({ id, force = false }) => api.delete(`/residents/${id}${force ? '?force=true' : ''}`),
    onSuccess: () => {
      queryClient.invalidateQueries(['residents'])
      toast.success('Resident deleted successfully')
      // If current page becomes empty after deletion, go to previous page
      if (residents.length === 1 && page > 1) {
        setPage(page - 1)
      }
    },
    onError: (error) => {
      const errorMessage = error.response?.data?.message || error.message || 'Failed to delete resident'
      toast.error(errorMessage)
    }
  })

  const importFromDownloadsMutation = useMutation({
    mutationFn: (bhwName) => api.post(`/residents/import-from-downloads?bhwName=${encodeURIComponent(bhwName || 'Emily rotairo')}`),
    onSuccess: (response) => {
      queryClient.invalidateQueries(['residents'])
      setPage(1) // Reset to first page after import
      const data = response.data
      toast.success(
        `Import completed! Files: ${data.filesProcessed}, Imported: ${data.totalImported}, Errors: ${data.totalErrors}`,
        { duration: 5000 }
      )
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to import residents from Downloads')
    },
  })

  const handleImportFromDownloads = () => {
    if (confirm('This will import all Excel files (.xlsx, .xls) from your Downloads folder. Continue?')) {
      const bhwName = prompt('Enter BHW name to assign (default: Emily rotairo):', 'Emily rotairo') || 'Emily rotairo'
      importFromDownloadsMutation.mutate(bhwName)
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Residents</h1>
          <p>Manage resident information and household records</p>
        </div>
        <div style={{ display: 'flex', gap: '0.75rem' }}>
          <button 
            className="btn-secondary" 
            onClick={handleImportFromDownloads}
            disabled={importFromDownloadsMutation.isPending}
            style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}
          >
            <Upload size={20} />
            {importFromDownloadsMutation.isPending ? 'Importing...' : 'Import from Downloads'}
          </button>
          <button className="btn-primary" onClick={() => setShowModal(true)}>
            <Plus size={20} />
            Add Resident
          </button>
        </div>
      </div>

      <div className="search-bar">
        <Search size={20} />
        <input
          type="text"
          placeholder="Search residents by name, address, or contact..."
          value={search}
          onChange={(e) => handleSearchChange(e.target.value)}
        />
        <select
          value={pageSize}
          onChange={(e) => {
            setPageSize(Number(e.target.value))
            setPage(1)
          }}
          style={{
            marginLeft: '1rem',
            padding: '0.5rem',
            border: '1px solid var(--border-color)',
            borderRadius: '6px',
            background: 'var(--bg)',
            color: 'var(--text)',
            cursor: 'pointer'
          }}
        >
          <option value={10}>10 per page</option>
          <option value={25}>25 per page</option>
          <option value={50}>50 per page</option>
          <option value={100}>100 per page</option>
        </select>
      </div>

      {error && (
        <div className="error-message">
          <AlertCircle size={20} />
          <div>
            <strong>Unable to load residents</strong>
            <p>Please make sure the backend server is running on http://localhost:5000</p>
          </div>
        </div>
      )}

      <div className="table-container">
        <table className="data-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Age</th>
              <th>Gender</th>
              <th>Address</th>
              <th>Contact</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {residents.length === 0 && !error && (
              <tr>
                <td colSpan="7" style={{ textAlign: 'center', padding: '2rem' }}>
                  No residents found. Click "Add Resident" to create your first resident.
                </td>
              </tr>
            )}
            {residents.map((resident) => {
              const age = Math.floor((new Date() - new Date(resident.dateOfBirth)) / (365.25 * 24 * 60 * 60 * 1000))
              return (
                <tr key={resident.id}>
                  <td>
                    <div className="flex items-center gap-2">
                      <User size={16} />
                      <div>
                        <div style={{ fontWeight: 500 }}>
                          {resident.firstName} {resident.middleName ? resident.middleName + ' ' : ''}{resident.lastName} {resident.suffix || ''}
                        </div>
                        {resident.household && (
                          <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                            HH: {resident.household.householdNumber}
                          </div>
                        )}
                      </div>
                    </div>
                  </td>
                  <td>{age} years old</td>
                  <td>{resident.gender}</td>
                  <td>{resident.address}</td>
                  <td>{resident.contactNumber || '-'}</td>
                  <td>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
                      {resident.isVoter && (
                        <span className="badge badge-success" style={{ fontSize: '0.7rem' }}>Voter</span>
                      )}
                      {resident.isPWD && (
                        <span className="badge" style={{ fontSize: '0.7rem', background: 'rgba(59, 130, 246, 0.1)', color: 'var(--accent)' }}>PWD</span>
                      )}
                      {resident.isSenior && (
                        <span className="badge" style={{ fontSize: '0.7rem', background: 'rgba(245, 158, 11, 0.1)', color: 'var(--warning)' }}>Senior</span>
                      )}
                      {!resident.isVoter && !resident.isPWD && !resident.isSenior && (
                        <span className="badge badge-secondary" style={{ fontSize: '0.7rem' }}>-</span>
                      )}
                    </div>
                  </td>
                  <td>
                    <div className="action-buttons">
                      <button
                        className="btn-icon"
                        onClick={() => {
                          setViewingResident(resident)
                          setShowViewModal(true)
                        }}
                        title="View Details"
                      >
                        <Eye size={16} />
                      </button>
                      <button
                        className="btn-icon"
                        onClick={() => {
                          setEditingResident(resident)
                          setShowModal(true)
                        }}
                        title="Edit"
                      >
                        <Edit size={16} />
                      </button>
                      <button
                        className="btn-icon btn-danger"
                        onClick={async () => {
                          if (confirm('Are you sure you want to delete this resident?')) {
                            try {
                              await deleteMutation.mutateAsync({ id: resident.id, force: false })
                            } catch (error) {
                              // If regular delete fails, offer force delete
                              const errorMessage = error.response?.data?.message || error.message || ''
                              if (errorMessage.includes('related records')) {
                                const forceDelete = confirm(
                                  `${errorMessage}\n\nDo you want to force delete? This will permanently delete all related records (certificates, medical records, vaccinations, etc.). This action cannot be undone.`
                                )
                                if (forceDelete) {
                                  deleteMutation.mutate({ id: resident.id, force: true })
                                }
                              }
                            }
                          }
                        }}
                        title="Delete"
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      {isLoading && (
        <div style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
          Loading residents...
        </div>
      )}

      {!isLoading && totalCount > 0 && (
        <Pagination
          currentPage={page}
          totalPages={totalPages}
          onPageChange={setPage}
          pageSize={pageSize}
          totalCount={totalCount}
        />
      )}

      <ResidentModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false)
          setEditingResident(null)
          queryClient.invalidateQueries(['residents'])
        }}
        resident={editingResident}
      />

      <ResidentViewModal
        isOpen={showViewModal}
        onClose={() => {
          setShowViewModal(false)
          setViewingResident(null)
        }}
        resident={viewingResident}
      />
    </div>
  )
}

