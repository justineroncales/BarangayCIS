using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffTasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StaffTasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? assignedTo)
        {
            var query = _context.StaffTasks.AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }
            if (!string.IsNullOrEmpty(assignedTo))
            {
                query = query.Where(t => t.AssignedTo == assignedTo);
            }
            var tasks = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _context.StaffTasks.FindAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStaffTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var task = new StaffTask
                {
                    TaskName = dto.TaskName,
                    Description = dto.Description,
                    AssignedTo = dto.AssignedTo,
                    Priority = dto.Priority,
                    Status = dto.Status,
                    DueDate = dto.DueDate,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StaffTasks.Add(task);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the task", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStaffTaskDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var task = await _context.StaffTasks.FindAsync(id);
                if (task == null) return NotFound();

                if (dto.TaskName != null) task.TaskName = dto.TaskName;
                if (dto.Description != null) task.Description = dto.Description;
                if (dto.AssignedTo != null) task.AssignedTo = dto.AssignedTo;
                if (dto.Priority != null) task.Priority = dto.Priority;
                if (dto.Status != null)
                {
                    task.Status = dto.Status;
                    if (dto.Status == "Completed" && !task.CompletedDate.HasValue)
                    {
                        task.CompletedDate = DateTime.UtcNow;
                    }
                }
                if (dto.DueDate.HasValue) task.DueDate = dto.DueDate;
                if (dto.CompletedDate.HasValue) task.CompletedDate = dto.CompletedDate;
                task.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the task", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.StaffTasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.StaffTasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/staff-schedules")]
    [Authorize]
    public class StaffSchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StaffSchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? staffName, [FromQuery] DateTime? date)
        {
            var query = _context.StaffSchedules.AsQueryable();
            if (!string.IsNullOrEmpty(staffName))
            {
                query = query.Where(s => s.StaffName == staffName);
            }
            if (date.HasValue)
            {
                query = query.Where(s => s.ScheduleDate.Date == date.Value.Date);
            }
            var schedules = await query.OrderBy(s => s.ScheduleDate).ThenBy(s => s.StartTime).ToListAsync();
            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var schedule = await _context.StaffSchedules.FindAsync(id);
            if (schedule == null) return NotFound();
            return Ok(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStaffScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var schedule = new StaffSchedule
                {
                    StaffName = dto.StaffName,
                    ScheduleType = dto.ScheduleType,
                    ScheduleDate = dto.ScheduleDate,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Description = dto.Description,
                    Status = dto.Status,
                    CreatedAt = DateTime.UtcNow
                };

                _context.StaffSchedules.Add(schedule);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = schedule.Id }, schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the schedule", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStaffScheduleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var schedule = await _context.StaffSchedules.FindAsync(id);
                if (schedule == null) return NotFound();

                if (dto.StaffName != null) schedule.StaffName = dto.StaffName;
                if (dto.ScheduleType != null) schedule.ScheduleType = dto.ScheduleType;
                if (dto.ScheduleDate.HasValue) schedule.ScheduleDate = dto.ScheduleDate.Value;
                if (dto.StartTime.HasValue) schedule.StartTime = dto.StartTime;
                if (dto.EndTime.HasValue) schedule.EndTime = dto.EndTime;
                if (dto.Description != null) schedule.Description = dto.Description;
                if (dto.Status != null) schedule.Status = dto.Status;

                await _context.SaveChangesAsync();
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the schedule", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.StaffSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            _context.StaffSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/attendance")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? staffName, [FromQuery] DateTime? date)
        {
            var query = _context.Attendances.AsQueryable();
            if (!string.IsNullOrEmpty(staffName))
            {
                query = query.Where(a => a.StaffName == staffName);
            }
            if (date.HasValue)
            {
                query = query.Where(a => a.AttendanceDate.Date == date.Value.Date);
            }
            var attendance = await query.OrderByDescending(a => a.AttendanceDate).ToListAsync();
            return Ok(attendance);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return NotFound();
            return Ok(attendance);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAttendanceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var attendance = new Attendance
                {
                    StaffName = dto.StaffName,
                    AttendanceDate = dto.AttendanceDate,
                    TimeIn = dto.TimeIn,
                    TimeOut = dto.TimeOut,
                    Status = dto.Status,
                    Remarks = dto.Remarks,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = attendance.Id }, attendance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the attendance", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAttendanceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var attendance = await _context.Attendances.FindAsync(id);
                if (attendance == null) return NotFound();

                if (dto.StaffName != null) attendance.StaffName = dto.StaffName;
                if (dto.AttendanceDate.HasValue) attendance.AttendanceDate = dto.AttendanceDate.Value;
                if (dto.TimeIn.HasValue) attendance.TimeIn = dto.TimeIn;
                if (dto.TimeOut.HasValue) attendance.TimeOut = dto.TimeOut;
                if (dto.Status != null) attendance.Status = dto.Status;
                if (dto.Remarks != null) attendance.Remarks = dto.Remarks;

                await _context.SaveChangesAsync();
                return Ok(attendance);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the attendance", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return NotFound();

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

