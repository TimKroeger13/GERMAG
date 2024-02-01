#!/bin/bash
sudo docker build -f Dockerfile --no-cache -t GERMAG:live .
read -r -p "Database Password: " dbPassword 
sudo docker run \
  -d -p 443:443 \
  --name GERMAG-live \
  --env DATABASE_CONNECTION="Host=172.19.0.50:5432;Database=gasag;Username=postgres;Password=$dbPassword" \
  --restart always \
  -v /home/certs:/root/certs \
  --ip 172.19.0.51 \
  --network db-network \
GERMAG:live