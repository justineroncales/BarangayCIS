import { useState, useEffect } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function BudgetModal({ isOpen, onClose, budget = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    budgetName: '',
    budgetType: '',
    allocatedAmount: '',
    fiscalYearStart: '',
    fiscalYearEnd: '',
    description: '',
  })

  useEffect(() => {
    if (budget) {
      setFormData({
        budgetName: budget.budgetName || '',
        budgetType: budget.budgetType || '',
        allocatedAmount: budget.allocatedAmount || '',
        fiscalYearStart: budget.fiscalYearStart ? new Date(budget.fiscalYearStart).toISOString().split('T')[0] : '',
        fiscalYearEnd: budget.fiscalYearEnd ? new Date(budget.fiscalYearEnd).toISOString().split('T')[0] : '',
        description: budget.description || '',
      })
    } else {
      // Set default fiscal year (current year)
      const currentYear = new Date().getFullYear()
      setFormData({
        budgetName: '',
        budgetType: 'General Fund',
        allocatedAmount: '',
        fiscalYearStart: `${currentYear}-01-01`,
        fiscalYearEnd: `${currentYear}-12-31`,
        description: '',
      })
    }
  }, [budget, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/budgets', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['budgets'])
      toast.success('Budget created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create budget')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/budgets/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['budgets'])
      toast.success('Budget updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update budget')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()

    // Validate dates
    const startDate = new Date(formData.fiscalYearStart)
    const endDate = new Date(formData.fiscalYearEnd)
    
    if (endDate <= startDate) {
      toast.error('Fiscal year end date must be after start date')
      return
    }

    const data = {
      budgetName: formData.budgetName,
      budgetType: formData.budgetType,
      allocatedAmount: parseFloat(formData.allocatedAmount),
      fiscalYearStart: formData.fiscalYearStart,
      fiscalYearEnd: formData.fiscalYearEnd,
      description: formData.description || null,
    }

    if (budget) {
      updateMutation.mutate({ id: budget.id, data })
    } else {
      createMutation.mutate(data)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{budget ? 'Edit Budget' : 'New Budget'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label>
              Budget Name <span className="required">*</span>
            </label>
            <input
              type="text"
              value={formData.budgetName}
              onChange={(e) => setFormData({ ...formData, budgetName: e.target.value })}
              required
              placeholder="e.g., General Fund 2024"
            />
          </div>

          <div className="form-group">
            <label>
              Budget Type <span className="required">*</span>
            </label>
            <select
              value={formData.budgetType}
              onChange={(e) => setFormData({ ...formData, budgetType: e.target.value })}
              required
            >
              <option value="">Select type</option>
              <option value="General Fund">General Fund</option>
              <option value="Special Fund">Special Fund</option>
              <option value="Trust Fund">Trust Fund</option>
              <option value="Development Fund">Development Fund</option>
              <option value="Other">Other</option>
            </select>
          </div>

          <div className="form-group">
            <label>
              Allocated Amount (₱) <span className="required">*</span>
            </label>
            <input
              type="number"
              step="0.01"
              min="0.01"
              value={formData.allocatedAmount}
              onChange={(e) => setFormData({ ...formData, allocatedAmount: e.target.value })}
              required
              placeholder="0.00"
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>
                Fiscal Year Start <span className="required">*</span>
              </label>
              <input
                type="date"
                value={formData.fiscalYearStart}
                onChange={(e) => setFormData({ ...formData, fiscalYearStart: e.target.value })}
                required
              />
            </div>

            <div className="form-group">
              <label>
                Fiscal Year End <span className="required">*</span>
              </label>
              <input
                type="date"
                value={formData.fiscalYearEnd}
                onChange={(e) => setFormData({ ...formData, fiscalYearEnd: e.target.value })}
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label>Description</label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              rows="3"
              placeholder="Optional description or notes"
            />
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
                : budget
                ? 'Update Budget'
                : 'Create Budget'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

