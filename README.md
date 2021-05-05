## PROJACEK HQ

dotnet publish -c Release -r linux-x64

chmod -R 777 projack/

cd projack/

./react-app

## DOCKER WAY

docker compose up

in browser: http://localhost/

docker exec -it sql-server "/home/sql-server-volume/restore.sh"

when debugging via Visual Studio you need to run sql-server on your own (see: VS Docker Tools early stage issues):
docker exec -it sql-server bash -c "cd /opt/mssql/bin ; ./sqlservr"

on linux host: chmod -R +x ./Docker/volumes/sql-server-volume/