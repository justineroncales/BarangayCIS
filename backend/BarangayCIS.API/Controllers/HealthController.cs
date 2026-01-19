using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Data;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Controllers
{
    [ApiController]
    [Route("api/medical-records")]
    [Authorize]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicalRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type)
        {
            var query = _context.MedicalRecords.Include(m => m.Resident).AsQueryable();
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(m => m.RecordType == type);
            }
            var records = await query.OrderByDescending(m => m.RecordDate).ToListAsync();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _context.MedicalRecords
                .Include(m => m.Resident)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (record == null) return NotFound();
            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicalRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resident = await _context.Residents.FindAsync(dto.ResidentId);
                if (resident == null)
                {
                    return BadRequest(new { message = "Resident not found" });
                }

                var record = new MedicalRecord
                {
                    ResidentId = dto.ResidentId,
                    RecordType = dto.RecordType,
                    RecordDate = dto.RecordDate,
                    Diagnosis = dto.Diagnosis,
                    Symptoms = dto.Symptoms,
                    Treatment = dto.Treatment,
                    Prescription = dto.Prescription,
                    AttendedBy = dto.AttendedBy,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MedicalRecords.Add(record);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the medical record", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicalRecordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var record = await _context.MedicalRecords.FindAsync(id);
                if (record == null) return NotFound();

                if (dto.ResidentId.HasValue)
                {
                    var resident = await _context.Residents.FindAsync(dto.ResidentId.Value);
                    if (resident == null)
                    {
                        return BadRequest(new { message = "Resident not found" });
                    }
                    record.ResidentId = dto.ResidentId.Value;
                }
                if (dto.RecordType != null) record.RecordType = dto.RecordType;
                if (dto.RecordDate.HasValue) record.RecordDate = dto.RecordDate.Value;
                if (dto.Diagnosis != null) record.Diagnosis = dto.Diagnosis;
                if (dto.Symptoms != null) record.Symptoms = dto.Symptoms;
                if (dto.Treatment != null) record.Treatment = dto.Treatment;
                if (dto.Prescription != null) record.Prescription = dto.Prescription;
                if (dto.AttendedBy != null) record.AttendedBy = dto.AttendedBy;
                if (dto.Notes != null) record.Notes = dto.Notes;

                await _context.SaveChangesAsync();
                return Ok(record);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the medical record", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null) return NotFound();

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/vaccinations")]
    [Authorize]
    public class VaccinationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VaccinationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? type)
        {
            var query = _context.Vaccinations.Include(v => v.Resident).AsQueryable();
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(v => v.VaccineType == type);
            }
            var vaccinations = await query.OrderByDescending(v => v.VaccinationDate).ToListAsync();
            return Ok(vaccinations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vaccination = await _context.Vaccinations
                .Include(v => v.Resident)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vaccination == null) return NotFound();
            return Ok(vaccination);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVaccinationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var resident = await _context.Residents.FindAsync(dto.ResidentId);
                if (resident == null)
                {
                    return BadRequest(new { message = "Resident not found" });
                }

                var vaccination = new Vaccination
                {
                    ResidentId = dto.ResidentId,
                    VaccineName = dto.VaccineName,
                    VaccineType = dto.VaccineType,
                    VaccinationDate = dto.VaccinationDate,
                    DoseNumber = dto.DoseNumber,
                    BatchNumber = dto.BatchNumber,
                    AdministeredBy = dto.AdministeredBy,
                    NextDoseDate = dto.NextDoseDate,
                    Notes = dto.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Vaccinations.Add(vaccination);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = vaccination.Id }, vaccination);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the vaccination", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVaccinationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var vaccination = await _context.Vaccinations.FindAsync(id);
                if (vaccination == null) return NotFound();

                if (dto.ResidentId.HasValue)
                {
                    var resident = await _context.Residents.FindAsync(dto.ResidentId.Value);
                    if (resident == null)
                    {
                        return BadRequest(new { message = "Resident not found" });
                    }
                    vaccination.ResidentId = dto.ResidentId.Value;
                }
                if (dto.VaccineName != null) vaccination.VaccineName = dto.VaccineName;
                if (dto.VaccineType != null) vaccination.VaccineType = dto.VaccineType;
                if (dto.VaccinationDate.HasValue) vaccination.VaccinationDate = dto.VaccinationDate.Value;
                if (dto.DoseNumber != null) vaccination.DoseNumber = dto.DoseNumber;
                if (dto.BatchNumber != null) vaccination.BatchNumber = dto.BatchNumber;
                if (dto.AdministeredBy != null) vaccination.AdministeredBy = dto.AdministeredBy;
                if (dto.NextDoseDate.HasValue) vaccination.NextDoseDate = dto.NextDoseDate;
                if (dto.Notes != null) vaccination.Notes = dto.Notes;

                await _context.SaveChangesAsync();
                return Ok(vaccination);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the vaccination", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vaccination = await _context.Vaccinations.FindAsync(id);
            if (vaccination == null) return NotFound();

            _context.Vaccinations.Remove(vaccination);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    [ApiController]
    [Route("api/medicine-inventory")]
    [Authorize]
    public class MedicineInventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicineInventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status)
        {
            var query = _context.MedicineInventories.AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(m => m.Status == status);
            }
            var medicines = await query.OrderBy(m => m.MedicineName).ToListAsync();
            return Ok(medicines);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var medicine = await _context.MedicineInventories.FindAsync(id);
            if (medicine == null) return NotFound();
            return Ok(medicine);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMedicineInventoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var medicine = new MedicineInventory
                {
                    MedicineName = dto.MedicineName,
                    GenericName = dto.GenericName,
                    Unit = dto.Unit,
                    Quantity = dto.Quantity,
                    MinimumStock = dto.MinimumStock,
                    ExpiryDate = dto.ExpiryDate,
                    Supplier = dto.Supplier,
                    UnitPrice = dto.UnitPrice,
                    Status = dto.Status,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MedicineInventories.Add(medicine);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = medicine.Id }, medicine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the medicine", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicineInventoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var medicine = await _context.MedicineInventories.FindAsync(id);
                if (medicine == null) return NotFound();

                if (dto.MedicineName != null) medicine.MedicineName = dto.MedicineName;
                if (dto.GenericName != null) medicine.GenericName = dto.GenericName;
                if (dto.Unit != null) medicine.Unit = dto.Unit;
                if (dto.Quantity.HasValue) medicine.Quantity = dto.Quantity.Value;
                if (dto.MinimumStock.HasValue) medicine.MinimumStock = dto.MinimumStock;
                if (dto.ExpiryDate.HasValue) medicine.ExpiryDate = dto.ExpiryDate;
                if (dto.Supplier != null) medicine.Supplier = dto.Supplier;
                if (dto.UnitPrice.HasValue) medicine.UnitPrice = dto.UnitPrice;
                if (dto.Status != null) medicine.Status = dto.Status;
                medicine.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(medicine);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the medicine", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var medicine = await _context.MedicineInventories.FindAsync(id);
            if (medicine == null) return NotFound();

            _context.MedicineInventories.Remove(medicine);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

