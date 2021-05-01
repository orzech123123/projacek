## PROJACEK HQ

dotnet publish -c Release -r linux-x64

chmod -R 777 projack/

cd projack/

./react-app

## DOCKER WAY

docker compose up

in browser: http://localhost/

/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'krynkszta.X' -i /home/sql-server-volume/restore.sql