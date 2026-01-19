import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function DeliveryModal({ isOpen, onClose, delivery = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    bhwProfileId: '',
    motherName: '',
    childName: '',
    purokSitio: '',
    gender: '',
    dateOfBirth: '',
    timeOfBirth: '',
    weight: '',
    height: '',
    placeOfBirth: '',
    deliveryType: '',
    bcgAndHepaB: '',
    attendedBy: '',
  })

  const { data: bhwProfiles = [] } = useQuery({
    queryKey: ['bhw-profiles'],
    queryFn: () => api.get('/bhw-profiles', { params: { status: 'Active' } }).then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (!isOpen) return
    
    if (delivery) {
      setFormData({
        bhwProfileId: delivery.bhwProfileId || '',
        motherName: delivery.motherName || '',
        childName: delivery.childName || '',
        purokSitio: delivery.purokSitio || '',
        gender: delivery.gender || '',
        dateOfBirth: delivery.dateOfBirth ? new Date(delivery.dateOfBirth).toISOString().split('T')[0] : '',
        timeOfBirth: delivery.timeOfBirth || '',
        weight: delivery.weight || '',
        height: delivery.height || '',
        placeOfBirth: delivery.placeOfBirth || '',
        deliveryType: delivery.deliveryType || '',
        bcgAndHepaB: delivery.bcgAndHepaB || '',
        attendedBy: delivery.attendedBy || '',
      })
    } else {
      setFormData({
        bhwProfileId: '',
        motherName: '',
        childName: '',
        purokSitio: '',
        gender: '',
        dateOfBirth: '',
        timeOfBirth: '',
        weight: '',
        height: '',
        placeOfBirth: '',
        deliveryType: '',
        bcgAndHepaB: '',
        attendedBy: '',
      })
    }
  }, [delivery, isOpen])

  const mutation = useMutation({
    mutationFn: (data) => {
      // Validate bhwProfileId first - must be a valid number > 0
      let bhwProfileId = 0
      if (typeof data.bhwProfileId === 'string') {
        if (data.bhwProfileId === '' || data.bhwProfileId === '0') {
          throw new Error('Please select a BHW Profile')
        }
        bhwProfileId = parseInt(data.bhwProfileId, 10)
      } else if (typeof data.bhwProfileId === 'number') {
        bhwProfileId = data.bhwProfileId
      }
      
      if (!bhwProfileId || bhwProfileId <= 0 || isNaN(bhwProfileId)) {
        throw new Error('Please select a BHW Profile')
      }
      
      if (!data.motherName || !data.childName || !data.gender || !data.dateOfBirth) {
        throw new Error('Please fill in all required fields')
      }
      
      // Ensure date is valid
      const dateOfBirth = new Date(data.dateOfBirth)
      if (isNaN(dateOfBirth.getTime())) {
        throw new Error('Invalid date of birth')
      }
      
      const payload = {
        bhwProfileId: bhwProfileId,
        motherName: data.motherName.trim(),
        childName: data.childName.trim(),
        purokSitio: data.purokSitio?.trim() || null,
        gender: data.gender.trim(),
        dateOfBirth: dateOfBirth.toISOString(),
        timeOfBirth: data.timeOfBirth?.trim() || null,
        weight: data.weight?.trim() || null,
        height: data.height?.trim() || null,
        placeOfBirth: data.placeOfBirth?.trim() || null,
        deliveryType: data.deliveryType || null,
        bcgAndHepaB: data.bcgAndHepaB?.trim() || null,
        attendedBy: data.attendedBy?.trim() || null,
      }
      
      console.log('Sending delivery payload:', JSON.stringify(payload, null, 2))
      
      if (delivery) {
        return api.put(`/bhw-reports/deliveries/${delivery.id}`, payload)
      }
      return api.post('/bhw-reports/deliveries', payload)
    },
    onSuccess: () => {
      queryClient.invalidateQueries(['deliveries'])
      toast.success(delivery ? 'Delivery updated successfully' : 'Delivery created successfully')
      onClose()
    },
    onError: (error) => {
      console.error('Delivery save error:', error.response?.data || error)
      const errorData = error.response?.data
      const errorMessage = errorData?.message || errorData?.title || error.message || 'Failed to save delivery'
      
      // Handle ASP.NET Core validation errors (dictionary format: { field: [messages] })
      if (errorData?.errors && typeof errorData.errors === 'object' && !Array.isArray(errorData.errors)) {
        const validationErrors = Object.entries(errorData.errors)
          .map(([field, messages]) => {
            const msgArray = Array.isArray(messages) ? messages : [messages]
            // Convert camelCase field names to more readable format
            const fieldName = field.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase())
            return `${fieldName}: ${msgArray.join(', ')}`
          })
          .join('; ')
        toast.error(`Validation Error: ${validationErrors}`, { duration: 6000 })
        console.error('Validation errors:', errorData.errors)
      } else if (errorData?.errors && Array.isArray(errorData.errors)) {
        const errorDetails = errorData.errors.map(e => `${e.field}: ${e.message}`).join(', ')
        toast.error(`${errorMessage}: ${errorDetails}`, { duration: 6000 })
      } else {
        toast.error(errorMessage)
      }
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    mutation.mutate(formData)
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{delivery ? 'Edit Delivery' : 'Add Delivery'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-row">
            <div className="form-group">
              <label>BHW Profile *</label>
              <select
                value={formData.bhwProfileId || ''}
                onChange={(e) => setFormData({ ...formData, bhwProfileId: e.target.value ? parseInt(e.target.value) : '' })}
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
              <label>Gender *</label>
              <select
                value={formData.gender}
                onChange={(e) => setFormData({ ...formData, gender: e.target.value })}
                required
              >
                <option value="">Select...</option>
                <option value="M">Male</option>
                <option value="F">Female</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Mother Name *</label>
              <input
                type="text"
                value={formData.motherName}
                onChange={(e) => setFormData({ ...formData, motherName: e.target.value })}
                required
              />
            </div>
            <div className="form-group">
              <label>Child Name *</label>
              <input
                type="text"
                value={formData.childName}
                onChange={(e) => setFormData({ ...formData, childName: e.target.value })}
                required
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Purok/Sitio</label>
              <input
                type="text"
                value={formData.purokSitio}
                onChange={(e) => setFormData({ ...formData, purokSitio: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Date of Birth *</label>
              <input
                type="date"
                value={formData.dateOfBirth}
                onChange={(e) => setFormData({ ...formData, dateOfBirth: e.target.value })}
                required
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Time of Birth</label>
              <input
                type="text"
                value={formData.timeOfBirth}
                onChange={(e) => setFormData({ ...formData, timeOfBirth: e.target.value })}
                placeholder="e.g., 2:13 AM"
              />
            </div>
            <div className="form-group">
              <label>Weight</label>
              <input
                type="text"
                value={formData.weight}
                onChange={(e) => setFormData({ ...formData, weight: e.target.value })}
                placeholder="e.g., 2.84 kg"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Height</label>
              <input
                type="text"
                value={formData.height}
                onChange={(e) => setFormData({ ...formData, height: e.target.value })}
                placeholder="e.g., 51 CM"
              />
            </div>
            <div className="form-group">
              <label>Place of Birth</label>
              <input
                type="text"
                value={formData.placeOfBirth}
                onChange={(e) => setFormData({ ...formData, placeOfBirth: e.target.value })}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Delivery Type</label>
              <select
                value={formData.deliveryType}
                onChange={(e) => setFormData({ ...formData, deliveryType: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="CS">CS (Cesarean Section)</option>
                <option value="NSD">NSD (Normal Spontaneous Delivery)</option>
              </select>
            </div>
            <div className="form-group">
              <label>BCG & HEPA B</label>
              <input
                type="text"
                value={formData.bcgAndHepaB}
                onChange={(e) => setFormData({ ...formData, bcgAndHepaB: e.target.value })}
                placeholder="e.g., HEPA B 7-2-2025 BCG-7-2-28"
              />
            </div>
          </div>

          <div className="form-group">
            <label>Attended By</label>
            <input
              type="text"
              value={formData.attendedBy}
              onChange={(e) => setFormData({ ...formData, attendedBy: e.target.value })}
              placeholder="e.g., ORB SAUNAR AGE: 19 YRS DL G2 P1"
            />
          </div>

          <div className="modal-actions">
            <button type="button" onClick={onClose} className="btn-secondary">
              Cancel
            </button>
            <button type="submit" className="btn-primary" disabled={mutation.isPending}>
              {mutation.isPending ? 'Saving...' : delivery ? 'Update' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

