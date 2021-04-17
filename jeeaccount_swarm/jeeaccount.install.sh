#!/bin/sh

export DOMAIN=jee.vn
export VAULT_ENDPOINT=http://vault_vault:8200
export VAULT_TOKEN=s.j9o2MvtdSSeTT0dvFH8SAbQ9
export KAFKA_BROKER=kafka1:9999,kafka2:9999,kafka3:9999
export CONNECTION_STRING="Data Source=192.168.199.4,1433;Initial Catalog=JeeAccount;User ID=jeeaccount;Password=Je3@cCountD\$ev"
export PUBLIC_LINK_API=https://jeeaccount-api.jee.vn

docker stack deploy -c ./jeeaccount.swarm.yml --with-registry-auth jeeaccount