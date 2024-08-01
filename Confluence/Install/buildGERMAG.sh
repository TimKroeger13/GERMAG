#!/bin/bash
sudo docker build -f Dockerfile --no-cache -t germag:live .
read -r -p "Database Password: " dbPassword 
sudo docker rm -f germag-live
sudo docker run \
  -d -p 443:443 \
  --name germag-live \
  --env DATABASE_CONNECTION="Host=172.19.0.50:5432;Database=gasag;Username=postgres;Password=$dbPassword" \
  --restart always \
  -v /etc/letsencrypt/live/tkroeger.com:/root/certs \
  --ip 172.19.0.51 \
  --network db-network \
germag:live
