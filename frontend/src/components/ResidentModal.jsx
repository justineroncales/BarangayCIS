import { useState, useEffect } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import api from '../services/api'
import { X } from 'lucide-react'
import { toast } from 'react-hot-toast'

export default function ResidentModal({ isOpen, onClose, resident = null }) {
  const queryClient = useQueryClient()
  const [formData, setFormData] = useState({
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
    occupation: '',
    employmentStatus: '',
    isVoter: false,
    voterId: '',
    householdId: null,
    bhwProfileId: null,
    relationshipToHead: '',
    educationalAttainment: '',
    bloodType: '',
    isPWD: false,
    isSenior: false,
    notes: '',
  })

  const { data: households = [] } = useQuery({
    queryKey: ['households'],
    queryFn: () => api.get('/households').then((res) => res.data),
    enabled: isOpen,
  })

  const { data: bhwProfiles = [] } = useQuery({
    queryKey: ['bhw-profiles'],
    queryFn: () => api.get('/bhw-profiles?status=Active').then((res) => res.data),
    enabled: isOpen,
  })

  useEffect(() => {
    if (resident) {
      setFormData({
        firstName: resident.firstName || '',
        lastName: resident.lastName || '',
        middleName: resident.middleName || '',
        suffix: resident.suffix || '',
        dateOfBirth: resident.dateOfBirth ? new Date(resident.dateOfBirth).toISOString().split('T')[0] : '',
        gender: resident.gender || '',
        address: resident.address || '',
        contactNumber: resident.contactNumber || '',
        email: resident.email || '',
        civilStatus: resident.civilStatus || '',
        occupation: resident.occupation || '',
        employmentStatus: resident.employmentStatus || '',
        isVoter: resident.isVoter || false,
        voterId: resident.voterId || '',
        householdId: resident.householdId || null,
        bhwProfileId: resident.bhwProfileId || null,
        relationshipToHead: resident.relationshipToHead || '',
        educationalAttainment: resident.educationalAttainment || '',
        bloodType: resident.bloodType || '',
        isPWD: resident.isPWD || false,
        isSenior: resident.isSenior || false,
        notes: resident.notes || '',
      })
    } else {
      // Reset form for new resident
      setFormData({
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
        occupation: '',
        employmentStatus: '',
        isVoter: false,
        voterId: '',
        householdId: null,
        bhwProfileId: null,
        relationshipToHead: '',
        educationalAttainment: '',
        bloodType: '',
        isPWD: false,
        isSenior: false,
        notes: '',
      })
    }
  }, [resident, isOpen])

  const createMutation = useMutation({
    mutationFn: (data) => api.post('/residents', data),
    onSuccess: () => {
      queryClient.invalidateQueries(['residents'])
      toast.success('Resident created successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to create resident')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }) => api.put(`/residents/${id}`, data),
    onSuccess: () => {
      queryClient.invalidateQueries(['residents'])
      toast.success('Resident updated successfully')
      onClose()
    },
    onError: (error) => {
      toast.error(error.response?.data?.message || 'Failed to update resident')
    },
  })

  const handleSubmit = (e) => {
    e.preventDefault()
    const submitData = {
      ...formData,
      dateOfBirth: new Date(formData.dateOfBirth),
      householdId: formData.householdId || null,
      bhwProfileId: formData.bhwProfileId || null,
    }

    if (resident) {
      updateMutation.mutate({ id: resident.id, data: submitData })
    } else {
      createMutation.mutate(submitData)
    }
  }

  if (!isOpen) return null

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{resident ? 'Edit Resident' : 'Add New Resident'}</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
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
              <label>Date of Birth *</label>
              <input
                type="date"
                required
                value={formData.dateOfBirth}
                onChange={(e) => setFormData({ ...formData, dateOfBirth: e.target.value })}
              />
            </div>
            <div className="form-group">
              <label>Gender *</label>
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
            <label>Address *</label>
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
              <label>Occupation</label>
              <input
                type="text"
                value={formData.occupation}
                onChange={(e) => setFormData({ ...formData, occupation: e.target.value })}
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Employment Status</label>
              <select
                value={formData.employmentStatus}
                onChange={(e) => setFormData({ ...formData, employmentStatus: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="Employed">Employed</option>
                <option value="Unemployed">Unemployed</option>
                <option value="Self-Employed">Self-Employed</option>
                <option value="Student">Student</option>
                <option value="Retired">Retired</option>
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
              <label>Household</label>
              <select
                value={formData.householdId || ''}
                onChange={(e) => setFormData({ ...formData, householdId: e.target.value ? parseInt(e.target.value) : null })}
              >
                <option value="">None</option>
                {households.map((h) => (
                  <option key={h.id} value={h.id}>
                    {h.householdNumber} - {h.address}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-group">
              <label>Relationship to Head</label>
              <select
                value={formData.relationshipToHead}
                onChange={(e) => setFormData({ ...formData, relationshipToHead: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="Head">Head of Household</option>
                <option value="Spouse">Spouse</option>
                <option value="Child">Child</option>
                <option value="Parent">Parent</option>
                <option value="Sibling">Sibling</option>
                <option value="Other Relative">Other Relative</option>
                <option value="Non-Relative">Non-Relative</option>
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Assigned BHW (Barangay Health Worker)</label>
              <select
                value={formData.bhwProfileId || ''}
                onChange={(e) => setFormData({ ...formData, bhwProfileId: e.target.value ? parseInt(e.target.value) : null })}
              >
                <option value="">Select BHW...</option>
                {bhwProfiles.map((bhw) => (
                  <option key={bhw.id} value={bhw.id}>
                    {bhw.bhwNumber} - {bhw.firstName} {bhw.lastName}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Blood Type</label>
              <select
                value={formData.bloodType}
                onChange={(e) => setFormData({ ...formData, bloodType: e.target.value })}
              >
                <option value="">Select...</option>
                <option value="A+">A+</option>
                <option value="A-">A-</option>
                <option value="B+">B+</option>
                <option value="B-">B-</option>
                <option value="AB+">AB+</option>
                <option value="AB-">AB-</option>
                <option value="O+">O+</option>
                <option value="O-">O-</option>
                <option value="Unknown">Unknown</option>
              </select>
            </div>
            <div className="form-group">
              <label>Age</label>
              <input
                type="text"
                value={formData.dateOfBirth ? Math.floor((new Date() - new Date(formData.dateOfBirth)) / (365.25 * 24 * 60 * 60 * 1000)) : ''}
                disabled
                style={{ background: 'var(--bg-primary)', cursor: 'not-allowed' }}
                placeholder="Calculated from date of birth"
              />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>
                <input
                  type="checkbox"
                  checked={formData.isVoter}
                  onChange={(e) => setFormData({ ...formData, isVoter: e.target.checked })}
                />
                Is Voter
              </label>
              {formData.isVoter && (
                <input
                  type="text"
                  placeholder="Voter ID"
                  value={formData.voterId}
                  onChange={(e) => setFormData({ ...formData, voterId: e.target.value })}
                  style={{ marginTop: '0.5rem' }}
                />
              )}
            </div>
            <div className="form-group">
              <label>
                <input
                  type="checkbox"
                  checked={formData.isPWD}
                  onChange={(e) => setFormData({ ...formData, isPWD: e.target.checked })}
                />
                Person with Disability
              </label>
            </div>
            <div className="form-group">
              <label>
                <input
                  type="checkbox"
                  checked={formData.isSenior}
                  onChange={(e) => setFormData({ ...formData, isSenior: e.target.checked })}
                />
                Senior Citizen
              </label>
            </div>
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
                : resident
                ? 'Update'
                : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

