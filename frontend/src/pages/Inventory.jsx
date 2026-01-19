import './Pages.css'

export default function Inventory() {
  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Inventory</h1>
          <p>Track barangay assets and equipment</p>
        </div>
        <button className="btn-primary">Add Item</button>
      </div>
      <div className="empty-state">
        <p>Inventory module coming soon.</p>
      </div>
    </div>
  )
}

