#!/bin/bash
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "krynkszta.X" -i /home/sql-server-volume/restore-vat2jacoljacol.sql 
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "krynkszta.X" -i /home/sql-server-volume/restore-wmprojack.sql 