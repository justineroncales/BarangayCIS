using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    // Hotline / Citizen Assistance
    public class CitizenReport
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string ReportType { get; set; } = string.Empty; // Pothole, Emergency, Noise, etc.
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [StringLength(255)]
        public string? Location { get; set; }
        
        [StringLength(100)]
        public string? ReporterName { get; set; }
        
        [StringLength(20)]
        public string? ReporterContact { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Resolved, Closed
        
        [StringLength(2000)]
        public string? Resolution { get; set; }
        
        public DateTime? ResolvedDate { get; set; }
        
        [StringLength(100)]
        public string? AssignedTo { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
    
    // Event & Announcement
    public class Announcement
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Content { get; set; }
        
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // General, Disaster, Event, etc.
        
        [StringLength(50)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
        
        public DateTime? EventDate { get; set; }
        
        public DateTime? EventEndDate { get; set; }
        
        [StringLength(255)]
        public string? Location { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Expired, Archived
        
        public bool IsPublished { get; set; } = false;
        
        public DateTime? PublishedAt { get; set; }
        
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
    
    // Disaster Response
    public class DisasterMap
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string LocationName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string LocationType { get; set; } = string.Empty; // EvacuationCenter, FloodProne, HazardZone
        
        public decimal? Latitude { get; set; }
        
        public decimal? Longitude { get; set; }
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Inactive
        
        public int? Capacity { get; set; }
        
        public int? CurrentOccupancy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class Evacuee
    {
        public int Id { get; set; }
        
        public int ResidentId { get; set; }
        
        public int DisasterMapId { get; set; }
        
        public DateTime EvacuatedDate { get; set; }
        
        public DateTime? ReturnedDate { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Evacuated"; // Evacuated, Returned
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Resident Resident { get; set; } = null!;
        
        public DisasterMap DisasterMap { get; set; } = null!;
    }
    
    // Staff Task & Scheduling
    public class StaffTask
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string TaskName { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [StringLength(100)]
        public string AssignedTo { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Cancelled
        
        public DateTime? DueDate { get; set; }
        
        public DateTime? CompletedDate { get; set; }
        
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class StaffSchedule
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string StaffName { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string ScheduleType { get; set; } = string.Empty; // Patrol, Office, Shift
        
        public DateTime ScheduleDate { get; set; }
        
        public TimeSpan? StartTime { get; set; }
        
        public TimeSpan? EndTime { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
    public class Attendance
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string StaffName { get; set; } = string.Empty;
        
        public DateTime AttendanceDate { get; set; }
        
        public TimeSpan? TimeIn { get; set; }
        
        public TimeSpan? TimeOut { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Present"; // Present, Absent, Late, On Leave
        
        [StringLength(500)]
        public string? Remarks { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
    // Business Permit
    public class BusinessPermit
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string PermitNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string BusinessName { get; set; } = string.Empty;
        
        public int? OwnerResidentId { get; set; }
        
        [StringLength(100)]
        public string? OwnerName { get; set; }
        
        [StringLength(255)]
        public string BusinessAddress { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string BusinessType { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Pre-Assessed, Approved, Renewed, Expired
        
        public DateTime IssueDate { get; set; }
        
        public DateTime ExpiryDate { get; set; }
        
        [StringLength(2000)]
        public string? Requirements { get; set; }
        
        [StringLength(2000)]
        public string? AssessmentNotes { get; set; }
        
        public DateTime? RenewalReminderDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
    
    // Suggestion Box
    public class Suggestion
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Content { get; set; }
        
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Concern, Suggestion, Issue
        
        [StringLength(50)]
        public string Status { get; set; } = "New"; // New, Under Review, Addressed, Closed
        
        [StringLength(100)]
        public string? SubmittedBy { get; set; } // Can be anonymous
        
        public bool IsAnonymous { get; set; } = true;
        
        [StringLength(2000)]
        public string? Response { get; set; }
        
        [StringLength(100)]
        public string? RespondedBy { get; set; }
        
        public DateTime? RespondedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}


