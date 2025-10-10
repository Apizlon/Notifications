# Notifications

# Tools
## Swagger
Если захочется потестить в swagger-ui, в поле вставки токена надо написать именно 'Bearer {token}', а не просто токен

## CLI PostgreSQL
Подключение к CLI:
`docker exec -it <имя_контейнера> psql -U postgres`

В Docker Desktop:
`psql -U postgres`

Получение списка баз данных:
`\l` или `\list`
`\l+` для более детальной информации

Подключение к базе данных:
`\c {имя базы данных}`

Просмотр таблиц
`\dt`
`\dt+` для более детальной информации

Получение данных:
```sql
SELECT * FROM users;
```
Одной командой:
`docker exec -it <имя_контейнера> psql -U <пользователь> -d <имя_базы> -c "<SQL-запрос>"`

В Docker Desktop:
`psql -U <пользователь> -d <имя_базы> -c "<SQL-запрос>"`