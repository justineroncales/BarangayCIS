import './Pages.css'

export default function Suggestions() {
  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Suggestion Box</h1>
          <p>View and respond to anonymous feedback from residents</p>
        </div>
      </div>
      <div className="empty-state">
        <p>No suggestions yet.</p>
      </div>
    </div>
  )
}

