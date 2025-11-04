в контейнере
kafka-topics --list --bootstrap-server kafka:9092
посмотреть список топиков

создать топик:
kafka-topics --create --topic notifications --partitions 1 --replication-factor 1 --bootstrap-server kafka:9092