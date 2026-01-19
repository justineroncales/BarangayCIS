import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Search, Plus, FileText, QrCode, Edit, Trash2, Printer } from 'lucide-react'
import { toast } from 'react-hot-toast'
import CertificateModal from '../components/CertificateModal'
import './Pages.css'

export default function Certificates() {
  const [search, setSearch] = useState('')
  const [filterType, setFilterType] = useState('')
  const [filterStatus, setFilterStatus] = useState('')
  const [showModal, setShowModal] = useState(false)
  const [editingCertificate, setEditingCertificate] = useState(null)
  const queryClient = useQueryClient()

  const { data: certificates = [] } = useQuery({
    queryKey: ['certificates', filterType, filterStatus],
    queryFn: () =>
      api
        .get('/certificates', { params: { type: filterType, status: filterStatus } })
        .then((res) => res.data),
  })

  const generateQRMutation = useMutation({
    mutationFn: (id) => api.post(`/certificates/${id}/generate-qr`),
    onSuccess: () => {
      queryClient.invalidateQueries(['certificates'])
      toast.success('QR code generated successfully')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id) => api.delete(`/certificates/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries(['certificates'])
      toast.success('Certificate deleted successfully')
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to delete certificate')
    },
  })

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Certificates</h1>
          <p>Manage barangay certificates and documents</p>
        </div>
        <button className="btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={20} />
          Issue Certificate
        </button>
      </div>

      <div className="filters">
        <div className="search-bar">
          <Search size={20} />
          <input
            type="text"
            placeholder="Search certificates..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <select
          value={filterType}
          onChange={(e) => setFilterType(e.target.value)}
        >
          <option value="">All Types</option>
          <option value="Clearance">Clearance</option>
          <option value="Indigency">Indigency</option>
          <option value="Residency">Residency</option>
          <option value="BusinessPermit">Business Permit</option>
          <option value="ID">Barangay ID</option>
        </select>
        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
        >
          <option value="">All Status</option>
          <option value="Pending">Pending</option>
          <option value="Approved">Approved</option>
          <option value="Issued">Issued</option>
        </select>
      </div>

      <div className="table-container">
        <table className="data-table">
          <thead>
            <tr>
              <th>Certificate #</th>
              <th>Type</th>
              <th>Resident</th>
              <th>Issue Date</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {certificates.map((cert) => (
              <tr key={cert.id}>
                <td>{cert.certificateNumber}</td>
                <td>{cert.certificateType}</td>
                <td>
                  {cert.resident?.firstName} {cert.resident?.lastName}
                </td>
                <td>{new Date(cert.issueDate).toLocaleDateString()}</td>
                <td>
                  <span className={`badge badge-${cert.status.toLowerCase()}`}>
                    {cert.status}
                  </span>
                </td>
                <td>
                  <div className="action-buttons">
                    <button
                      className="btn-icon"
                      onClick={() => {
                        setEditingCertificate(cert)
                        setShowModal(true)
                      }}
                      title="Edit"
                    >
                      <Edit size={16} />
                    </button>
                    <button
                      className="btn-icon"
                      onClick={() => generateQRMutation.mutate(cert.id)}
                      title="Generate QR Code"
                    >
                      <QrCode size={16} />
                    </button>
                    <button
                      className="btn-icon"
                      onClick={async () => {
                        try {
                          const token = localStorage.getItem('auth-storage')
                          let authToken = ''
                          if (token) {
                            try {
                              const parsed = JSON.parse(token)
                              if (parsed.state?.token) {
                                authToken = parsed.state.token
                              }
                            } catch (e) {}
                          }

                          if (!authToken) {
                            toast.error('Please login to print certificates')
                            return
                          }

                          // Fetch the print HTML
                          const response = await fetch(`http://localhost:5000/api/certificates/${cert.id}/print`, {
                            headers: {
                              'Authorization': `Bearer ${authToken}`
                            }
                          })

                          if (!response.ok) {
                            toast.error('Failed to load certificate for printing')
                            return
                          }

                          const html = await response.text()
                          
                          // Create a new window and write the HTML
                          const printWindow = window.open('', '_blank', 'width=800,height=600')
                          if (!printWindow) {
                            toast.error('Please allow pop-ups to print certificates')
                            return
                          }
                          
                          printWindow.document.open()
                          printWindow.document.write(html)
                          printWindow.document.close()
                          
                          // Wait for content to load, then print
                          printWindow.onload = () => {
                            setTimeout(() => {
                              printWindow.print()
                            }, 500)
                          }
                          
                          // Fallback in case onload doesn't fire
                          setTimeout(() => {
                            if (printWindow.document.readyState === 'complete') {
                              printWindow.print()
                            }
                          }, 1000)
                          
                        } catch (error) {
                          console.error('Print error:', error)
                          toast.error('Failed to print certificate')
                        }
                      }}
                      title="Print Certificate"
                    >
                      <Printer size={16} />
                    </button>
                    <button
                      className="btn-icon btn-danger"
                      onClick={() => {
                        if (confirm('Are you sure you want to delete this certificate?')) {
                          deleteMutation.mutate(cert.id)
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

      <CertificateModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false)
          setEditingCertificate(null)
        }}
        certificate={editingCertificate}
      />
    </div>
  )
}

