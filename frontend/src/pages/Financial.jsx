import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { Plus, Edit, Trash2, TrendingUp, TrendingDown } from 'lucide-react'
import { toast } from 'react-hot-toast'
import BudgetModal from '../components/BudgetModal'
import './Pages.css'

export default function Financial() {
  const [showModal, setShowModal] = useState(false)
  const [editingBudget, setEditingBudget] = useState(null)
  const queryClient = useQueryClient()

  const { data: budgets = [] } = useQuery({
    queryKey: ['budgets'],
    queryFn: () => api.get('/budgets').then((res) => res.data),
    retry: false,
    refetchOnWindowFocus: false,
  })

  const deleteMutation = useMutation({
    mutationFn: (id) => api.delete(`/budgets/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries(['budgets'])
      toast.success('Budget deleted successfully')
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to delete budget')
    },
  })

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Financial & Budget Tracker</h1>
          <p>Monitor budgets, expenses, and financial transparency</p>
        </div>
        <button className="btn-primary" onClick={() => setShowModal(true)}>
          <Plus size={20} />
          New Budget
        </button>
      </div>

      {budgets.length === 0 ? (
        <div className="empty-state">
          <p>No budgets found. Click "New Budget" to create one.</p>
        </div>
      ) : (
        <div className="budget-grid">
          {budgets.map((budget) => (
            <div key={budget.id} className="budget-card">
              <div className="budget-header">
                <div>
                  <h3>{budget.budgetName}</h3>
                  <span className="badge">{budget.budgetType}</span>
                </div>
                <div className="action-buttons">
                  <button
                    className="btn-icon"
                    onClick={() => {
                      setEditingBudget(budget)
                      setShowModal(true)
                    }}
                    title="Edit Budget"
                  >
                    <Edit size={16} />
                  </button>
                  <button
                    className="btn-icon btn-danger"
                    onClick={() => {
                      if (confirm('Are you sure you want to delete this budget? This will also delete all associated expenses.')) {
                        deleteMutation.mutate(budget.id)
                      }
                    }}
                    title="Delete Budget"
                  >
                    <Trash2 size={16} />
                  </button>
                </div>
              </div>
              <div className="budget-amounts">
                <div className="amount-item">
                  <span className="amount-label">Allocated</span>
                  <span className="amount-value">
                    ₱{budget.allocatedAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                  </span>
                </div>
                <div className="amount-item">
                  <span className="amount-label">Used</span>
                  <span className="amount-value used">
                    ₱{budget.usedAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                  </span>
                </div>
                <div className="amount-item">
                  <span className="amount-label">Remaining</span>
                  <span className="amount-value remaining">
                    ₱{budget.remainingAmount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                  </span>
                </div>
              </div>
              <div className="budget-progress">
                <div className="progress-bar">
                  <div
                    className="progress-fill"
                    style={{
                      width: `${Math.min((budget.usedAmount / budget.allocatedAmount) * 100, 100)}%`,
                      backgroundColor: (budget.usedAmount / budget.allocatedAmount) > 0.9 ? 'var(--danger)' : 
                                      (budget.usedAmount / budget.allocatedAmount) > 0.7 ? 'var(--warning)' : 'var(--accent)'
                    }}
                  />
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '0.5rem', fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                  <span>Fiscal Year: {new Date(budget.fiscalYearStart).getFullYear()}</span>
                  <span>{((budget.usedAmount / budget.allocatedAmount) * 100).toFixed(1)}% used</span>
                </div>
              </div>
              {budget.description && (
                <div style={{ marginTop: '1rem', paddingTop: '1rem', borderTop: '1px solid var(--border-color)' }}>
                  <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>{budget.description}</p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      <BudgetModal
        isOpen={showModal}
        onClose={() => {
          setShowModal(false)
          setEditingBudget(null)
        }}
        budget={editingBudget}
      />
    </div>
  )
}

