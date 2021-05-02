RESTORE DATABASE [vat2_jacoljacol] 
FROM  
DISK = N'/home/sql-server-volume/backups/vat2_jacoljacol.bak' 
WITH  
FILE = 1,  
MOVE N'firma' TO N'/var/opt/mssql/data/vat2_jacoljacol.mdf',  
MOVE N'firma_log' TO N'/var/opt/mssql/data/vat2_jacoljacol.ldf',  
NOUNLOAD,  
REPLACE,  
STATS = 5
go