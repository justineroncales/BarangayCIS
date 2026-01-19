import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function BHWProfileModal({ isOpen, onClose, bhwProfile = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
    residentId: null,
    firstName: '',
    lastName: '',
    middleName: '',
    suffix: '',
    dateOfBirth: '',
    gender: '',
    address: '',
    contactNumber: '',
    email: '',
    civilStatus: '',
    educationalAttainment: '',
    dateAppointed: '',
    status: 'Active',
    specialization: '',
    notes: '',
  })

  const { data: residents = [] } = useQuery({
    queryKey: ['residents'],
    queryFn: () => api.get('/residents').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (bhwProfile) {
      setFormData({
        residentId: bhwProfile.residentId || null,
        firstName: bhwProfile.firstName || '',
        lastName: bhwProfile.lastName || '',
        middleName: bhwProfile.middleName || '',
        suffix: bhwProfile.suffix || '',
        dateOfBirth: bhwProfile.dateOfBirth ? new Date(bhwProfile.dateOfBirth).toISOString().split('T')[0] : '',
        gender: bhwProfile.gender || '',
        address: bhwProfile.address || '',
        contactNumber: bhwProfile.contactNumber || '',
        email: bhwProfile.email || '',
        civilStatus: bhwProfile.civilStatus || '',
        educationalAttainment: bhwProfile.educationalAttainment || '',
        dateAppointed: bhwProfile.dateAppointed ? new Date(bhwProfile.dateAppointed).toISOString().split('T')[0] : '',
        status: bhwProfile.status || 'Active',
        specialization: bhwProfile.specialization || '',
        notes: bhwProfile.notes || '',
      })
    } else {
      setFormData({
        residentId: null,
        firstName: '',
        lastName: '',
        middleName: '',
        suffix: '',
        dateOfBirth: '',
        gender: '',
        address: '',
        contactNumber: '',
        email: '',
        civilStatus: '',
        educationalAttainment: '',
        dateAppointed: new Date().toISOString().split('T')[0],
        status: 'Active',
        specialization: '',
        notes: '',
      })
    }
  }, [bhwProfile, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/bhw-profiles', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['bhw-profiles'])
      toast.success('BHW Profile created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create BHW Profile')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/bhw-profiles/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['bhw-profiles'])
      toast.success('BHW Profile updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update BHW Profile')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const submitData = {
      ...formData,
      dateOfBirth: new Date(formData.dateOfBirth),
      dateAppointed: new Date(formData.dateAppointed),
      residentId: formData.residentId || null,
    }

    if (bhwProfile) {
      updateMutation.mutate({ id: bhwProfile.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{bhwProfile ? 'Edit BHW Profile' : 'Add New BHW Profile'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label>Link to Resident (Optional)</label>
            <select
              value={formData.residentId || ''}
              onChange={(e) => setFormData({ ...formData, residentId: e.target.value ? parseInt(e.target.value) : null })}
            >
              <option value="">None - Create New BHW</option>
              {residents.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.firstName} {r.lastName} - {r.address}
                </option>
              ))}
            </select>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>First Name *</label>
              <input
                type="text"
                required
                value={formData.firstName}
                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Last Name *</label>
              <input
                type="text"
                required
                value={formData.lastName}
                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Middle Name</label>
              <input
                type="text"
                value={formData.middleName}
                onChange={(e) => setFormData({ ...formData, middleName: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Suffix</label>
              <input
                type="text"
                value={formData.suffix}
                onChange={(e) => setFormData({ ...formData, suffix: e.target.value })}
                placeholder="Jr., Sr., III, etc."
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label >Date of Birth</label>
              <input
                type="date"
                required
                value={formData.dateOfBirth}
                onChange={(e) => setFormData({ ...formData, dateOfBirth: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label >Gender</label>
              <select
                required
                value={formData.gender}
                onChange={(e) => setFormData({ ...formData, gender: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="Male">Male</option>
                <option value="Female">Female</option>
                <option value="Other">Other</option>
              </select>
            </div>
          </div>

          <div className="form-group">
            <label >Address</label>
            <input
              type="text"
              required
              value={formData.address}
              onChange={(e) => setFormData({ ...formData, address: e.target.value })}
            />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Contact Number</label>
              <input
                type="tel"
                value={formData.contactNumber}
                onChange={(e) => setFormData({ ...formData, contactNumber: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Email</label>
              <input
                type="email"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Civil Status</label>
              <select
                value={formData.civilStatus}
                onChange={(e) => setFormData({ ...formData, civilStatus: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="Single">Single</option>
                <option value="Married">Married</option>
                <option value="Divorced">Divorced</option>
                <option value="Widowed">Widowed</option>
              </select>
            </div>
            <div className="form-group">
              <label>Educational Attainment</label>
              <select
                value={formData.educationalAttainment}
                onChange={(e) => setFormData({ ...formData, educationalAttainment: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="Elementary">Elementary</option>
                <option value="High School">High School</option>
                <option value="Senior High School">Senior High School</option>
                <option value="Vocational">Vocational</option>
                <option value="College">College</option>
                <option value="Graduate">Graduate</option>
                <option value="Post Graduate">Post Graduate</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label >Date Appointed</label>
              <input
                type="date"
                required
                value={formData.dateAppointed}
                onChange={(e) => setFormData({ ...formData, dateAppointed: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label >Status</label>
              <select
                required
                value={formData.status}
                onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              >
                <option value="Active">Active</option>
                <option value="Inactive">Inactive</option>
                <option value="Resigned">Resigned</option>
                <option value="Terminated">Terminated</option>
              </select>
            </div>
          </div>

          <div className="form-group">
            <label>Specialization</label>
            <input
              type="text"
              value={formData.specialization}
              onChange={(e) => setFormData({ ...formData, specialization: e.target.value })}
              placeholder="e.g., Maternal Health, Child Health, etc."
            />
          </div>

          <div className="form-group">
            <label>Notes</label>
            <textarea
              rows="3"
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
              {createMutation.isPending || updateMutation.isPending
                ? 'Saving...'
                : bhwProfile
                ? 'Update'
                : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

