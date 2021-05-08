## PROJACEK HQ

dotnet publish -c Release -r linux-x64

chmod -R 777 projack/

cd projack/

./react-app

## DOCKER WAY

sudo apt-get install -y docker.io

udo curl -L "https://github.com/docker/compose/releases/download/1.29.1/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose

sudo chmod +x /usr/local/bin/docker-compose

chmod -R 777 ./Docker/volumes/sql-server-volume/

docker compose up

docker exec -it sql-server "/home/sql-server-volume/restore.sh"

in browser: http://localhost/

when debugging via Visual Studio you need to run sql-server on your own (see: VS Docker Tools early stage issues): 
docker exec -it sql-server bash -c "cd /opt/mssql/bin ; ./sqlservr"

## DATABASE

DBCC SHRINKFILE(2,256) - shirnk ldf
