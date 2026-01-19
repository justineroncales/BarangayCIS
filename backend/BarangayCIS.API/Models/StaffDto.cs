using System.ComponentModel.DataAnnotations;

namespace BarangayCIS.API.Models
{
    public class CreateStaffTaskDto
    {
        [Required]
        [StringLength(255)]
        public string TaskName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(100)]
        public string AssignedTo { get; set; } = string.Empty;

        [StringLength(50)]
        public string Priority { get; set; } = "Normal";

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime? DueDate { get; set; }
    }

    public class UpdateStaffTaskDto
    {
        [StringLength(255)]
        public string? TaskName { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? AssignedTo { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedDate { get; set; }
    }

    public class CreateStaffScheduleDto
    {
        [Required]
        [StringLength(100)]
        public string StaffName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ScheduleType { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduleDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";
    }

    public class UpdateStaffScheduleDto
    {
        [StringLength(100)]
        public string? StaffName { get; set; }

        [StringLength(50)]
        public string? ScheduleType { get; set; }

        public DateTime? ScheduleDate { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }
    }

    public class CreateAttendanceDto
    {
        [Required]
        [StringLength(100)]
        public string StaffName { get; set; } = string.Empty;

        [Required]
        public DateTime AttendanceDate { get; set; }

        public TimeSpan? TimeIn { get; set; }

        public TimeSpan? TimeOut { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Present";

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class UpdateAttendanceDto
    {
        [StringLength(100)]
        public string? StaffName { get; set; }

        public DateTime? AttendanceDate { get; set; }

        public TimeSpan? TimeIn { get; set; }

        public TimeSpan? TimeOut { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}



