using Microsoft.EntityFrameworkCore;
using BarangayCIS.API.Models;

namespace BarangayCIS.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Core entities
        public DbSet<User> Users { get; set; }
        public DbSet<Resident> Residents { get; set; }
        public DbSet<Household> Households { get; set; }
        
        // Certificate System
        public DbSet<Certificate> Certificates { get; set; }
        
        // Incident & Blotter
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<IncidentAttachment> IncidentAttachments { get; set; }
        
        // Financial
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryBorrowing> InventoryBorrowings { get; set; }
        
        // Projects
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectUpdate> ProjectUpdates { get; set; }
        
        // Health
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Vaccination> Vaccinations { get; set; }
        public DbSet<MedicineInventory> MedicineInventories { get; set; }
        
        // Other modules
        public DbSet<CitizenReport> CitizenReports { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<DisasterMap> DisasterMaps { get; set; }
        public DbSet<Evacuee> Evacuees { get; set; }
        public DbSet<StaffTask> StaffTasks { get; set; }
        public DbSet<StaffSchedule> StaffSchedules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<BusinessPermit> BusinessPermits { get; set; }
        public DbSet<Suggestion> Suggestions { get; set; }
        
        // BHW (Barangay Health Worker)
        public DbSet<BHWProfile> BHWProfiles { get; set; }
        public DbSet<BHWAssignment> BHWAssignments { get; set; }
        public DbSet<BHWVisitLog> BHWVisitLogs { get; set; }
        public DbSet<BHWTraining> BHWTrainings { get; set; }
        public DbSet<BHWIncentive> BHWIncentives { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<KRAReport> KRAReports { get; set; }
        
        // Senior Citizen
        public DbSet<SeniorCitizenID> SeniorCitizenIDs { get; set; }
        public DbSet<SeniorCitizenBenefit> SeniorCitizenBenefits { get; set; }
        public DbSet<SeniorHealthMonitoring> SeniorHealthMonitorings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and indexes
            modelBuilder.Entity<Resident>()
                .HasIndex(r => new { r.LastName, r.FirstName, r.DateOfBirth })
                .IsUnique(false);

            modelBuilder.Entity<Certificate>()
                .HasIndex(c => c.CertificateNumber)
                .IsUnique()
                .HasFilter("[CertificateNumber] IS NOT NULL AND [CertificateNumber] != ''");

            // Configure QR code fields to use nvarchar(max) for large base64 strings
            modelBuilder.Entity<Certificate>()
                .Property(c => c.QRCodeData)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Certificate>()
                .Property(c => c.QRCodeImagePath)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Incident>()
                .HasIndex(i => i.IncidentNumber)
                .IsUnique();

            modelBuilder.Entity<Household>()
                .HasIndex(h => h.HouseholdNumber)
                .IsUnique();

            // Configure Incident relationships with Resident
            // Using NoAction to prevent cascade path cycles in SQL Server
            modelBuilder.Entity<Incident>()
                .HasOne(i => i.Complainant)
                .WithMany()
                .HasForeignKey(i => i.ComplainantId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Incident>()
                .HasOne(i => i.Respondent)
                .WithMany()
                .HasForeignKey(i => i.RespondentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Resident relationships
            modelBuilder.Entity<Resident>()
                .HasOne(r => r.Household)
                .WithMany(h => h.Residents)
                .HasForeignKey(r => r.HouseholdId)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder.Entity<Resident>()
                .HasOne(r => r.BHWProfile)
                .WithMany()
                .HasForeignKey(r => r.BHWProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Certificate relationship
            modelBuilder.Entity<Certificate>()
                .HasOne(c => c.Resident)
                .WithMany(r => r.Certificates)
                .HasForeignKey(c => c.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure MedicalRecord relationship
            modelBuilder.Entity<MedicalRecord>()
                .HasOne(m => m.Resident)
                .WithMany(r => r.MedicalRecords)
                .HasForeignKey(m => m.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Vaccination relationship
            modelBuilder.Entity<Vaccination>()
                .HasOne(v => v.Resident)
                .WithMany()
                .HasForeignKey(v => v.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Evacuee relationships
            modelBuilder.Entity<Evacuee>()
                .HasOne(e => e.Resident)
                .WithMany()
                .HasForeignKey(e => e.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Evacuee>()
                .HasOne(e => e.DisasterMap)
                .WithMany()
                .HasForeignKey(e => e.DisasterMapId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ProjectUpdate relationship
            modelBuilder.Entity<ProjectUpdate>()
                .HasOne(pu => pu.Project)
                .WithMany(p => p.Updates)
                .HasForeignKey(pu => pu.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Expense relationship
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.Budget)
                .WithMany(b => b.Expenses)
                .HasForeignKey(e => e.BudgetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure InventoryBorrowing relationship
            modelBuilder.Entity<InventoryBorrowing>()
                .HasOne(ib => ib.InventoryItem)
                .WithMany(i => i.Borrowings)
                .HasForeignKey(ib => ib.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure IncidentAttachment relationship
            modelBuilder.Entity<IncidentAttachment>()
                .HasOne(ia => ia.Incident)
                .WithMany(i => i.Attachments)
                .HasForeignKey(ia => ia.IncidentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure BHW relationships
            modelBuilder.Entity<BHWProfile>()
                .HasIndex(b => b.BHWNumber)
                .IsUnique()
                .HasFilter("[BHWNumber] IS NOT NULL AND [BHWNumber] != ''");
            
            modelBuilder.Entity<BHWProfile>()
                .HasOne(b => b.Resident)
                .WithMany()
                .HasForeignKey(b => b.ResidentId)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder.Entity<BHWAssignment>()
                .HasOne(a => a.BHWProfile)
                .WithMany(b => b.Assignments)
                .HasForeignKey(a => a.BHWProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<BHWVisitLog>()
                .HasOne(v => v.BHWProfile)
                .WithMany(b => b.VisitLogs)
                .HasForeignKey(v => v.BHWProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<BHWVisitLog>()
                .HasOne(v => v.Resident)
                .WithMany()
                .HasForeignKey(v => v.ResidentId)
                .OnDelete(DeleteBehavior.SetNull);
            
            modelBuilder.Entity<BHWTraining>()
                .HasOne(t => t.BHWProfile)
                .WithMany(b => b.Trainings)
                .HasForeignKey(t => t.BHWProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<BHWIncentive>()
                .HasOne(i => i.BHWProfile)
                .WithMany(b => b.Incentives)
                .HasForeignKey(i => i.BHWProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure Delivery relationship
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.BHWProfile)
                .WithMany()
                .HasForeignKey(d => d.BHWProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure KRAReport relationship
            modelBuilder.Entity<KRAReport>()
                .HasOne(k => k.BHWProfile)
                .WithMany()
                .HasForeignKey(k => k.BHWProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure Senior Citizen relationships
            modelBuilder.Entity<SeniorCitizenID>()
                .HasIndex(s => s.SeniorCitizenNumber)
                .IsUnique()
                .HasFilter("[SeniorCitizenNumber] IS NOT NULL AND [SeniorCitizenNumber] != ''");
            
            modelBuilder.Entity<SeniorCitizenID>()
                .HasOne(s => s.Resident)
                .WithMany()
                .HasForeignKey(s => s.ResidentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<SeniorCitizenBenefit>()
                .HasOne(b => b.SeniorCitizenID)
                .WithMany(s => s.Benefits)
                .HasForeignKey(b => b.SeniorCitizenIDId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<SeniorHealthMonitoring>()
                .HasOne(m => m.SeniorCitizenID)
                .WithMany(s => s.HealthMonitorings)
                .HasForeignKey(m => m.SeniorCitizenIDId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure Delivery relationship with BHWProfile
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.BHWProfile)
                .WithMany()
                .HasForeignKey(d => d.BHWProfileId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Configure KRAReport relationship with BHWProfile
            modelBuilder.Entity<KRAReport>()
                .HasOne(k => k.BHWProfile)
                .WithMany()
                .HasForeignKey(k => k.BHWProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

