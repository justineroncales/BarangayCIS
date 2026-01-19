-- Add BHWProfileId column to Residents table
-- This script adds the BHWProfileId foreign key column to link residents with BHW profiles

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

PRINT 'Checking and adding BHWProfileId column to Residents table...';

-- Check if Residents table exists
IF OBJECT_ID(N'[dbo].[Residents]', N'U') IS NULL
BEGIN
    PRINT 'ERROR: Residents table does not exist. Please ensure the database is initialized first.';
    RETURN;
END

-- Check if BHWProfiles table exists (required for foreign key)
IF OBJECT_ID(N'[dbo].[BHWProfiles]', N'U') IS NULL
BEGIN
    PRINT 'WARNING: BHWProfiles table does not exist. Column will be added without foreign key constraint.';
    PRINT 'The foreign key constraint can be added later when BHWProfiles table is created.';
    
    -- Add column without foreign key if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Residents]') AND name = 'BHWProfileId')
    BEGIN
        ALTER TABLE [dbo].[Residents]
        ADD [BHWProfileId] INT NULL;
        
        PRINT 'BHWProfileId column added successfully (without foreign key constraint).';
    END
    ELSE
    BEGIN
        PRINT 'BHWProfileId column already exists.';
    END
    RETURN;
END

-- Check if column doesn't exist before adding
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Residents]') AND name = 'BHWProfileId')
BEGIN
    PRINT 'Adding BHWProfileId column...';
    
    -- Add the BHWProfileId column
    ALTER TABLE [dbo].[Residents]
    ADD [BHWProfileId] INT NULL;

    PRINT 'BHWProfileId column added successfully.';
END
ELSE
BEGIN
    PRINT 'BHWProfileId column already exists.';
END

-- Check if index doesn't exist before creating
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Residents_BHWProfileId' AND object_id = OBJECT_ID('dbo.Residents'))
BEGIN
    PRINT 'Creating index IX_Residents_BHWProfileId...';
    CREATE INDEX [IX_Residents_BHWProfileId] ON [dbo].[Residents] ([BHWProfileId]);
    PRINT 'Index created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_Residents_BHWProfileId already exists.';
END

-- Check if foreign key constraint doesn't exist before adding
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Residents_BHWProfiles_BHWProfileId')
BEGIN
    PRINT 'Adding foreign key constraint...';
    
    -- Add foreign key constraint
    ALTER TABLE [dbo].[Residents]
    ADD CONSTRAINT [FK_Residents_BHWProfiles_BHWProfileId] 
    FOREIGN KEY ([BHWProfileId]) 
    REFERENCES [dbo].[BHWProfiles] ([Id]) 
    ON DELETE SET NULL;

    PRINT 'Foreign key constraint added successfully.';
END
ELSE
BEGIN
    PRINT 'Foreign key constraint FK_Residents_BHWProfiles_BHWProfileId already exists.';
END

PRINT 'BHWProfileId setup completed successfully.';
GO

