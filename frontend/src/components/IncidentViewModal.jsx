import { X, Loader2 } from 'lucide-react'

export default function IncidentViewModal({ isOpen, onClose, incident }) {
  if (!isOpen) return null
  
  if (!incident) {
    return (
      <div className="modal-overlay" onClick={onClose}>
        <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
          <div className="modal-header">
            <h2>Incident Details</h2>
            <button className="modal-close" onClick={onClose}>
              <X size={24} />
            </button>
          </div>
          <div style={{ padding: '2rem', textAlign: 'center' }}>
            <Loader2 size={32} className="spinner" />
            <p style={{ marginTop: '1rem' }}>Loading incident details...</p>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content modal-large" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Incident Details</h2>
          <button className="modal-close" onClick={onClose}>
            <X size={24} />
          </button>
        </div>

        <div className="resident-details">
          <div className="details-section">
            <h3>Incident Information</h3>
            <div className="details-grid">
              <div className="detail-item">
                <label>Incident Number</label>
                <div style={{ fontWeight: 600, color: 'var(--accent)' }}>
                  {incident.incidentNumber}
                </div>
              </div>
              <div className="detail-item">
                <label>Type</label>
                <div>
                  <span className="badge">{incident.incidentType}</span>
                </div>
              </div>
              <div className="detail-item">
                <label>Status</label>
                <div>
                  <span className={`badge badge-${incident.status.toLowerCase().replace(' ', '-')}`}>
                    {incident.status}
                  </span>
                </div>
              </div>
              <div className="detail-item">
                <label>Title</label>
                <div style={{ fontWeight: 500 }}>{incident.title}</div>
              </div>
            </div>
          </div>

          <div className="details-section">
            <h3>Dates & Timeline</h3>
            <div className="details-grid">
              <div className="detail-item">
                <label>Incident Date</label>
                <div>{new Date(incident.incidentDate).toLocaleDateString()}</div>
              </div>
              {incident.reportedDate && (
                <div className="detail-item">
                  <label>Reported Date</label>
                  <div>{new Date(incident.reportedDate).toLocaleDateString()}</div>
                </div>
              )}
              {incident.resolutionDate && (
                <div className="detail-item">
                  <label>Resolution Date</label>
                  <div>{new Date(incident.resolutionDate).toLocaleDateString()}</div>
                </div>
              )}
              {incident.mediationScheduledDate && (
                <div className="detail-item">
                  <label>Mediation Scheduled</label>
                  <div>{new Date(incident.mediationScheduledDate).toLocaleDateString()}</div>
                </div>
              )}
            </div>
          </div>

          {incident.description && (
            <div className="details-section">
              <h3>Description</h3>
              <div className="detail-item">
                <div style={{ padding: '0.75rem', background: 'var(--bg-primary)', borderRadius: '6px', whiteSpace: 'pre-wrap' }}>
                  {incident.description}
                </div>
              </div>
            </div>
          )}

          {incident.location && (
            <div className="details-section">
              <h3>Location</h3>
              <div className="detail-item">
                <div>{incident.location}</div>
              </div>
            </div>
          )}

          <div className="details-section">
            <h3>Parties Involved</h3>
            <div className="details-grid">
              <div className="detail-item">
                <label>Complainant</label>
                <div>
                  {incident.complainant ? (
                    <>
                      {incident.complainant.firstName} {incident.complainant.lastName}
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                        {incident.complainant.address}
                      </div>
                    </>
                  ) : incident.complainantName ? (
                    incident.complainantName
                  ) : (
                    '-'
                  )}
                </div>
              </div>
              <div className="detail-item">
                <label>Respondent</label>
                <div>
                  {incident.respondent ? (
                    <>
                      {incident.respondent.firstName} {incident.respondent.lastName}
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                        {incident.respondent.address}
                      </div>
                    </>
                  ) : incident.respondentName ? (
                    incident.respondentName
                  ) : (
                    '-'
                  )}
                </div>
              </div>
            </div>
          </div>

          <div className="details-section">
            <h3>Assignment & Reporting</h3>
            <div className="details-grid">
              {incident.assignedTo && (
                <div className="detail-item">
                  <label>Assigned To</label>
                  <div>{incident.assignedTo}</div>
                </div>
              )}
              {incident.reportedBy && (
                <div className="detail-item">
                  <label>Reported By</label>
                  <div>{incident.reportedBy}</div>
                </div>
              )}
            </div>
          </div>

          {incident.actionTaken && (
            <div className="details-section">
              <h3>Action Taken</h3>
              <div className="detail-item">
                <div style={{ padding: '0.75rem', background: 'var(--bg-primary)', borderRadius: '6px', whiteSpace: 'pre-wrap' }}>
                  {incident.actionTaken}
                </div>
              </div>
            </div>
          )}

          {incident.resolution && (
            <div className="details-section">
              <h3>Resolution</h3>
              <div className="detail-item">
                <div style={{ padding: '0.75rem', background: 'var(--bg-primary)', borderRadius: '6px', whiteSpace: 'pre-wrap' }}>
                  {incident.resolution}
                </div>
              </div>
            </div>
          )}

          {incident.attachments && incident.attachments.length > 0 && (
            <div className="details-section">
              <h3>Attachments</h3>
              <div className="detail-item">
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                  {incident.attachments.map((attachment) => (
                    <div key={attachment.id} style={{ padding: '0.5rem', background: 'var(--bg-primary)', borderRadius: '4px' }}>
                      <div style={{ fontWeight: 500 }}>{attachment.fileName}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>
                        {attachment.fileType} • {(attachment.fileSize / 1024).toFixed(2)} KB
                      </div>
                    </div>
                  ))}
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

