using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using System.Data;
using System.Data.Common;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestDoc = QuestPDF.Fluent;
using DocX = DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.Text;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportBuilderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportBuilderController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all available tables in the database
        /// </summary>
        [HttpGet("tables")]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME";
                
                using var reader = await command.ExecuteReaderAsync();
                var tables = new List<string>();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
                
                await connection.CloseAsync();

                return Ok(tables);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving tables", error = ex.Message });
            }
        }

        /// <summary>
        /// Get columns for a specific table
        /// </summary>
        [HttpGet("tables/{tableName}/columns")]
        public async Task<IActionResult> GetTableColumns(string tableName)
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        COLUMN_NAME,
                        DATA_TYPE,
                        IS_NULLABLE,
                        CHARACTER_MAXIMUM_LENGTH
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = @tableName
                    ORDER BY ORDINAL_POSITION";
                    
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@tableName";
                parameter.Value = tableName;
                command.Parameters.Add(parameter);
                
                using var reader = await command.ExecuteReaderAsync();
                var columnList = new List<object>();
                
                while (await reader.ReadAsync())
                {
                    columnList.Add(new
                    {
                        columnName = reader.IsDBNull(0) ? null : reader.GetString(0),
                        dataType = reader.IsDBNull(1) ? null : reader.GetString(1),
                        isNullable = reader.IsDBNull(2) ? false : reader.GetString(2) == "YES",
                        maxLength = reader.IsDBNull(3) ? null : reader.GetInt32(3).ToString()
                    });
                }
                
                await connection.CloseAsync();

                return Ok(columnList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error retrieving columns for table {tableName}", error = ex.Message });
            }
        }

        /// <summary>
        /// Get database schema (all tables with their columns)
        /// </summary>
        [HttpGet("schema")]
        public async Task<IActionResult> GetSchema()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();
                
                // Get all tables
                using var tableCommand = connection.CreateCommand();
                tableCommand.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME";
                
                using var tableReader = await tableCommand.ExecuteReaderAsync();
                var tables = new List<string>();
                while (await tableReader.ReadAsync())
                {
                    tables.Add(tableReader.GetString(0));
                }
                await tableReader.CloseAsync();

                var schema = new List<object>();

                foreach (var tableName in tables)
                {
                    using var columnCommand = connection.CreateCommand();
                    columnCommand.CommandText = @"
                        SELECT 
                            COLUMN_NAME,
                            DATA_TYPE,
                            IS_NULLABLE
                        FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = @tableName
                        ORDER BY ORDINAL_POSITION";
                        
                    var tableParam = columnCommand.CreateParameter();
                    tableParam.ParameterName = "@tableName";
                    tableParam.Value = tableName;
                    columnCommand.Parameters.Add(tableParam);
                    
                    using var columnReader = await columnCommand.ExecuteReaderAsync();
                    var columnList = new List<object>();
                    
                    while (await columnReader.ReadAsync())
                    {
                        columnList.Add(new
                        {
                            name = columnReader.IsDBNull(0) ? null : columnReader.GetString(0),
                            type = columnReader.IsDBNull(1) ? null : columnReader.GetString(1),
                            nullable = columnReader.IsDBNull(2) ? false : columnReader.GetString(2) == "YES"
                        });
                    }
                    
                    await columnReader.CloseAsync();

                    schema.Add(new
                    {
                        tableName = tableName,
                        columns = columnList
                    });
                }
                
                await connection.CloseAsync();

                return Ok(schema);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving schema", error = ex.Message });
            }
        }

        /// <summary>
        /// Execute a custom report query
        /// </summary>
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteReport([FromBody] ReportQueryRequest request)
        {
            try
            {
                // Basic validation - prevent dangerous SQL operations
                var upperQuery = request.Query.ToUpper();
                if (upperQuery.Contains("DROP") || 
                    upperQuery.Contains("DELETE") || 
                    upperQuery.Contains("UPDATE") || 
                    upperQuery.Contains("INSERT") ||
                    upperQuery.Contains("ALTER") ||
                    upperQuery.Contains("CREATE") ||
                    upperQuery.Contains("TRUNCATE") ||
                    upperQuery.Contains("EXEC") ||
                    upperQuery.Contains("EXECUTE"))
                {
                    return BadRequest(new { message = "Only SELECT queries are allowed" });
                }

                if (!upperQuery.TrimStart().StartsWith("SELECT"))
                {
                    return BadRequest(new { message = "Only SELECT queries are allowed" });
                }

                // Execute the query
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = request.Query;
                command.CommandTimeout = 300; // 5 minutes timeout

                using var reader = await command.ExecuteReaderAsync();
                
                var columns = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(reader.GetName(i));
                }

                var rows = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        row[columns[i]] = value;
                    }
                    rows.Add(row);
                }

                await connection.CloseAsync();

                return Ok(new
                {
                    columns = columns,
                    rows = rows,
                    rowCount = rows.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error executing report query", error = ex.Message });
            }
        }

        /// <summary>
        /// Export report to Excel
        /// </summary>
        [HttpPost("export/excel")]
        public IActionResult ExportToExcel([FromBody] ReportExportRequest request)
        {
            try
            {
                if (request.Data == null || request.Data.Columns == null || request.Data.Rows == null)
                {
                    return BadRequest(new { message = "Report data is required" });
                }

                var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Report");

                // Add title
                if (!string.IsNullOrEmpty(request.Title))
                {
                    worksheet.Cell(1, 1).Value = request.Title;
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                    worksheet.Range(1, 1, 1, request.Data.Columns.Count).Merge();
                }

                // Add headers
                var headerRow = string.IsNullOrEmpty(request.Title) ? 1 : 2;
                for (int i = 0; i < request.Data.Columns.Count; i++)
                {
                    worksheet.Cell(headerRow, i + 1).Value = request.Data.Columns[i];
                    worksheet.Cell(headerRow, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Add data rows
                for (int row = 0; row < request.Data.Rows.Count; row++)
                {
                    var dataRow = request.Data.Rows[row];
                    for (int col = 0; col < request.Data.Columns.Count; col++)
                    {
                        var value = dataRow.ContainsKey(request.Data.Columns[col]) 
                            ? dataRow[request.Data.Columns[col]] 
                            : null;
                        worksheet.Cell(headerRow + row + 1, col + 1).Value = value?.ToString() ?? string.Empty;
                    }
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Generate filename
                var fileName = string.IsNullOrEmpty(request.FileName) 
                    ? $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx" 
                    : request.FileName;

                // Save workbook to memory stream
                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting to Excel", error = ex.Message });
            }
        }

        /// <summary>
        /// Export report to PDF
        /// </summary>
        [HttpPost("export/pdf")]
        public IActionResult ExportToPdf([FromBody] ReportExportRequest request)
        {
            try
            {
                if (request.Data == null || request.Data.Columns == null || request.Data.Rows == null)
                {
                    return BadRequest(new { message = "Report data is required" });
                }

                QuestPDF.Settings.License = LicenseType.Community;

                var document = QuestDoc.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // Header
                        page.Header()
                            .Text(request.Title ?? "Report")
                            .SemiBold().FontSize(16).AlignCenter();

                        // Content - Table
                        page.Content()
                            .Table(table =>
                            {
                                // Table header
                                table.ColumnsDefinition(columns =>
                                {
                                    for (int i = 0; i < request.Data.Columns.Count; i++)
                                    {
                                        columns.RelativeColumn();
                                    }
                                });

                                // Header row
                                table.Header(header =>
                                {
                                    foreach (var column in request.Data.Columns)
                                    {
                                        header.Cell().Element(cell => cell
                                            .Background(Colors.Grey.Lighten3)
                                            .Padding(5)
                                            .Text(column)
                                            .SemiBold());
                                    }
                                });

                                // Data rows
                                foreach (var row in request.Data.Rows)
                                {
                                    foreach (var column in request.Data.Columns)
                                    {
                                        var value = row.ContainsKey(column) ? row[column] : null;
                                        table.Cell().Element(cell => cell
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(5)
                                            .Text(value?.ToString() ?? string.Empty));
                                    }
                                }
                            });

                        // Footer
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                            });
                    });
                });

                var fileName = string.IsNullOrEmpty(request.FileName) 
                    ? $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf" 
                    : request.FileName;

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting to PDF", error = ex.Message });
            }
        }

        /// <summary>
        /// Export report to DOCX
        /// </summary>
        [HttpPost("export/docx")]
        public IActionResult ExportToDocx([FromBody] ReportExportRequest request)
        {
            try
            {
                if (request.Data == null || request.Data.Columns == null || request.Data.Rows == null)
                {
                    return BadRequest(new { message = "Report data is required" });
                }

                using var stream = new MemoryStream();
                using (var wordDoc = WordprocessingDocument.Create(stream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    var mainPart = wordDoc.AddMainDocumentPart();
                    var doc = new DocX.Document();
                    mainPart.Document = doc;
                    var body = doc.AppendChild(new DocX.Body());

                    // Title
                    if (!string.IsNullOrEmpty(request.Title))
                    {
                        var titlePara = body.AppendChild(new DocX.Paragraph());
                        var titleRun = titlePara.AppendChild(new DocX.Run());
                        titleRun.AppendChild(new DocX.Text(request.Title));
                        titleRun.RunProperties = new DocX.RunProperties(
                            new DocX.Bold(),
                            new DocX.FontSize { Val = "28" }
                        );
                        titlePara.ParagraphProperties = new DocX.ParagraphProperties(
                            new DocX.Justification { Val = DocX.JustificationValues.Center },
                            new DocX.SpacingBetweenLines { After = "200" }
                        );
                    }

                    // Table
                    var table = body.AppendChild(new DocX.Table(
                        new DocX.TableProperties(
                            new DocX.TableWidth { Width = "0", Type = DocX.TableWidthUnitValues.Auto }
                        )
                    ));

                    // Table grid
                    var grid = new DocX.TableGrid();
                    for (int i = 0; i < request.Data.Columns.Count; i++)
                    {
                        grid.AppendChild(new DocX.GridColumn());
                    }
                    table.AppendChild(grid);

                    // Header row
                    var headerRow = table.AppendChild(new DocX.TableRow());
                    foreach (var column in request.Data.Columns)
                    {
                        var cell = headerRow.AppendChild(new DocX.TableCell(
                            new DocX.Paragraph(
                                new DocX.Run(
                                    new DocX.Text(column),
                                    new DocX.RunProperties(new DocX.Bold())
                                )
                            )
                        ));
                        cell.TableCellProperties = new DocX.TableCellProperties(
                            new DocX.Shading { Fill = "D3D3D3" }
                        );
                    }

                    // Data rows
                    foreach (var row in request.Data.Rows)
                    {
                        var tableRow = table.AppendChild(new DocX.TableRow());
                        foreach (var column in request.Data.Columns)
                        {
                            var value = row.ContainsKey(column) ? row[column] : null;
                            tableRow.AppendChild(new DocX.TableCell(
                                new DocX.Paragraph(
                                    new DocX.Run(new DocX.Text(value?.ToString() ?? string.Empty))
                                )
                            ));
                        }
                    }

                    wordDoc.MainDocumentPart!.Document.Save();
                }

                var fileName = string.IsNullOrEmpty(request.FileName) 
                    ? $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.docx" 
                    : request.FileName;

                return File(stream.ToArray(), 
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document", 
                    fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting to DOCX", error = ex.Message });
            }
        }

        /// <summary>
        /// Generate SQL query from report builder configuration
        /// </summary>
        [HttpPost("generate-sql")]
        public IActionResult GenerateSql([FromBody] ReportConfiguration config)
        {
            try
            {
                if (config == null)
                {
                    return BadRequest(new { message = "Configuration is required" });
                }

                // Filter out invalid filters, sorts, and groups
                if (config.Filters != null)
                {
                    config.Filters = config.Filters
                        .Where(f => !string.IsNullOrWhiteSpace(f.ColumnName) && !string.IsNullOrWhiteSpace(f.TableName))
                        .ToList();
                }

                if (config.SortBy != null)
                {
                    config.SortBy = config.SortBy
                        .Where(s => !string.IsNullOrWhiteSpace(s.ColumnName) && !string.IsNullOrWhiteSpace(s.TableName))
                        .ToList();
                }

                if (config.GroupBy != null)
                {
                    config.GroupBy = config.GroupBy
                        .Where(g => !string.IsNullOrWhiteSpace(g.ColumnName) && !string.IsNullOrWhiteSpace(g.TableName))
                        .ToList();
                }

                var sql = BuildSqlQuery(config);
                return Ok(new { query = sql });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating SQL", error = ex.Message, details = ex.ToString() });
            }
        }

        private string BuildSqlQuery(ReportConfiguration config)
        {
            if (config.Tables == null || config.Tables.Count == 0)
            {
                throw new ArgumentException("At least one table must be selected");
            }

            var selectFields = new List<string>();
            var fromClause = config.Tables.First(); // Primary table
            var joins = new List<string>();
            var whereClause = new List<string>();
            var groupByClause = new List<string>();
            var orderByClause = new List<string>();

            // Build SELECT clause
            if (config.Fields != null && config.Fields.Count > 0)
            {
                foreach (var field in config.Fields)
                {
                    var fieldName = $"[{field.TableName}].[{field.ColumnName}]";
                    if (!string.IsNullOrEmpty(field.Alias))
                    {
                        fieldName += $" AS [{field.Alias}]";
                    }
                    selectFields.Add(fieldName);
                }
            }
            else
            {
                selectFields.Add($"[{fromClause}].*");
            }

            // Build JOIN clauses
            if (config.Joins != null)
            {
                foreach (var join in config.Joins)
                {
                    var joinClause = $"{join.Type} JOIN [{join.TargetTable}] ON [{join.SourceTable}].[{join.SourceColumn}] = [{join.TargetTable}].[{join.TargetColumn}]";
                    joins.Add(joinClause);
                }
            }

            // Build WHERE clause
            if (config.Filters != null && config.Filters.Count > 0)
            {
                foreach (var filter in config.Filters)
                {
                    // Skip invalid filters
                    if (string.IsNullOrWhiteSpace(filter.ColumnName) || string.IsNullOrWhiteSpace(filter.TableName))
                        continue;

                    var filterClause = BuildFilterClause(filter);
                    whereClause.Add(filterClause);
                }
            }

            // Build GROUP BY clause
            if (config.GroupBy != null && config.GroupBy.Count > 0)
            {
                foreach (var group in config.GroupBy)
                {
                    groupByClause.Add($"[{group.TableName}].[{group.ColumnName}]");
                }
            }

            // Build ORDER BY clause
            if (config.SortBy != null && config.SortBy.Count > 0)
            {
                foreach (var sort in config.SortBy)
                {
                    var sortClause = $"[{sort.TableName}].[{sort.ColumnName}] {sort.Direction}";
                    orderByClause.Add(sortClause);
                }
            }

            // Construct final SQL
            var sql = $"SELECT {string.Join(", ", selectFields)} FROM [{fromClause}]";
            
            if (joins.Count > 0)
            {
                sql += " " + string.Join(" ", joins);
            }

            if (whereClause.Count > 0)
            {
                sql += $" WHERE {string.Join(" AND ", whereClause)}";
            }

            if (groupByClause.Count > 0)
            {
                sql += $" GROUP BY {string.Join(", ", groupByClause)}";
            }

            if (orderByClause.Count > 0)
            {
                sql += $" ORDER BY {string.Join(", ", orderByClause)}";
            }

            // Add TOP clause if limit is specified
            if (config.Limit > 0)
            {
                sql = $"SELECT TOP {config.Limit} " + sql.Substring(7); // Replace "SELECT " with "SELECT TOP N "
            }

            return sql;
        }

        private string BuildFilterClause(ReportFilter filter)
        {
            var columnName = $"[{filter.TableName}].[{filter.ColumnName}]";
            var value = filter.Value;

            // Escape SQL string values to prevent injection
            static string EscapeSqlString(object? val)
            {
                if (val == null) return string.Empty;
                return val.ToString()?.Replace("'", "''") ?? string.Empty;
            }

            switch (filter.Operator?.ToUpper())
            {
                case "EQUALS":
                case "=":
                    if (string.IsNullOrEmpty(value?.ToString()))
                    {
                        throw new ArgumentException($"Value is required for EQUALS operator on {filter.ColumnName}");
                    }
                    if (IsStringType(filter.DataType))
                    {
                        return $"{columnName} = '{EscapeSqlString(value)}'";
                    }
                    return $"{columnName} = {value}";

                case "NOT_EQUALS":
                case "!=":
                case "<>":
                    if (string.IsNullOrEmpty(value?.ToString()))
                    {
                        throw new ArgumentException($"Value is required for NOT_EQUALS operator on {filter.ColumnName}");
                    }
                    if (IsStringType(filter.DataType))
                    {
                        return $"{columnName} <> '{EscapeSqlString(value)}'";
                    }
                    return $"{columnName} <> {value}";

                case "CONTAINS":
                case "LIKE":
                    if (string.IsNullOrEmpty(value?.ToString()))
                    {
                        throw new ArgumentException($"Value is required for CONTAINS operator on {filter.ColumnName}");
                    }
                    return $"{columnName} LIKE '%{EscapeSqlString(value)}%'";

                case "STARTS_WITH":
                    if (string.IsNullOrEmpty(value?.ToString()))
                    {
                        throw new ArgumentException($"Value is required for STARTS_WITH operator on {filter.ColumnName}");
                    }
                    return $"{columnName} LIKE '{EscapeSqlString(value)}%'";

                case "ENDS_WITH":
                    if (string.IsNullOrEmpty(value?.ToString()))
                    {
                        throw new ArgumentException($"Value is required for ENDS_WITH operator on {filter.ColumnName}");
                    }
                    return $"{columnName} LIKE '%{EscapeSqlString(value)}'";

                case "GREATER_THAN":
                case ">":
                    return $"{columnName} > {value}";

                case "GREATER_THAN_OR_EQUAL":
                case ">=":
                    return $"{columnName} >= {value}";

                case "LESS_THAN":
                case "<":
                    return $"{columnName} < {value}";

                case "LESS_THAN_OR_EQUAL":
                case "<=":
                    return $"{columnName} <= {value}";

                case "IS_NULL":
                    return $"{columnName} IS NULL";

                case "IS_NOT_NULL":
                    return $"{columnName} IS NOT NULL";

                case "IN":
                    if (string.IsNullOrEmpty(value?.ToString()))
                    {
                        throw new ArgumentException($"Value is required for IN operator on {filter.ColumnName}");
                    }
                    var values = value.ToString()!.Split(',').Select(v => IsStringType(filter.DataType) ? $"'{EscapeSqlString(v.Trim())}'" : v.Trim());
                    return $"{columnName} IN ({string.Join(", ", values)})";

                case "BETWEEN":
                    var rangeValues = value.ToString().Split('|');
                    if (rangeValues.Length == 2)
                    {
                        return $"{columnName} BETWEEN {rangeValues[0]} AND {rangeValues[1]}";
                    }
                    throw new ArgumentException("BETWEEN operator requires two values separated by |");

                default:
                    throw new ArgumentException($"Unknown filter operator: {filter.Operator}");
            }
        }

        private bool IsStringType(string? dataType)
        {
            if (string.IsNullOrEmpty(dataType)) return false;
            var upperType = dataType.ToUpper();
            return upperType.Contains("CHAR") || upperType.Contains("TEXT") || upperType.Contains("VARCHAR") || upperType.Contains("NVARCHAR");
        }
    }

    // DTOs
    public class ReportQueryRequest
    {
        public string Query { get; set; } = string.Empty;
    }

    public class ReportConfiguration
    {
        public List<string> Tables { get; set; } = new();
        public List<ReportField> Fields { get; set; } = new();
        public List<ReportJoin> Joins { get; set; } = new();
        public List<ReportFilter> Filters { get; set; } = new();
        public List<ReportGroupBy> GroupBy { get; set; } = new();
        public List<ReportSort> SortBy { get; set; } = new();
        public int Limit { get; set; } = 0;
    }

    public class ReportField
    {
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string? Alias { get; set; }
        public string? AggregateFunction { get; set; } // SUM, COUNT, AVG, MIN, MAX
    }

    public class ReportJoin
    {
        public string Type { get; set; } = "INNER"; // INNER, LEFT, RIGHT, FULL
        public string SourceTable { get; set; } = string.Empty;
        public string SourceColumn { get; set; } = string.Empty;
        public string TargetTable { get; set; } = string.Empty;
        public string TargetColumn { get; set; } = string.Empty;
    }

    public class ReportFilter
    {
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty; // EQUALS, CONTAINS, GREATER_THAN, etc.
        public object? Value { get; set; }
    }

    public class ReportGroupBy
    {
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
    }

    public class ReportSort
    {
        public string TableName { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string Direction { get; set; } = "ASC"; // ASC, DESC
    }

    public class ReportExportRequest
    {
        public string? Title { get; set; }
        public string? FileName { get; set; }
        public ReportData? Data { get; set; }
    }

    public class ReportData
    {
        public List<string> Columns { get; set; } = new();
        public List<Dictionary<string, object>> Rows { get; set; } = new();
    }
}
