BACKUP DATABASE [BarangayCIS] 
TO DISK = 'D:\brgy\database-backup\BarangayCIS.bak' 
WITH FORMAT, INIT, 
NAME = 'Full Backup of BarangayCIS', 
SKIP, NOREWIND, NOUNLOAD, STATS = 10
