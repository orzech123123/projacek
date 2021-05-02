## PROJACEK HQ

dotnet publish -c Release -r linux-x64

chmod -R 777 projack/

cd projack/

./react-app

## DOCKER WAY

docker compose up

in browser: http://localhost/

 docker exec -it sql-server "/home/sql-server-volume/restore.sh"