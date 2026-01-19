SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

PRINT 'Ensuring BHW and Senior Citizen tables exist...';

IF OBJECT_ID(N'dbo.BHWProfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BHWProfiles (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ResidentId INT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        MiddleName NVARCHAR(100) NULL,
        Suffix NVARCHAR(20) NULL,
        DateOfBirth DATETIME2 NOT NULL,
        Gender NVARCHAR(10) NOT NULL,
        Address NVARCHAR(255) NOT NULL,
        ContactNumber NVARCHAR(20) NULL,
        Email NVARCHAR(100) NULL,
        CivilStatus NVARCHAR(50) NULL,
        EducationalAttainment NVARCHAR(100) NULL,
        BHWNumber NVARCHAR(50) NOT NULL,
        DateAppointed DATETIME2 NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT ('Active'),
        Specialization NVARCHAR(500) NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL
    );

    ALTER TABLE dbo.BHWProfiles
        ADD CONSTRAINT FK_BHWProfiles_Residents
        FOREIGN KEY (ResidentId) REFERENCES dbo.Residents(Id)
        ON DELETE SET NULL;

    CREATE UNIQUE INDEX IX_BHWProfiles_BHWNumber
        ON dbo.BHWProfiles(BHWNumber)
        WHERE BHWNumber IS NOT NULL AND BHWNumber <> '';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BHWProfiles_BHWNumber' AND object_id = OBJECT_ID('dbo.BHWProfiles'))
BEGIN
    CREATE UNIQUE INDEX IX_BHWProfiles_BHWNumber
        ON dbo.BHWProfiles(BHWNumber)
        WHERE BHWNumber IS NOT NULL AND BHWNumber <> '';
END
GO

IF OBJECT_ID(N'dbo.BHWAssignments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BHWAssignments (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        BHWProfileId INT NOT NULL,
        ZoneName NVARCHAR(100) NOT NULL,
        ZoneDescription NVARCHAR(255) NULL,
        CoverageArea NVARCHAR(255) NULL,
        AssignmentDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT ('Active'),
        Notes NVARCHAR(500) NULL,
        AssignedBy NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME())
    );

    ALTER TABLE dbo.BHWAssignments
        ADD CONSTRAINT FK_BHWAssignments_BHWProfiles
        FOREIGN KEY (BHWProfileId) REFERENCES dbo.BHWProfiles(Id)
        ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.BHWVisitLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BHWVisitLogs (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        BHWProfileId INT NOT NULL,
        ResidentId INT NULL,
        VisitedPersonName NVARCHAR(100) NULL,
        Address NVARCHAR(255) NULL,
        VisitDate DATETIME2 NOT NULL,
        VisitType NVARCHAR(50) NOT NULL,
        VisitPurpose NVARCHAR(2000) NULL,
        Findings NVARCHAR(2000) NULL,
        ActionsTaken NVARCHAR(2000) NULL,
        Recommendations NVARCHAR(2000) NULL,
        ReferralStatus NVARCHAR(50) NULL,
        ReferralNotes NVARCHAR(500) NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME())
    );

    ALTER TABLE dbo.BHWVisitLogs
        ADD CONSTRAINT FK_BHWVisitLogs_BHWProfiles
        FOREIGN KEY (BHWProfileId) REFERENCES dbo.BHWProfiles(Id)
        ON DELETE NO ACTION;

    ALTER TABLE dbo.BHWVisitLogs
        ADD CONSTRAINT FK_BHWVisitLogs_Residents
        FOREIGN KEY (ResidentId) REFERENCES dbo.Residents(Id)
        ON DELETE SET NULL;
END
GO

IF OBJECT_ID(N'dbo.BHWTrainings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BHWTrainings (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        BHWProfileId INT NOT NULL,
        TrainingTitle NVARCHAR(255) NOT NULL,
        Description NVARCHAR(2000) NULL,
        TrainingProvider NVARCHAR(100) NULL,
        TrainingDate DATETIME2 NOT NULL,
        TrainingEndDate DATETIME2 NULL,
        TrainingType NVARCHAR(50) NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT ('Completed'),
        CertificateNumber NVARCHAR(100) NULL,
        CertificatePath NVARCHAR(500) NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME())
    );

    ALTER TABLE dbo.BHWTrainings
        ADD CONSTRAINT FK_BHWTrainings_BHWProfiles
        FOREIGN KEY (BHWProfileId) REFERENCES dbo.BHWProfiles(Id)
        ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.BHWIncentives', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BHWIncentives (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        BHWProfileId INT NOT NULL,
        IncentiveType NVARCHAR(50) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        IncentiveDate DATETIME2 NOT NULL,
        PaymentStatus NVARCHAR(50) NOT NULL DEFAULT ('Pending'),
        PaymentDate DATETIME2 NULL,
        PaymentMethod NVARCHAR(100) NULL,
        ReferenceNumber NVARCHAR(200) NULL,
        Remarks NVARCHAR(500) NULL,
        ProcessedBy NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME())
    );

    ALTER TABLE dbo.BHWIncentives
        ADD CONSTRAINT FK_BHWIncentives_BHWProfiles
        FOREIGN KEY (BHWProfileId) REFERENCES dbo.BHWProfiles(Id)
        ON DELETE NO ACTION;
END
GO

IF OBJECT_ID(N'dbo.SeniorCitizenIDs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SeniorCitizenIDs (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ResidentId INT NOT NULL,
        SeniorCitizenNumber NVARCHAR(50) NOT NULL,
        ApplicationDate DATETIME2 NOT NULL,
        IssueDate DATETIME2 NULL,
        ExpiryDate DATETIME2 NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT ('Pending'),
        RequirementsSubmitted NVARCHAR(500) NULL,
        RequirementsMissing NVARCHAR(500) NULL,
        Remarks NVARCHAR(500) NULL,
        ProcessedBy NVARCHAR(100) NULL,
        LastValidatedDate DATETIME2 NULL,
        NextValidationDate DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL
    );

    ALTER TABLE dbo.SeniorCitizenIDs
        ADD CONSTRAINT FK_SeniorCitizenIDs_Residents
        FOREIGN KEY (ResidentId) REFERENCES dbo.Residents(Id)
        ON DELETE NO ACTION;

    CREATE UNIQUE INDEX IX_SeniorCitizenIDs_Number
        ON dbo.SeniorCitizenIDs(SeniorCitizenNumber)
        WHERE SeniorCitizenNumber IS NOT NULL AND SeniorCitizenNumber <> '';
END
GO

IF OBJECT_ID(N'dbo.SeniorCitizenBenefits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SeniorCitizenBenefits (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SeniorCitizenIDId INT NOT NULL,
        BenefitType NVARCHAR(50) NOT NULL,
        BenefitDescription NVARCHAR(255) NULL,
        Amount DECIMAL(18,2) NULL,
        BenefitDate DATETIME2 NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT ('Pending'),
        Requirements NVARCHAR(2000) NULL,
        Notes NVARCHAR(2000) NULL,
        ProcessedBy NVARCHAR(100) NULL,
        ProcessedDate DATETIME2 NULL,
        ReferenceNumber NVARCHAR(200) NULL,
        PaymentMethod NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NULL
    );

    ALTER TABLE dbo.SeniorCitizenBenefits
        ADD CONSTRAINT FK_SeniorCitizenBenefits_SeniorCitizenIDs
        FOREIGN KEY (SeniorCitizenIDId) REFERENCES dbo.SeniorCitizenIDs(Id)
        ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.SeniorHealthMonitorings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SeniorHealthMonitorings (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SeniorCitizenIDId INT NOT NULL,
        MonitoringDate DATETIME2 NOT NULL,
        MonitoringType NVARCHAR(50) NOT NULL,
        BloodPressure NVARCHAR(100) NULL,
        BloodSugar NVARCHAR(50) NULL,
        Weight NVARCHAR(50) NULL,
        Height NVARCHAR(50) NULL,
        BMI NVARCHAR(50) NULL,
        HealthFindings NVARCHAR(2000) NULL,
        Complaints NVARCHAR(2000) NULL,
        Medications NVARCHAR(2000) NULL,
        Recommendations NVARCHAR(2000) NULL,
        ReferralStatus NVARCHAR(50) NULL,
        ReferralNotes NVARCHAR(500) NULL,
        AttendedBy NVARCHAR(100) NULL,
        Notes NVARCHAR(500) NULL,
        NextCheckupDate DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT (SYSUTCDATETIME())
    );

    ALTER TABLE dbo.SeniorHealthMonitorings
        ADD CONSTRAINT FK_SeniorHealthMonitorings_SeniorCitizenIDs
        FOREIGN KEY (SeniorCitizenIDId) REFERENCES dbo.SeniorCitizenIDs(Id)
        ON DELETE CASCADE;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SeniorCitizenIDs_Number' AND object_id = OBJECT_ID('dbo.SeniorCitizenIDs'))
BEGIN
    CREATE UNIQUE INDEX IX_SeniorCitizenIDs_Number
        ON dbo.SeniorCitizenIDs(SeniorCitizenNumber)
        WHERE SeniorCitizenNumber IS NOT NULL AND SeniorCitizenNumber <> '';
END
GO

PRINT 'BHW and Senior Citizen tables are ready.';

