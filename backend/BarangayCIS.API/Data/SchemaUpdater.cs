using System.IO;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace BarangayCIS.API.Data
{
    public static class SchemaUpdater
    {
        public static void EnsureBhwAndSeniorTables(ApplicationDbContext context)
        {
            try
            {
                var scriptPath = ResolveScriptPath();
                if (scriptPath == null)
                {
                    return;
                }

                var scriptContent = File.ReadAllText(scriptPath);
                var batches = Regex.Split(scriptContent, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                foreach (var batch in batches)
                {
                    var commandText = batch.Trim();
                    if (string.IsNullOrWhiteSpace(commandText))
                    {
                        continue;
                    }

                    context.Database.ExecuteSqlRaw(commandText);
                }
            }
            catch
            {
                // swallow exceptions so startup continues even if the script cannot run
            }
        }

        public static void EnsureBHWProfileIdColumn(ApplicationDbContext context)
        {
            try
            {
                var scriptPath = ResolveScriptPath("AddBHWProfileIdToResidents.sql");
                if (scriptPath == null)
                {
                    Console.WriteLine("WARNING: AddBHWProfileIdToResidents.sql script not found. Skipping BHWProfileId column setup.");
                    return;
                }

                var scriptContent = File.ReadAllText(scriptPath);
                var batches = Regex.Split(scriptContent, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                foreach (var batch in batches)
                {
                    var commandText = batch.Trim();
                    if (string.IsNullOrWhiteSpace(commandText))
                    {
                        continue;
                    }

                    context.Database.ExecuteSqlRaw(commandText);
                }
                
                Console.WriteLine("BHWProfileId column setup completed.");
            }
            catch (Exception ex)
            {
                // Log the exception but don't stop startup
                Console.WriteLine($"ERROR: Failed to setup BHWProfileId column: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public static void EnsureDeliveriesAndKRAReportsTables(ApplicationDbContext context)
        {
            try
            {
                // Check if Deliveries table exists by trying to query it
                bool deliveriesTableExists = false;
                try
                {
                    context.Database.ExecuteSqlRaw("SELECT TOP 1 * FROM Deliveries");
                    deliveriesTableExists = true;
                }
                catch
                {
                    deliveriesTableExists = false;
                }

                if (!deliveriesTableExists)
                {
                    Console.WriteLine("Creating Deliveries table...");
                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Deliveries]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [Deliveries] (
                                [Id] int NOT NULL IDENTITY(1,1),
                                [BHWProfileId] int NOT NULL,
                                [MotherName] nvarchar(100) NOT NULL,
                                [ChildName] nvarchar(100) NOT NULL,
                                [PurokSitio] nvarchar(100) NULL,
                                [Gender] nvarchar(10) NOT NULL,
                                [DateOfBirth] datetime2 NOT NULL,
                                [TimeOfBirth] nvarchar(20) NULL,
                                [Weight] nvarchar(20) NULL,
                                [Height] nvarchar(20) NULL,
                                [PlaceOfBirth] nvarchar(255) NULL,
                                [DeliveryType] nvarchar(10) NULL,
                                [BCGAndHepaB] nvarchar(255) NULL,
                                [AttendedBy] nvarchar(255) NULL,
                                [Year] int NOT NULL,
                                [CreatedAt] datetime2 NOT NULL,
                                [UpdatedAt] datetime2 NULL,
                                CONSTRAINT [PK_Deliveries] PRIMARY KEY ([Id]),
                                CONSTRAINT [FK_Deliveries_BHWProfiles_BHWProfileId] FOREIGN KEY ([BHWProfileId]) REFERENCES [BHWProfiles] ([Id]) ON DELETE NO ACTION
                            );
                        END
                    ");
                    Console.WriteLine("Deliveries table created successfully.");
                }

                // Check if KRAReports table exists
                bool kraReportsTableExists = false;
                try
                {
                    context.Database.ExecuteSqlRaw("SELECT TOP 1 * FROM KRAReports");
                    kraReportsTableExists = true;
                }
                catch
                {
                    kraReportsTableExists = false;
                }

                if (!kraReportsTableExists)
                {
                    Console.WriteLine("Creating KRAReports table...");
                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[KRAReports]') AND type in (N'U'))
                        BEGIN
                            CREATE TABLE [KRAReports] (
                                [Id] int NOT NULL IDENTITY(1,1),
                                [BHWProfileId] int NOT NULL,
                                [Year] int NOT NULL,
                                [Month] int NOT NULL,
                                [PillsPOP_10To14] int NOT NULL,
                                [PillsPOP_15To19] int NOT NULL,
                                [PillsPOP_20Plus] int NOT NULL,
                                [PillsCOC_10To14] int NOT NULL,
                                [PillsCOC_15To19] int NOT NULL,
                                [PillsCOC_20Plus] int NOT NULL,
                                [DMPA_10To14] int NOT NULL,
                                [DMPA_15To19] int NOT NULL,
                                [DMPA_20Plus] int NOT NULL,
                                [Condom_10To14] int NOT NULL,
                                [Condom_15To19] int NOT NULL,
                                [Condom_20Plus] int NOT NULL,
                                [Implant_10To14] int NOT NULL,
                                [Implant_15To19] int NOT NULL,
                                [Implant_20Plus] int NOT NULL,
                                [BTL_10To14] int NOT NULL,
                                [BTL_15To19] int NOT NULL,
                                [BTL_20Plus] int NOT NULL,
                                [LAM_10To14] int NOT NULL,
                                [LAM_15To19] int NOT NULL,
                                [LAM_20Plus] int NOT NULL,
                                [IUD_10To14] int NOT NULL,
                                [IUD_15To19] int NOT NULL,
                                [IUD_20Plus] int NOT NULL,
                                [Deliveries_10To14] int NOT NULL,
                                [Deliveries_15To19] int NOT NULL,
                                [Deliveries_20Plus] int NOT NULL,
                                [TeenagePregnancies] int NOT NULL,
                                [Notes] nvarchar(500) NULL,
                                [CreatedAt] datetime2 NOT NULL,
                                [UpdatedAt] datetime2 NULL,
                                CONSTRAINT [PK_KRAReports] PRIMARY KEY ([Id]),
                                CONSTRAINT [FK_KRAReports_BHWProfiles_BHWProfileId] FOREIGN KEY ([BHWProfileId]) REFERENCES [BHWProfiles] ([Id]) ON DELETE NO ACTION
                            );
                        END
                    ");
                    Console.WriteLine("KRAReports table created successfully.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception but don't stop startup
                Console.WriteLine($"ERROR: Failed to create Deliveries/KRAReports tables: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static string? ResolveScriptPath(string? scriptName = null)
        {
            scriptName ??= "AddBhwAndSeniorTables.sql";
            var possiblePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Scripts", scriptName),
                Path.Combine(AppContext.BaseDirectory, "..", "Scripts", scriptName),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "Scripts", scriptName),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Scripts", scriptName),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Scripts", scriptName)
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return Path.GetFullPath(path);
                }
            }

            return null;
        }
    }
}

