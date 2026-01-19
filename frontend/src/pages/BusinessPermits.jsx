import './Pages.css'

export default function BusinessPermits() {
  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Business Permits</h1>
          <p>Manage business permit applications and renewals</p>
        </div>
        <button className="btn-primary">New Application</button>
      </div>
      <div className="empty-state">
        <p>Business permit module coming soon.</p>
      </div>
    </div>
  )
}

