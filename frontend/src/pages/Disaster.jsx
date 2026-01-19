import './Pages.css'

export default function Disaster() {
  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Disaster Response</h1>
          <p>Emergency mapping and evacuation center management</p>
        </div>
        <button className="btn-primary">Add Location</button>
      </div>
      <div className="empty-state">
        <p>Disaster response module coming soon.</p>
      </div>
    </div>
  )
}

