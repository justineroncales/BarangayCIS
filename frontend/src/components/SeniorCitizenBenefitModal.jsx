import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function SeniorCitizenBenefitModal({ isOpen, onClose, benefit = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    seniorCitizenIDId: '',
    benefitType: '',
    benefitDescription: '',
    amount: '',
    benefitDate: '',
    status: 'Pending',
    requirements: '',
    notes: '',
    processedBy: '',
    processedDate: '',
    referenceNumber: '',
    paymentMethod: '',
  })

  const { data: seniorIds = [] } = useQuery({
    queryKey: ['senior-citizen-ids'],
    queryFn: () => api.get('/senior-citizen-ids').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (benefit) {
      setFormData({
        seniorCitizenIDId: benefit.seniorCitizenIDId || '',
        benefitType: benefit.benefitType || '',
        benefitDescription: benefit.benefitDescription || '',
        amount: benefit.amount || '',
        benefitDate: benefit.benefitDate ? new Date(benefit.benefitDate).toISOString().split('T')[0] : '',
        status: benefit.status || 'Pending',
        requirements: benefit.requirements || '',
        notes: benefit.notes || '',
        processedBy: benefit.processedBy || '',
        processedDate: benefit.processedDate ? new Date(benefit.processedDate).toISOString().split('T')[0] : '',
        referenceNumber: benefit.referenceNumber || '',
        paymentMethod: benefit.paymentMethod || '',
      })
    } else {
      setFormData({
        seniorCitizenIDId: '',
        benefitType: '',
        benefitDescription: '',
        amount: '',
        benefitDate: new Date().toISOString().split('T')[0],
        status: 'Pending',
        requirements: '',
        notes: '',
        processedBy: '',
        processedDate: '',
        referenceNumber: '',
        paymentMethod: '',
      })
    }
  }, [benefit, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/senior-citizen-benefits', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['senior-citizen-benefits'])
      toast.success('Benefit record created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create benefit record')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/senior-citizen-benefits/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['senior-citizen-benefits'])
      toast.success('Benefit record updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update benefit record')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const submitData = {
      ...formData,
      seniorCitizenIDId: parseInt(formData.seniorCitizenIDId),
      amount: formData.amount ? parseFloat(formData.amount) : null,
      benefitDate: new Date(formData.benefitDate),
      processedDate: formData.processedDate ? new Date(formData.processedDate) : null,
    }

    if (benefit) {
      updateMutation.mutate({ id: benefit.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{benefit ? 'Edit Benefit' : 'Add New Benefit'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label >Senior Citizen *</label>
            <select
              required
              value={formData.seniorCitizenIDId}
              onChange={(e) => setFormData({ ...formData, seniorCitizenIDId: e.target.value })}
            >
              <option value="">Select Senior Citizen...</option>
              {seniorIds.map((sc) => (
                <option key={sc.id} value={sc.id}>
                  {sc.seniorCitizenNumber} - {sc.resident?.firstName} {sc.resident?.lastName}
                </option>
              ))}
            </select>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label >Benefit Type *</label>
              <select
                required
                value={formData.benefitType}
                onChange={(e) => setFormData({ ...formData, benefitType: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="Social Pension">Social Pension</option>
                <option value="Discount">Discount</option>
                <option value="Medical Assistance">Medical Assistance</option>
                <option value="Burial Assistance">Burial Assistance</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div className="form-group">
              <label>Amount (₱)</label>
              <input
                type="number"
                step="0.01"
                min="0"
                value={formData.amount}
                onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
              />
            </div>
          </div>

          <div className="form-group">
            <label>Benefit Description</label>
            <textarea
              rows="2"
              value={formData.benefitDescription}
              onChange={(e) => setFormData({ ...formData, benefitDescription: e.target.value })}
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label >Benefit Date *</label>
              <input
                type="date"
                required
                value={formData.benefitDate}
                onChange={(e) => setFormData({ ...formData, benefitDate: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label >Status *</label>
              <select
                required
                value={formData.status}
                onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              >
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Disbursed">Disbursed</option>
                <option value="Denied">Denied</option>
              </select>
            </div>
          </div>

          {formData.status === 'Disbursed' && (
            <>
              <div className="form-row">
                <div className="form-group">
                  <label>Processed Date</label>
                  <input
                    type="date"
                    value={formData.processedDate}
                    onChange={(e) => setFormData({ ...formData, processedDate: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label>Payment Method</label>
                  <select
                    value={formData.paymentMethod}
                    onChange={(e) => setFormData({ ...formData, paymentMethod: e.target.value })}
                  >
                    <option value="">Select...</option>
                    <option value="Cash">Cash</option>
                    <option value="Bank Transfer">Bank Transfer</option>
                    <option value="Check">Check</option>
                  </select>
                </div>
              </div>
              <div className="form-group">
                <label>Reference Number</label>
                <input
                  type="text"
                  value={formData.referenceNumber}
                  onChange={(e) => setFormData({ ...formData, referenceNumber: e.target.value })}
                  placeholder="Transaction reference, check number"
                />
              </div>
            </>
          )}

          <div className="form-group">
            <label>Requirements</label>
            <textarea
              rows="3"
              value={formData.requirements}
              onChange={(e) => setFormData({ ...formData, requirements: e.target.value })}
              placeholder="Requirements for the benefit"
            />
          </div>

          <div className="form-group">
            <label>Processed By</label>
            <input
              type="text"
              value={formData.processedBy}
              onChange={(e) => setFormData({ ...formData, processedBy: e.target.value })}
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
              {createMutation.isPending || updateMutation.isPending ? 'Saving...' : benefit ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

