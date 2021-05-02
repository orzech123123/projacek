RESTORE DATABASE [WmProjack] 
FROM  
DISK = N'/home/sql-server-volume/backups/WmProjack.bak' 
WITH  
FILE = 1,  
MOVE N'WmProJack' TO N'/var/opt/mssql/data/WmProJack.mdf',  
MOVE N'WmProJack_log' TO N'/var/opt/mssql/data/WmProJack.ldf',  
NOUNLOAD,  
REPLACE,  
STATS = 5
go