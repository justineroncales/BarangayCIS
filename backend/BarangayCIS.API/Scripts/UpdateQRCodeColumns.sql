-- Script to update QRCode columns to support larger base64 strings
-- Run this if you get "String or binary data would be truncated" errors

USE BarangayCIS;
GO

-- Update QRCodeData column
ALTER TABLE [Certificates]
ALTER COLUMN [QRCodeData] NVARCHAR(MAX) NULL;
GO

-- Update QRCodeImagePath column  
ALTER TABLE [Certificates]
ALTER COLUMN [QRCodeImagePath] NVARCHAR(MAX) NULL;
GO

PRINT 'QRCode columns updated successfully!';
GO


