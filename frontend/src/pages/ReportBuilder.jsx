import { useState, useEffect } from 'react'
import { useQuery, useMutation } from '@tanstack/react-query'
import api from '../services/api'
import { 
  DndContext, 
  DragOverlay,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors 
} from '@dnd-kit/core'
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy
} from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { 
  Plus, X, Database, Columns, Filter, SortAsc, 
  Group, Eye, Save, Loader, Search, FileText,
  Download, Printer, FileSpreadsheet, FileType 
} from 'lucide-react'
import { toast } from 'react-hot-toast'
import './Pages.css'
import './ReportBuilder.css'

function SortableItem({ id, children, onRemove }) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging
  } = useSortable({ id })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1
  }

  return (
    <div ref={setNodeRef} style={style} {...attributes} {...listeners} className="sortable-field">
      {children}
      {onRemove && (
        <button 
          className="btn-icon btn-remove" 
          onClick={(e) => {
            e.stopPropagation()
            onRemove(id)
          }}
        >
          <X size={14} />
        </button>
      )}
    </div>
  )
}

export default function ReportBuilder() {
  const [selectedTable, setSelectedTable] = useState('')
  const [selectedFields, setSelectedFields] = useState([])
  const [filters, setFilters] = useState([])
  const [sortBy, setSortBy] = useState([])
  const [groupBy, setGroupBy] = useState([])
  const [reportData, setReportData] = useState(null)
  const [isExecuting, setIsExecuting] = useState(false)
  const [activeId, setActiveId] = useState(null)

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  )

  // Get database schema
  const { data: schema, isLoading: schemaLoading } = useQuery({
    queryKey: ['report-builder-schema'],
    queryFn: () => api.get('/reportbuilder/schema').then(res => res.data),
    retry: false
  })

  // Get columns for selected table
  const { data: columns = [] } = useQuery({
    queryKey: ['report-builder-columns', selectedTable],
    queryFn: () => api.get(`/reportbuilder/tables/${selectedTable}/columns`).then(res => res.data),
    enabled: !!selectedTable,
    retry: false
  })

  // Handle drag end
  const handleDragEnd = (event) => {
    const { active, over } = event
    setActiveId(null)

    if (over && active.id !== over.id) {
      setSelectedFields((items) => {
        const oldIndex = items.findIndex(item => item.id === active.id)
        const newIndex = items.findIndex(item => item.id === over.id)
        return arrayMove(items, oldIndex, newIndex)
      })
    }
  }

  const handleDragStart = (event) => {
    setActiveId(event.active.id)
  }

  // Add field to report
  const addField = (column) => {
    const fieldId = `field-${Date.now()}-${Math.random()}`
    setSelectedFields([...selectedFields, {
      id: fieldId,
      tableName: selectedTable,
      columnName: column.columnName,
      alias: '',
      aggregateFunction: null
    }])
  }

  // Remove field
  const removeField = (fieldId) => {
    setSelectedFields(selectedFields.filter(f => f.id !== fieldId))
  }

  // Add filter
  const addFilter = () => {
    const filterId = `filter-${Date.now()}`
    setFilters([...filters, {
      id: filterId,
      tableName: selectedTable,
      columnName: '',
      dataType: '',
      operator: 'EQUALS',
      value: ''
    }])
  }

  // Remove filter
  const removeFilter = (filterId) => {
    setFilters(filters.filter(f => f.id !== filterId))
  }

  // Generate and execute report
  const executeReport = async () => {
    if (selectedFields.length === 0) {
      toast.error('Please add at least one field to the report')
      return
    }

    setIsExecuting(true)
    try {
      const config = {
        tables: [selectedTable],
        fields: selectedFields.map(f => ({
          tableName: f.tableName,
          columnName: f.columnName,
          alias: f.alias || null,
          aggregateFunction: f.aggregateFunction || null
        })),
        // Only include filters that have a column name selected
        filters: filters
          .filter(f => f.columnName && f.tableName)
          .map(f => ({
            tableName: f.tableName,
            columnName: f.columnName,
            dataType: f.dataType || '',
            operator: f.operator || 'EQUALS',
            value: f.value || ''
          })),
        // Only include sorts that have a column name
        sortBy: sortBy
          .filter(s => s.columnName && s.tableName)
          .map(s => ({
            tableName: s.tableName,
            columnName: s.columnName,
            direction: s.direction || 'ASC'
          })),
        // Only include groupBy that have a column name
        groupBy: groupBy
          .filter(g => g.columnName && g.tableName)
          .map(g => ({
            tableName: g.tableName,
            columnName: g.columnName
          })),
        limit: 0
      }

      // First generate SQL
      const sqlResponse = await api.post('/reportbuilder/generate-sql', config)
      const query = sqlResponse.data.query

      // Then execute the query
      const result = await api.post('/reportbuilder/execute', { query })
      setReportData(result.data)
      
      toast.success(`Report executed successfully! ${result.data.rowCount} rows returned`)
    } catch (error) {
      const errorMessage = error.response?.data?.message || error.response?.data?.error || error.message || 'Failed to execute report'
      toast.error(errorMessage)
      console.error('Report execution error:', error.response?.data || error)
    } finally {
      setIsExecuting(false)
    }
  }

  // Export report to different formats
  const exportReport = async (format) => {
    if (!reportData) {
      toast.error('No report data to export')
      return
    }

    // Map format to correct file extension
    const fileExtensionMap = {
      'excel': 'xlsx',
      'pdf': 'pdf',
      'docx': 'docx'
    }
    const fileExtension = fileExtensionMap[format] || format

    try {
      const exportData = {
        title: selectedTable ? `Report - ${selectedTable}` : 'Report',
        fileName: `Report_${selectedTable || 'data'}_${new Date().toISOString().split('T')[0]}.${fileExtension}`,
        data: {
          columns: reportData.columns,
          rows: reportData.rows
        }
      }

      const response = await api.post(`/reportbuilder/export/${format}`, exportData, {
        responseType: 'blob'
      })

      // Create download link
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', exportData.fileName)
      document.body.appendChild(link)
      link.click()
      link.remove()
      window.URL.revokeObjectURL(url)

      toast.success(`Report exported to ${format.toUpperCase()} successfully!`)
    } catch (error) {
      toast.error(error.response?.data?.message || `Failed to export to ${format.toUpperCase()}`)
      console.error('Export error:', error.response?.data || error)
    }
  }

  const tables = schema?.map(t => t.tableName) || []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1>Report Builder</h1>
          <p>Create custom reports by dragging and dropping fields from database tables</p>
        </div>
        <button 
          className="btn-primary" 
          onClick={executeReport}
          disabled={isExecuting || selectedFields.length === 0}
        >
          {isExecuting ? <Loader size={20} className="spinner" /> : <Eye size={20} />}
          {isExecuting ? 'Executing...' : 'Execute Report'}
        </button>
      </div>

      <div className="report-builder-container">
        {/* Left Panel - Tables and Fields */}
        <div className="report-builder-sidebar">
          <div className="sidebar-section">
            <h3>
              <Database size={16} />
              Database Tables
            </h3>
            {schemaLoading ? (
              <div className="loading">Loading tables...</div>
            ) : (
              <select 
                value={selectedTable} 
                onChange={(e) => {
                  setSelectedTable(e.target.value)
                  setSelectedFields([])
                  setFilters([])
                  setSortBy([])
                  setGroupBy([])
                }}
                className="table-selector"
              >
                <option value="">Select a table...</option>
                {tables.map(table => (
                  <option key={table} value={table}>{table}</option>
                ))}
              </select>
            )}
          </div>

          {selectedTable && columns.length > 0 && (
            <div className="sidebar-section">
              <h3>
                <Columns size={16} />
                Available Fields
              </h3>
              <div className="fields-list">
                {columns.map((column, idx) => (
                  <div 
                    key={idx} 
                    className="field-item"
                    onClick={() => addField(column)}
                  >
                    <div className="field-name">{column.columnName}</div>
                    <div className="field-type">{column.dataType}</div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Center Panel - Report Configuration */}
        <div className="report-builder-main">
          <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragStart={handleDragStart}
            onDragEnd={handleDragEnd}
          >
            {/* Selected Fields */}
            <div className="builder-section">
              <h3>Report Fields (Drag to reorder)</h3>
              <SortableContext
                items={selectedFields.map(f => f.id)}
                strategy={verticalListSortingStrategy}
              >
                <div className="selected-fields">
                  {selectedFields.length === 0 ? (
                    <div className="empty-state-small">
                      <p>No fields selected. Click on fields from the left panel to add them.</p>
                    </div>
                  ) : (
                    selectedFields.map((field) => (
                      <SortableItem key={field.id} id={field.id} onRemove={removeField}>
                        <div className="field-config">
                          <div className="field-info">
                            <span className="field-label">{field.tableName}.{field.columnName}</span>
                            {field.alias && (
                              <span className="field-alias">as {field.alias}</span>
                            )}
                          </div>
                          <input
                            type="text"
                            placeholder="Alias (optional)"
                            value={field.alias}
                            onChange={(e) => {
                              setSelectedFields(selectedFields.map(f =>
                                f.id === field.id ? { ...f, alias: e.target.value } : f
                              ))
                            }}
                            className="field-alias-input"
                            onClick={(e) => e.stopPropagation()}
                          />
                        </div>
                      </SortableItem>
                    ))
                  )}
                </div>
              </SortableContext>
            </div>

            {/* Filters */}
            <div className="builder-section">
              <div className="section-header">
                <h3>
                  <Filter size={16} />
                  Filters
                </h3>
                <button className="btn-secondary btn-small" onClick={addFilter}>
                  <Plus size={14} />
                  Add Filter
                </button>
              </div>
              {filters.length === 0 ? (
                <div className="empty-state-small">
                  <p>No filters. Click "Add Filter" to add filtering conditions.</p>
                </div>
              ) : (
                <div className="filters-list">
                  {filters.map((filter, idx) => {
                    const availableColumns = columns.filter(c => 
                      !filters.slice(0, idx).some(f => f.columnName === c.columnName)
                    )
                    
                    return (
                      <div key={filter.id} className="filter-item">
                        <select
                          value={filter.columnName}
                          onChange={(e) => {
                            const column = columns.find(c => c.columnName === e.target.value)
                            setFilters(filters.map(f =>
                              f.id === filter.id 
                                ? { ...f, columnName: e.target.value, dataType: column?.dataType || '' }
                                : f
                            ))
                          }}
                        >
                          <option value="">Select column...</option>
                          {columns.map(col => (
                            <option key={col.columnName} value={col.columnName}>
                              {col.columnName} ({col.dataType})
                            </option>
                          ))}
                        </select>
                        <select
                          value={filter.operator}
                          onChange={(e) => {
                            setFilters(filters.map(f =>
                              f.id === filter.id ? { ...f, operator: e.target.value } : f
                            ))
                          }}
                        >
                          <option value="EQUALS">Equals (=)</option>
                          <option value="NOT_EQUALS">Not Equals (&lt;&gt;)</option>
                          <option value="CONTAINS">Contains (LIKE %...%)</option>
                          <option value="STARTS_WITH">Starts With (LIKE ...%)</option>
                          <option value="ENDS_WITH">Ends With (LIKE %...)</option>
                          <option value="GREATER_THAN">Greater Than (&gt;)</option>
                          <option value="GREATER_THAN_OR_EQUAL">Greater Than or Equal (&gt;=)</option>
                          <option value="LESS_THAN">Less Than (&lt;)</option>
                          <option value="LESS_THAN_OR_EQUAL">Less Than or Equal (&lt;=)</option>
                          <option value="IS_NULL">Is Null</option>
                          <option value="IS_NOT_NULL">Is Not Null</option>
                          <option value="IN">In (...)</option>
                          <option value="BETWEEN">Between</option>
                        </select>
                        {!['IS_NULL', 'IS_NOT_NULL'].includes(filter.operator) && (
                          <input
                            type="text"
                            placeholder="Value"
                            value={filter.value}
                            onChange={(e) => {
                              setFilters(filters.map(f =>
                                f.id === filter.id ? { ...f, value: e.target.value } : f
                              ))
                            }}
                          />
                        )}
                        <button 
                          className="btn-icon btn-danger" 
                          onClick={() => removeFilter(filter.id)}
                        >
                          <X size={16} />
                        </button>
                      </div>
                    )
                  })}
                </div>
              )}
            </div>

            {/* Sort By */}
            <div className="builder-section">
              <div className="section-header">
                <h3>
                  <SortAsc size={16} />
                  Sort By
                </h3>
                <button 
                  className="btn-secondary btn-small" 
                  onClick={() => {
                    if (selectedFields.length > 0) {
                      const firstField = selectedFields[0]
                      setSortBy([...sortBy, {
                        id: `sort-${Date.now()}`,
                        tableName: firstField.tableName,
                        columnName: firstField.columnName,
                        direction: 'ASC'
                      }])
                    }
                  }}
                >
                  <Plus size={14} />
                  Add Sort
                </button>
              </div>
              {sortBy.length === 0 ? (
                <div className="empty-state-small">
                  <p>No sorting. Click "Add Sort" to add sort criteria.</p>
                </div>
              ) : (
                <div className="sort-list">
                  {sortBy.map((sort) => (
                    <div key={sort.id} className="sort-item">
                      <select
                        value={`${sort.tableName}.${sort.columnName}`}
                        onChange={(e) => {
                          const [table, column] = e.target.value.split('.')
                          setSortBy(sortBy.map(s =>
                            s.id === sort.id ? { ...s, tableName: table, columnName: column } : s
                          ))
                        }}
                      >
                        {selectedFields.map(field => (
                          <option key={field.id} value={`${field.tableName}.${field.columnName}`}>
                            {field.tableName}.{field.columnName}
                          </option>
                        ))}
                      </select>
                      <select
                        value={sort.direction}
                        onChange={(e) => {
                          setSortBy(sortBy.map(s =>
                            s.id === sort.id ? { ...s, direction: e.target.value } : s
                          ))
                        }}
                      >
                        <option value="ASC">Ascending</option>
                        <option value="DESC">Descending</option>
                      </select>
                      <button 
                        className="btn-icon btn-danger" 
                        onClick={() => setSortBy(sortBy.filter(s => s.id !== sort.id))}
                      >
                        <X size={16} />
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <DragOverlay>
              {activeId ? (
                <div className="sortable-field dragging">
                  {selectedFields.find(f => f.id === activeId)?.tableName}.
                  {selectedFields.find(f => f.id === activeId)?.columnName}
                </div>
              ) : null}
            </DragOverlay>
          </DndContext>
        </div>
      </div>

      {/* Right Panel - Results */}
      {reportData && (
        <div className="report-results">
          <div className="results-header">
            <h3>
              <FileText size={16} />
              Report Results ({reportData.rowCount} rows)
            </h3>
            <div className="export-buttons">
              <button 
                className="btn-secondary btn-small"
                onClick={() => exportReport('excel')}
                title="Download as Excel"
              >
                <FileSpreadsheet size={16} />
                Excel
              </button>
              <button 
                className="btn-secondary btn-small"
                onClick={() => exportReport('pdf')}
                title="Download as PDF"
              >
                <FileType size={16} />
                PDF
              </button>
              <button 
                className="btn-secondary btn-small"
                onClick={() => exportReport('docx')}
                title="Download as Word"
              >
                <FileText size={16} />
                Word
              </button>
              <button 
                className="btn-secondary btn-small"
                onClick={() => window.print()}
                title="Print"
              >
                <Printer size={16} />
                Print
              </button>
            </div>
          </div>
          <div className="results-table-container">
            <table className="data-table">
              <thead>
                <tr>
                  {reportData.columns.map((col, idx) => (
                    <th key={idx}>{col}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {reportData.rows.length === 0 ? (
                  <tr>
                    <td colSpan={reportData.columns.length} style={{ textAlign: 'center', padding: '2rem' }}>
                      No data found
                    </td>
                  </tr>
                ) : (
                  reportData.rows.map((row, rowIdx) => (
                    <tr key={rowIdx}>
                      {reportData.columns.map((col, colIdx) => (
                        <td key={colIdx}>
                          {row[col] !== null && row[col] !== undefined ? String(row[col]) : '-'}
                        </td>
                      ))}
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}
