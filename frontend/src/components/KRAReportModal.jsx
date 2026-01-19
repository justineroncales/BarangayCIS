import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function KRAReportModal({ isOpen, onClose, report = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    bhwProfileId: '',
    year: new Date().getFullYear(),
    month: new Date().getMonth() + 1,
    pillsPOP_10To14: 0,
    pillsPOP_15To19: 0,
    pillsPOP_20Plus: 0,
    pillsCOC_10To14: 0,
    pillsCOC_15To19: 0,
    pillsCOC_20Plus: 0,
    dmpa_10To14: 0,
    dmpa_15To19: 0,
    dmpa_20Plus: 0,
    condom_10To14: 0,
    condom_15To19: 0,
    condom_20Plus: 0,
    implant_10To14: 0,
    implant_15To19: 0,
    implant_20Plus: 0,
    btl_10To14: 0,
    btl_15To19: 0,
    btl_20Plus: 0,
    lam_10To14: 0,
    lam_15To19: 0,
    lam_20Plus: 0,
    iud_10To14: 0,
    iud_15To19: 0,
    iud_20Plus: 0,
    deliveries_10To14: 0,
    deliveries_15To19: 0,
    deliveries_20Plus: 0,
    teenagePregnancies: 0,
    notes: '',
  })

  const { data: bhwProfiles = [] } = useQuery({
    queryKey: ['bhw-profiles'],
    queryFn: () => api.get('/bhw-profiles', { params: { status: 'Active' } }).then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (!isOpen) return
    
    if (report) {
      setFormData({
        bhwProfileId: report.bhwProfileId || '',
        year: report.year || new Date().getFullYear(),
        month: report.month || new Date().getMonth() + 1,
        pillsPOP_10To14: report.pillsPOP_10To14 ?? 0,
        pillsPOP_15To19: report.pillsPOP_15To19 ?? 0,
        pillsPOP_20Plus: report.pillsPOP_20Plus ?? 0,
        pillsCOC_10To14: report.pillsCOC_10To14 ?? 0,
        pillsCOC_15To19: report.pillsCOC_15To19 ?? 0,
        pillsCOC_20Plus: report.pillsCOC_20Plus ?? 0,
        dmpa_10To14: report.dmpA_10To14 ?? report.dMPA_10To14 ?? 0,
        dmpa_15To19: report.dmpA_15To19 ?? report.dMPA_15To19 ?? 0,
        dmpa_20Plus: report.dmpA_20Plus ?? report.dMPA_20Plus ?? 0,
        condom_10To14: report.condom_10To14 ?? 0,
        condom_15To19: report.condom_15To19 ?? 0,
        condom_20Plus: report.condom_20Plus ?? 0,
        implant_10To14: report.implant_10To14 ?? 0,
        implant_15To19: report.implant_15To19 ?? 0,
        implant_20Plus: report.implant_20Plus ?? 0,
        btl_10To14: report.btl_10To14 ?? 0,
        btl_15To19: report.btl_15To19 ?? 0,
        btl_20Plus: report.btl_20Plus ?? 0,
        lam_10To14: report.lam_10To14 ?? 0,
        lam_15To19: report.lam_15To19 ?? 0,
        lam_20Plus: report.lam_20Plus ?? 0,
        iud_10To14: report.iud_10To14 ?? 0,
        iud_15To19: report.iud_15To19 ?? 0,
        iud_20Plus: report.iud_20Plus ?? 0,
        deliveries_10To14: report.deliveries_10To14 ?? 0,
        deliveries_15To19: report.deliveries_15To19 ?? 0,
        deliveries_20Plus: report.deliveries_20Plus ?? 0,
        teenagePregnancies: report.teenagePregnancies ?? 0,
        notes: report.notes || '',
      })
    } else {
      setFormData({
        bhwProfileId: '',
        year: new Date().getFullYear(),
        month: new Date().getMonth() + 1,
        pillsPOP_10To14: 0,
        pillsPOP_15To19: 0,
        pillsPOP_20Plus: 0,
        pillsCOC_10To14: 0,
        pillsCOC_15To19: 0,
        pillsCOC_20Plus: 0,
        dmpa_10To14: 0,
        dmpa_15To19: 0,
        dmpa_20Plus: 0,
        condom_10To14: 0,
        condom_15To19: 0,
        condom_20Plus: 0,
        implant_10To14: 0,
        implant_15To19: 0,
        implant_20Plus: 0,
        btl_10To14: 0,
        btl_15To19: 0,
        btl_20Plus: 0,
        lam_10To14: 0,
        lam_15To19: 0,
        lam_20Plus: 0,
        iud_10To14: 0,
        iud_15To19: 0,
        iud_20Plus: 0,
        deliveries_10To14: 0,
        deliveries_15To19: 0,
        deliveries_20Plus: 0,
        teenagePregnancies: 0,
        notes: '',
      })
    }
  }, [report, isOpen])

  const mutation = useMutation({
    mutationFn: (data) => {
      if (!data.bhwProfileId || data.bhwProfileId === '') {
        throw new Error('BHW Profile is required')
      }
      
      const payload = {
        bhwProfileId: typeof data.bhwProfileId === 'string' ? parseInt(data.bhwProfileId) : data.bhwProfileId,
        year: parseInt(data.year) || new Date().getFullYear(),
        month: parseInt(data.month) || new Date().getMonth() + 1,
        pillsPOP_10To14: parseInt(data.pillsPOP_10To14) || 0,
        pillsPOP_15To19: parseInt(data.pillsPOP_15To19) || 0,
        pillsPOP_20Plus: parseInt(data.pillsPOP_20Plus) || 0,
        pillsCOC_10To14: parseInt(data.pillsCOC_10To14) || 0,
        pillsCOC_15To19: parseInt(data.pillsCOC_15To19) || 0,
        pillsCOC_20Plus: parseInt(data.pillsCOC_20Plus) || 0,
        dmpA_10To14: parseInt(data.dmpa_10To14) || 0,
        dmpA_15To19: parseInt(data.dmpa_15To19) || 0,
        dmpA_20Plus: parseInt(data.dmpa_20Plus) || 0,
        condom_10To14: parseInt(data.condom_10To14) || 0,
        condom_15To19: parseInt(data.condom_15To19) || 0,
        condom_20Plus: parseInt(data.condom_20Plus) || 0,
        implant_10To14: parseInt(data.implant_10To14) || 0,
        implant_15To19: parseInt(data.implant_15To19) || 0,
        implant_20Plus: parseInt(data.implant_20Plus) || 0,
        btl_10To14: parseInt(data.btl_10To14) || 0,
        btl_15To19: parseInt(data.btl_15To19) || 0,
        btl_20Plus: parseInt(data.btl_20Plus) || 0,
        lam_10To14: parseInt(data.lam_10To14) || 0,
        lam_15To19: parseInt(data.lam_15To19) || 0,
        lam_20Plus: parseInt(data.lam_20Plus) || 0,
        iud_10To14: parseInt(data.iud_10To14) || 0,
        iud_15To19: parseInt(data.iud_15To19) || 0,
        iud_20Plus: parseInt(data.iud_20Plus) || 0,
        deliveries_10To14: parseInt(data.deliveries_10To14) || 0,
        deliveries_15To19: parseInt(data.deliveries_15To19) || 0,
        deliveries_20Plus: parseInt(data.deliveries_20Plus) || 0,
        teenagePregnancies: parseInt(data.teenagePregnancies) || 0,
        notes: data.notes || '',
      }
      if (report) {
        return api.put(`/bhw-reports/kra/${report.id}`, payload)
      }
      return api.post('/bhw-reports/kra', payload)
    },
    onSuccess: () => {
      queryClient.invalidateQueries(['kra-reports'])
      toast.success(report ? 'KRA Report updated successfully' : 'KRA Report created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to save KRA report')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    mutation.mutate(formData)
  }

  const handleNumberChange = (field, value) => {
    setFormData({ ...formData, [field]: value === '' ? 0 : parseInt(value) || 0 })
  }

  if (!isOpen) return null

  const methods = [
    { label: 'PILLS-POP', fields: ['pillsPOP_10To14', 'pillsPOP_15To19', 'pillsPOP_20Plus'] },
    { label: 'PILLS-COC', fields: ['pillsCOC_10To14', 'pillsCOC_15To19', 'pillsCOC_20Plus'] },
    { label: 'DMPA', fields: ['dmpa_10To14', 'dmpa_15To19', 'dmpa_20Plus'] },
    { label: 'CONDOM', fields: ['condom_10To14', 'condom_15To19', 'condom_20Plus'] },
    { label: 'IMPLANT', fields: ['implant_10To14', 'implant_15To19', 'implant_20Plus'] },
    { label: 'BTL', fields: ['btl_10To14', 'btl_15To19', 'btl_20Plus'] },
    { label: 'LAM', fields: ['lam_10To14', 'lam_15To19', 'lam_20Plus'] },
    { label: 'IUD', fields: ['iud_10To14', 'iud_15To19', 'iud_20Plus'] },
  ]

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-extra-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{report ? 'Edit KRA Report' : 'Add KRA Report'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-row three-columns">
            <div className="form-group">
              <label>BHW Profile *</label>
              <select
                value={formData.bhwProfileId}
                onChange={(e) => setFormData({ ...formData, bhwProfileId: parseInt(e.target.value) })}
                required
              >
                <option value="">Select BHW...</option>
                {bhwProfiles.map((bhw) => (
                  <option key={bhw.id} value={bhw.id}>
                    {bhw.firstName} {bhw.lastName} ({bhw.bhwNumber})
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Year *</label>
              <input
                type="number"
                value={formData.year}
                onChange={(e) => setFormData({ ...formData, year: parseInt(e.target.value) || new Date().getFullYear() })}
                min="2020"
                max="2100"
                required
              />
            </div>
            <div className="form-group">
              <label>Month *</label>
              <select
                value={formData.month}
                onChange={(e) => setFormData({ ...formData, month: parseInt(e.target.value) })}
                required
              >
                <option value="1">January</option>
                <option value="2">February</option>
                <option value="3">March</option>
                <option value="4">April</option>
                <option value="5">May</option>
                <option value="6">June</option>
                <option value="7">July</option>
                <option value="8">August</option>
                <option value="9">September</option>
                <option value="10">October</option>
                <option value="11">November</option>
                <option value="12">December</option>
              </select>
            </div>
          </div>

          <div style={{ marginTop: '2rem', marginBottom: '1.5rem' }}>
            <h3 style={{ marginBottom: '1rem', fontSize: '1.1rem', fontWeight: 600, color: 'var(--text-primary)' }}>Family Planning Methods</h3>
            <div className="table-container-wrapper">
              <div className="table-container">
                <table className="data-table" style={{ fontSize: '0.875rem' }}>
                  <thead>
                    <tr>
                      <th style={{ textAlign: 'left' }}>Method</th>
                      <th style={{ textAlign: 'center' }}>10-14 y.o.</th>
                      <th style={{ textAlign: 'center' }}>15-19 y.o.</th>
                      <th style={{ textAlign: 'center' }}>20+ y.o.</th>
                    </tr>
                  </thead>
                  <tbody>
                    {methods.map((method) => (
                      <tr key={method.label}>
                        <td style={{ fontWeight: 500 }}>{method.label}</td>
                        {method.fields.map((field) => (
                          <td key={field}>
                            <input
                              type="number"
                              min="0"
                              value={formData[field]}
                              onChange={(e) => handleNumberChange(field, e.target.value)}
                              style={{ width: '100%', padding: '0.5rem', border: '1px solid var(--border-color)', borderRadius: '6px', background: 'var(--bg-primary)', fontSize: '0.875rem' }}
                            />
                          </td>
                        ))}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          <div style={{ marginTop: '2rem', marginBottom: '1.5rem' }}>
            <h3 style={{ marginBottom: '1rem', fontSize: '1.1rem', fontWeight: 600, color: 'var(--text-primary)' }}>Deliveries</h3>
            <div className="form-row three-columns">
              <div className="form-group">
                <label>10-14 y.o.</label>
                <input
                  type="number"
                  min="0"
                  value={formData.deliveries_10To14}
                  onChange={(e) => handleNumberChange('deliveries_10To14', e.target.value)}
                />
              </div>
              <div className="form-group">
                <label>15-19 y.o.</label>
                <input
                  type="number"
                  min="0"
                  value={formData.deliveries_15To19}
                  onChange={(e) => handleNumberChange('deliveries_15To19', e.target.value)}
                />
              </div>
              <div className="form-group">
                <label>20+ y.o.</label>
                <input
                  type="number"
                  min="0"
                  value={formData.deliveries_20Plus}
                  onChange={(e) => handleNumberChange('deliveries_20Plus', e.target.value)}
                />
              </div>
            </div>
          </div>

          <div className="form-group">
            <label>Teenage Pregnancies (Total)</label>
            <input
              type="number"
              min="0"
              value={formData.teenagePregnancies}
              onChange={(e) => handleNumberChange('teenagePregnancies', e.target.value)}
            />
          </div>

          <div className="form-group">
            <label>Notes</label>
            <textarea
              value={formData.notes}
              onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
              rows="3"
            />
          </div>

          <div className="modal-actions">
            <button type="button" onClick={onClose} className="btn-secondary">
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={mutation.isPending}>
              {mutation.isPending ? 'Saving...' : report ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

