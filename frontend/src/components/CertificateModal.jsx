import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function CertificateModal({ isOpen, onClose, certificate = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    certificateType: '',
    residentId: '',
    purpose: '',
    issueDate: new Date().toISOString().split('T')[0],
    expiryDate: '',
    status: 'Pending',
    issuedBy: '',
  })

  const { data: residents = [] } = useQuery({
    queryKey: ['residents'],
    queryFn: () => api.get('/residents').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (certificate) {
      setFormData({
        certificateType: certificate.certificateType || '',
        residentId: certificate.residentId?.toString() || '',
        purpose: certificate.purpose || '',
        issueDate: certificate.issueDate ? new Date(certificate.issueDate).toISOString().split('T')[0] : '',
        expiryDate: certificate.expiryDate ? new Date(certificate.expiryDate).toISOString().split('T')[0] : '',
        status: certificate.status || 'Pending',
        issuedBy: certificate.issuedBy || '',
      })
    } else {
      setFormData({
        certificateType: '',
        residentId: '',
        purpose: '',
        issueDate: new Date().toISOString().split('T')[0],
        expiryDate: '',
        status: 'Pending',
        issuedBy: '',
      })
    }
  }, [certificate, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/certificates', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['certificates'])
      toast.success('Certificate created successfully')
      onClose()
    },
    onError: (error) => {
      const errorMessage = error.response?.data?.message || 
                          error.response?.data?.title ||
                          error.response?.data?.errors ? 
                            JSON.stringify(error.response.data.errors) : 
                            'Failed to create certificate'
      toast.error(errorMessage)
      console.error('Certificate creation error:', error.response?.data)
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/certificates/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['certificates'])
      toast.success('Certificate updated successfully')
      onClose()
    },
    onError: (error) => {
      const errorMessage = error.response?.data?.message || 
                          error.response?.data?.title ||
                          error.response?.data?.errors ? 
                            JSON.stringify(error.response.data.errors) : 
                            'Failed to update certificate'
      toast.error(errorMessage)
      console.error('Certificate update error:', error.response?.data)
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    
    // Validate required fields
    if (!formData.residentId || formData.residentId === '') {
      toast.error('Please select a resident')
      return
    }

    if (!formData.certificateType || formData.certificateType === '') {
      toast.error('Please select a certificate type')
      return
    }

    if (!formData.issueDate || !formData.expiryDate) {
      toast.error('Please select both issue date and expiry date')
      return
    }

    const residentIdNum = parseInt(formData.residentId)
    if (isNaN(residentIdNum)) {
      toast.error('Invalid resident selected')
      return
    }

    const issueDateObj = new Date(formData.issueDate)
    const expiryDateObj = new Date(formData.expiryDate)

    if (isNaN(issueDateObj.getTime()) || isNaN(expiryDateObj.getTime())) {
      toast.error('Invalid date format')
      return
    }

    if (expiryDateObj <= issueDateObj) {
      toast.error('Expiry date must be after issue date')
      return
    }

    // Prepare data - exclude fields that are auto-generated or not needed
    const submitData = {
      certificateType: formData.certificateType,
      residentId: residentIdNum,
      purpose: formData.purpose || null,
      issueDate: issueDateObj.toISOString(),
      expiryDate: expiryDateObj.toISOString(),
      status: formData.status,
      issuedBy: formData.issuedBy || null,
    }

    if (certificate) {
      updateMutation.mutate({ id: certificate.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{certificate ? 'Edit Certificate' : 'Issue New Certificate'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-row">
            <div className="form-group">
              <label>Certificate Type *</label>
              <select
                required
                value={formData.certificateType}
                onChange={(e) => setFormData({ ...formData, certificateType: e.target.value })}
              >
                <option value="">Select Type...</option>
                <option value="Clearance">Barangay Clearance</option>
                <option value="Indigency">Indigency Certificate</option>
                <option value="Residency">Residency Certificate</option>
                <option value="BusinessPermit">Business Permit</option>
                <option value="ID">Barangay ID</option>
              </select>
            </div>
            <div className="form-group">
              <label>Resident *</label>
              <select
                required
                value={formData.residentId}
                onChange={(e) => setFormData({ ...formData, residentId: e.target.value })}
              >
                <option value="">Select Resident...</option>
                {residents.map((resident) => (
                  <option key={resident.id} value={resident.id}>
                    {resident.firstName} {resident.lastName} - {resident.address}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-group">
            <label>Purpose</label>
            <input
              type="text"
              value={formData.purpose}
              onChange={(e) => setFormData({ ...formData, purpose: e.target.value })}
              placeholder="Purpose of the certificate"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Issue Date *</label>
              <input
                type="date"
                required
                value={formData.issueDate}
                onChange={(e) => setFormData({ ...formData, issueDate: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Expiry Date *</label>
              <input
                type="date"
                required
                value={formData.expiryDate}
                onChange={(e) => setFormData({ ...formData, expiryDate: e.target.value })}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Status *</label>
              <select
                required
                value={formData.status}
                onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              >
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Issued">Issued</option>
                <option value="Expired">Expired</option>
              </select>
            </div>
            <div className="form-group">
              <label>Issued By</label>
              <input
                type="text"
                value={formData.issuedBy}
                onChange={(e) => setFormData({ ...formData, issuedBy: e.target.value })}
                placeholder="Name of issuing officer"
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
              disabled={createMutation.isPending || updateMutation.isPending}
            >
              {createMutation.isPending || updateMutation.isPending
                ? 'Saving...'
                : certificate
                ? 'Update'
                : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

