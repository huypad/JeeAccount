#!/bin/sh

export DOMAIN=jee.vn
export VAULT_ENDPOINT=http://vault_vault:8200
export VAULT_TOKEN=s.CnO0sRfOCIvMAYcSnoceE1A5
export KAFKA_BROKER=kafka1:9999,kafka2:9999,kafka3:9999
export CONNECTION_STRING="Data Source=192.168.199.4,1433;Initial Catalog=JeeAccount;User ID=jeeaccount;Password=Je3@cCountD\$ev"

docker stack deploy -c ./jeeaccount.swarm.yml --with-registry-auth jeeaccount