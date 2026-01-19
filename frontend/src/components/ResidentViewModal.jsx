import { X } from 'lucide-react'

export default function ResidentViewModal({ isOpen, onClose, resident }) {
  if (!isOpen || !resident) return null

  const age = Math.floor((new Date() - new Date(resident.dateOfBirth)) / (365.25 * 24 * 60 * 60 * 1000))

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Resident Details</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <div className="resident-details">
          <div className="details-section">
            <h3>Personal Information</h3>
            <div className="details-grid">
              <div className="detail-item">
                <label>Full Name</label>
                <div>{resident.firstName} {resident.middleName || ''} {resident.lastName} {resident.suffix || ''}</div>
              </div>
              <div className="detail-item">
                <label>Date of Birth</label>
                <div>{new Date(resident.dateOfBirth).toLocaleDateString()}</div>
              </div>
              <div className="detail-item">
                <label>Age</label>
                <div>{age} years old</div>
              </div>
              <div className="detail-item">
                <label>Gender</label>
                <div>{resident.gender}</div>
              </div>
              <div className="detail-item">
                <label>Civil Status</label>
                <div>{resident.civilStatus || '-'}</div>
              </div>
              <div className="detail-item">
                <label>Blood Type</label>
                <div>{resident.bloodType || '-'}</div>
              </div>
            </div>
          </div>

          <div className="details-section">
            <h3>Contact Information</h3>
            <div className="details-grid">
              <div className="detail-item">
                <label>Address</label>
                <div>{resident.address}</div>
              </div>
              <div className="detail-item">
                <label>Contact Number</label>
                <div>{resident.contactNumber || '-'}</div>
              </div>
              <div className="detail-item">
                <label>Email</label>
                <div>{resident.email || '-'}</div>
              </div>
            </div>
          </div>

          <div className="details-section">
            <h3>Employment & Education</h3>
            <div className="details-grid">
              <div className="detail-item">
                <label>Occupation</label>
                <div>{resident.occupation || '-'}</div>
              </div>
              <div className="detail-item">
                <label>Employment Status</label>
                <div>{resident.employmentStatus || '-'}</div>
              </div>
              <div className="detail-item">
                <label>Educational Attainment</label>
                <div>{resident.educationalAttainment || '-'}</div>
              </div>
            </div>
          </div>

          <div className="details-section">
            <h3>Household Information</h3>
            <div className="details-grid">
              <div className="detail-item">
                <label>Household Number</label>
                <div>{resident.household?.householdNumber || '-'}</div>
              </div>
              <div className="detail-item">
                <label>Relationship to Head</label>
                <div>{resident.relationshipToHead || '-'}</div>
              </div>
            </div>
          </div>

          <div className="details-section">
            <h3>Status & Classification</h3>
            <div className="details-grid">
              <div className="detail-item">
                <label>Voter Status</label>
                <div>
                  <span className={`badge ${resident.isVoter ? 'badge-success' : 'badge-secondary'}`}>
                    {resident.isVoter ? 'Yes' : 'No'}
                  </span>
                  {resident.isVoter && resident.voterId && (
                    <span style={{ marginLeft: '0.5rem', fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
                      (ID: {resident.voterId})
                    </span>
                  )}
                </div>
              </div>
              <div className="detail-item">
                <label>Person with Disability</label>
                <div>
                  <span className={`badge ${resident.isPWD ? 'badge-success' : 'badge-secondary'}`}>
                    {resident.isPWD ? 'Yes' : 'No'}
                  </span>
                </div>
              </div>
              <div className="detail-item">
                <label>Senior Citizen</label>
                <div>
                  <span className={`badge ${resident.isSenior ? 'badge-success' : 'badge-secondary'}`}>
                    {resident.isSenior ? 'Yes' : 'No'}
                  </span>
                </div>
              </div>
            </div>
          </div>

          {resident.notes && (
            <div className="details-section">
              <h3>Notes</h3>
              <div className="detail-item">
                <div style={{ padding: '0.75rem', background: 'var(--bg-primary)', borderRadius: '6px' }}>
                  {resident.notes}
                </div>
              </div>
            </div>
          )}
        </div>

        <div className="modal-actions">
          <button type="button" className="btn-secondary" onClick={onClose}>
            Close
          </button>
        </div>
      </div>
    </div>
  )
}


